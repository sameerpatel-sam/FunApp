using Microsoft.AspNetCore.SignalR;
using FunApp.Models;
using FunApp.Services;

namespace FunApp.Hubs
{
    public class QuizHub : Hub
    {
        private readonly QuizService _quizService;
        private readonly ILogger<QuizHub> _logger;

        public QuizHub(QuizService quizService, ILogger<QuizHub> logger)
        {
            _quizService = quizService;
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

            await Clients.All.SendAsync("UserJoined", user);
            _logger.LogInformation("User '{UserName}' joined the quiz", userName);
        }

        public async Task SubmitAnswer(string answer)
        {
            var userAnswer = _quizService.SubmitAnswer(Context.ConnectionId, answer);
            await Clients.All.SendAsync("AnswerReceived", userAnswer);
        }

        public async Task ReportVisibilityChange(int switchCount)
        {
            var user = _quizService.GetUser(Context.ConnectionId);
            if (user != null)
            {
                var newCount = _quizService.IncrementSwitchCount(Context.ConnectionId);
                
                _logger.LogWarning("User {UserName} switched away from quiz. Server switch count: {SwitchCount}", 
                    user.Name, newCount);
                
                // Notify quiz host about the switch
                await Clients.Others.SendAsync("UserSwitchedAway", new { 
                    UserName = user.Name, 
                    SwitchCount = newCount 
                });
            }
        }

        public async Task NextQuestion()
        {
            var question = _quizService.GetNextQuestion();
            _quizService.ClearAnswers();
            await Clients.All.SendAsync("NewQuestion", question);
        }

        public Task<List<dynamic>> GetCurrentAnswers()
        {
            var answers = _quizService.GetCurrentAnswers();
            return Task.FromResult(answers.Select(a => new { a.User, a.Answer }).Cast<dynamic>().ToList());
        }

        public Task<List<dynamic>> GetAllUserAnswers()
        {
            var allUsers = _quizService.GetAllUsers();
            var userAnswersDict = _quizService.GetAllUserAnswers();

            var results = allUsers.Select(user => 
            {
                List<object> answers;
                if (userAnswersDict.ContainsKey(user.ConnectionId))
                {
                    answers = userAnswersDict[user.ConnectionId]
                        .Select(a => (object)new { Answer = a })
                        .ToList();
                }
                else
                {
                    answers = new List<object>();
                }

                return (object)new
                {
                    user.Name,
                    user.SwitchCount,
                    Answers = answers
                };
            }).ToList();

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

        public Task<List<Question>> GetQuestions(string mode)
        {
            if (Enum.TryParse<GameMode>(mode, out var gameMode))
            {
                return Task.FromResult(_quizService.GetQuestionsByMode(gameMode));
            }
            return Task.FromResult(new List<Question>());
        }

        public async Task AddQuestion(string text, string mode)
        {
            if (Enum.TryParse<GameMode>(mode, out var gameMode))
            {
                _quizService.AddQuestion(text, gameMode);
                var questions = _quizService.GetQuestionsByMode(gameMode);
                await Clients.All.SendAsync("QuestionsUpdated", gameMode.ToString(), questions);
                _logger.LogInformation("Question added: {Text}", text);
            }
        }

        public async Task UpdateQuestion(int id, string text)
        {
            _quizService.UpdateQuestion(id, text);
            var gameMode = _quizService.GetGameMode();
            var questions = _quizService.GetQuestionsByMode(gameMode);
            await Clients.All.SendAsync("QuestionsUpdated", gameMode.ToString(), questions);
            _logger.LogInformation("Question {Id} updated", id);
        }

        public async Task DeleteQuestion(int id)
        {
            _quizService.DeleteQuestion(id);
            var gameMode = _quizService.GetGameMode();
            var questions = _quizService.GetQuestionsByMode(gameMode);
            await Clients.All.SendAsync("QuestionsUpdated", gameMode.ToString(), questions);
            _logger.LogInformation("Question {Id} deleted", id);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _quizService.RemoveUser(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}