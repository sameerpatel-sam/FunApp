using Microsoft.AspNetCore.SignalR;
using FunApp.Models;
using FunApp.Services;

namespace FunApp.Hubs
{
    public class QuizHub : Hub
    {
        private readonly QuizService _quizService;
        private readonly PersistentQuizService _persistent;
        private readonly ILogger<QuizHub> _logger;

        public QuizHub(QuizService quizService, PersistentQuizService persistent, ILogger<QuizHub> logger)
        {
            _quizService = quizService;
            _persistent = persistent;
            _logger = logger;
        }

        public async Task JoinQuiz(string userName)
        {
            var user = _quizService.AddUser(Context.ConnectionId, userName);
            if (user == null)
            {
                await Clients.Caller.SendAsync("JoinFailed", "This username is already in the quiz. Please use a different name.");
                _logger.LogWarning("Join attempt failed: username '{UserName}' already in quiz", userName);
                return;
            }
            await _persistent.EnsureSessionAsync();
            await Clients.All.SendAsync("UserJoined", user);
            _logger.LogInformation("User '{UserName}' joined the quiz", userName);
        }

        public Task<List<User>> GetParticipants()
        {
            var users = _quizService.GetAllUsers().ToList();
            _logger.LogInformation("GetParticipants -> {Count}", users.Count);
            return Task.FromResult(users);
        }

        public async Task SubmitAnswer(string answer)
        {
            var userAnswer = _quizService.SubmitAnswer(Context.ConnectionId, answer);
            if (userAnswer != null)
            {
                var qId = _quizService.GetCurrentQuestionId() ?? 0;
                await _persistent.AddResponseAsync(userAnswer.User.Name, qId, userAnswer.Answer);
            }
            await Clients.All.SendAsync("AnswerReceived", userAnswer);
        }

        public async Task ReportVisibilityChange(int switchCount)
        {
            var user = _quizService.GetUser(Context.ConnectionId) ?? _quizService.GetArchivedUser(Context.ConnectionId);
            if (user != null)
            {
                var newCount = _quizService.IncrementSwitchCount(Context.ConnectionId);
                _logger.LogWarning("User {UserName} switched away from quiz. Server switch count: {SwitchCount}", user.Name, newCount);
                await Clients.Others.SendAsync("UserSwitchedAway", new { UserName = user.Name, SwitchCount = newCount });
            }
        }

        public async Task NextQuestion()
        {
            try
            {
                await _persistent.EnsureSessionAsync();
                // Get next question from persisted store to ensure valid QuestionId
                var mode = _quizService.GetGameMode();
                var list = await _persistent.GetQuestionsAsync(mode);
                if (list.Count == 0)
                {
                    await Clients.All.SendAsync("NewQuestion", "No questions available for this game mode.");
                    return;
                }
                var idx = _quizService.AdvanceIndex(list.Count);
                var q = list[idx];
                _quizService.SetCurrentQuestionId(q.Id);
                _quizService.ClearAnswers();
                await Clients.All.SendAsync("NewQuestion", q.Text);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NextQuestion failed");
                throw new HubException($"NextQuestion failed: {ex.Message}");
            }
        }

        public Task<List<dynamic>> GetCurrentAnswers()
        {
            var answers = _quizService.GetCurrentAnswers();
            return Task.FromResult(answers.Select(a => new { a.User, a.Answer }).Cast<dynamic>().ToList());
        }

        public Task<List<dynamic>> GetAllUserAnswers()
        {
            var answersDict = _quizService.GetAllUserAnswers();
            var activeUsers = _quizService.GetAllUsers().ToDictionary(u => u.ConnectionId, u => u);
            var allConnectionIds = answersDict.Keys.Union(activeUsers.Keys).Distinct();

            var results = new List<object>();
            foreach (var cid in allConnectionIds)
            {
                var user = activeUsers.ContainsKey(cid) ? activeUsers[cid] : _quizService.GetArchivedUser(cid);
                if (user == null) continue;

                List<object> answers = answersDict.ContainsKey(cid)
                    ? answersDict[cid].Select(a => (object)new { Answer = a }).ToList()
                    : new List<object>();

                results.Add(new
                {
                    Name = user.Name,
                    user.SwitchCount,
                    Answers = answers
                });
            }

            return Task.FromResult(results.Cast<dynamic>().ToList());
        }

        public async Task SetGameMode(string mode)
        {
            if (Enum.TryParse<GameMode>(mode, out var gameMode))
            {
                _quizService.SetGameMode(gameMode);
                await Clients.All.SendAsync("GameModeChanged", mode);
                _logger.LogInformation("Game mode set to: {GameMode}", mode);
            }
        }

        public async Task<List<Question>> GetQuestions(string mode)
        {
            if (Enum.TryParse<GameMode>(mode, out var gameMode))
            {
                return await _persistent.GetQuestionsAsync(gameMode);
            }
            return new List<Question>();
        }

        public async Task AddQuestion(string text, string mode)
        {
            if (Enum.TryParse<GameMode>(mode, out var gameMode))
            {
                await _persistent.AddQuestionAsync(text, gameMode);
                var questions = await _persistent.GetQuestionsAsync(gameMode);
                await Clients.All.SendAsync("QuestionsUpdated", gameMode.ToString(), questions);
                _logger.LogInformation("Question added: {Text}", text);
            }
        }

        public async Task UpdateQuestion(int id, string text)
        {
            await _persistent.UpdateQuestionAsync(id, text);
            var gameMode = _quizService.GetGameMode();
            var questions = await _persistent.GetQuestionsAsync(gameMode);
            await Clients.All.SendAsync("QuestionsUpdated", gameMode.ToString(), questions);
            _logger.LogInformation("Question {Id} updated", id);
        }

        public async Task DeleteQuestion(int id)
        {
            // find the question to determine its mode for correct UI refresh
            var existing = await _persistent.FindQuestionAsync(id);
            var mode = existing?.GameMode ?? _quizService.GetGameMode();

            await _persistent.DeleteQuestionAsync(id);
            var questions = await _persistent.GetQuestionsAsync(mode);
            await Clients.All.SendAsync("QuestionsUpdated", mode.ToString(), questions);
            _logger.LogInformation("Question {Id} deleted (mode {Mode})", id, mode);
        }

        public async Task EndQuiz()
        {
            var results = await GetAllUserAnswers();
            await Clients.All.SendAsync("QuizEnded", results);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = _quizService.GetUser(Context.ConnectionId);
            _quizService.RemoveUser(Context.ConnectionId);
            if (user != null)
            {
                await Clients.All.SendAsync("UserLeft", new { connectionId = user.ConnectionId, name = user.Name });
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}