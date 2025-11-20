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
            
            // Log couple pairing if in couple mode
            if (_quizService.GetGameMode() == GameMode.Couple && !string.IsNullOrEmpty(user.LastName))
            {
                var couples = _quizService.GetCouplesByLastName();
                if (couples.ContainsKey(user.LastName.ToLower()) && couples[user.LastName.ToLower()].Count == 2)
                {
                    _logger.LogInformation("Couple formed: Mr & Mrs {LastName}", user.LastName);
                    await Clients.All.SendAsync("CoupleFormed", new { LastName = user.LastName });
                }
            }
            
            _logger.LogInformation("User '{UserName}' joined the quiz (FirstName: {FirstName}, LastName: {LastName})", 
                userName, user.FirstName, user.LastName);
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
                
                // Check if couple mode and both partners have answered
                if (_quizService.GetGameMode() == GameMode.Couple && !string.IsNullOrEmpty(userAnswer.User.LastName))
                {
                    var couples = _quizService.GetCouplesByLastName();
                    var lastName = userAnswer.User.LastName.ToLower();
                    
                    if (couples.ContainsKey(lastName))
                    {
                        var partners = couples[lastName];
                        var currentAnswers = _quizService.GetCurrentAnswers().ToList();
                        var bothAnswered = partners.All(p => currentAnswers.Any(ca => ca.User.ConnectionId == p.ConnectionId));
                        
                        if (bothAnswered)
                        {
                            await Clients.All.SendAsync("CoupleAnswered", new { LastName = userAnswer.User.LastName });
                        }
                    }
                }
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
                // Evaluate couple answers BEFORE clearing and BEFORE moving to next question
                if (_quizService.GetGameMode() == GameMode.Couple && _quizService.GetCurrentQuestionId() != null)
                {
                    await EvaluateCoupleAnswersForCurrentQuestion();
                }
                
                await _persistent.EnsureSessionAsync();
                var mode = _quizService.GetGameMode();
                var list = await _persistent.GetQuestionsAsync(mode);
                if (list.Count == 0)
                {
                    await Clients.All.SendAsync("NewQuestion", "No questions available for this game mode.");
                    return;
                }
                
                // Clear answers AFTER evaluation
                _quizService.ClearAnswers();
                
                // Check if we've reached the end of questions
                var currentIndex = _quizService.GetCurrentQuestionNumber() - 1; // 0-based
                var isLastQuestion = currentIndex >= list.Count - 1;
                
                if (isLastQuestion)
                {
                    // We've shown all questions, display end game message
                    if (mode == GameMode.Couple)
                    {
                        await Clients.All.SendAsync("GameOver", "?? Game is now over! Let's check which couple has stolen today's show! Click 'Show Results' to see the winners! ??");
                        _logger.LogInformation("All questions completed. Game over message sent.");
                    }
                    else
                    {
                        await Clients.All.SendAsync("GameOver", "?? Game is now over! Click 'Show Results' to see who won! ??");
                        _logger.LogInformation("All questions completed. Game over message sent.");
                    }
                    // Loop back to first question
                    _quizService.AdvanceIndex(list.Count); // This will reset to 0
                }
                
                // Now advance to next question
                var idx = _quizService.AdvanceIndex(list.Count);
                var q = list[idx];
                _quizService.SetCurrentQuestionId(q.Id);
                
                await Clients.All.SendAsync("NewQuestion", q.Text);
                
                _logger.LogInformation("Advanced to question {QuestionNumber}/{TotalQuestions}: {QuestionText}", 
                    _quizService.GetCurrentQuestionNumber(), list.Count, q.Text);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NextQuestion failed");
                throw new HubException($"NextQuestion failed: {ex.Message}");
            }
        }

        private async Task EvaluateCoupleAnswersForCurrentQuestion()
        {
            var evaluations = _quizService.EvaluateCoupleAnswers();
            var questionId = _quizService.GetCurrentQuestionId() ?? 0;
            
            foreach (var (lastNameKey, partner1Answer, partner2Answer, matched, properLastName) in evaluations)
            {
                // Use properLastName (with correct case) for database storage
                await _persistent.SaveCoupleScoreAsync(properLastName, questionId, matched, partner1Answer, partner2Answer);
                _logger.LogInformation("Couple {LastName}: Answers {Status} ('{Answer1}' vs '{Answer2}')", 
                    properLastName, matched ? "MATCHED" : "did not match", partner1Answer, partner2Answer);
            }
        }

        public Task<List<dynamic>> GetCurrentAnswers()
        {
            var answers = _quizService.GetCurrentAnswers();
            return Task.FromResult(answers.Select(a => new { a.User, a.Answer }).Cast<dynamic>().ToList());
        }

        public async Task<List<dynamic>> GetAllUserAnswers()
        {
            var gameMode = _quizService.GetGameMode();
            
            if (gameMode == GameMode.Couple)
            {
                // Evaluate current question's answers if not yet evaluated
                if (_quizService.GetCurrentQuestionId() != null)
                {
                    var currentAnswers = _quizService.GetCurrentAnswers().ToList();
                    if (currentAnswers.Count > 0)
                    {
                        _logger.LogInformation("Evaluating final question's answers before showing results...");
                        await EvaluateCoupleAnswersForCurrentQuestion();
                    }
                }
                
                // Get current session ID
                var sessionId = _persistent.GetCurrentSessionId();
                if (!sessionId.HasValue)
                {
                    _logger.LogWarning("No active session ID found!");
                    return new List<dynamic>();
                }
                
                _logger.LogInformation("Getting couple scores from database for session {SessionId}", sessionId.Value);
                
                // Get couple scores DIRECTLY from database - this is the source of truth
                var dbScores = await _persistent.GetCoupleTotalScoresAsync(sessionId.Value);
                
                _logger.LogInformation("Found {Count} couples in database", dbScores.Count);
                
                // Create result list - ONLY Name and Score, nothing else
                var results = new List<dynamic>();
                
                foreach (var (lastName, score) in dbScores)
                {
                    _logger.LogInformation("Couple {LastName}: Score from DB = {Score}", lastName, score);
                    
                    // Create a dictionary with ONLY the two properties we need
                    var coupleResult = new Dictionary<string, object>
                    {
                        { "Name", $"Mr & Mrs {lastName}" },
                        { "Score", score }
                    };
                    
                    results.Add(coupleResult);
                    
                    // Log what we're actually sending
                    _logger.LogInformation("Adding to results: Name='{Name}', Score={Score}", 
                        coupleResult["Name"], coupleResult["Score"]);
                }
                
                _logger.LogInformation("Returning {Count} couple results to UI", results.Count);
                return results;
            }
            else
            {
                // Individual mode - existing logic
                var answersDict = _quizService.GetAllUserAnswers();
                var activeUsers = _quizService.GetAllUsers().ToDictionary(u => u.ConnectionId, u => u);
                var allConnectionIds = answersDict.Keys.Union(activeUsers.Keys).Distinct();

                var results = new List<dynamic>();
                foreach (var cid in allConnectionIds)
                {
                    var user = activeUsers.ContainsKey(cid) ? activeUsers[cid] : _quizService.GetArchivedUser(cid);
                    if (user == null) continue;

                    List<object> answers = answersDict.ContainsKey(cid)
                        ? answersDict[cid].Select(a => (object)new { Answer = a }).ToList()
                        : new List<object>();

                    results.Add(new Dictionary<string, object>
                    {
                        { "Name", user.Name },
                        { "SwitchCount", user.SwitchCount },
                        { "Answers", answers }
                    });
                }

                return results;
            }
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
            await _persistent.DeleteQuestionAsync(id);
            var gameMode = _quizService.GetGameMode();
            var questions = await _persistent.GetQuestionsAsync(gameMode);
            await Clients.All.SendAsync("QuestionsUpdated", gameMode.ToString(), questions);
            _logger.LogInformation("Question {Id} deleted", id);
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