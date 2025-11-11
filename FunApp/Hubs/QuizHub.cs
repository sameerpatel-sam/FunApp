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
                // User already joined with this name
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

        // ReportVisibilityChange no longer trusts client-provided counts; increment on the server
        public async Task ReportVisibilityChange(int _ignoredSwitchCount)
        {
            var user = _quizService.GetUser(Context.ConnectionId);
            if (user != null)
            {
                var newCount = _quizService.IncrementSwitchCount(Context.ConnectionId);

                _logger.LogWarning("User {UserName} switched away from quiz. Server switch count: {SwitchCount}",
                    user.Name, newCount);

                // Notify quiz host about the switch with server-side count
                await Clients.Others.SendAsync("UserSwitchedAway", new
                {
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

        // New method: Display quiz results/summary
        public async Task ShowResults()
        {
            var allUsers = _quizService.GetAllUsers();
            var userAnswersDict = _quizService.GetAllUserAnswers();

            var results = allUsers.Select(user => 
            {
                List<object> answers = new();
                
                // Get answers for this user
                if (userAnswersDict.ContainsKey(user.ConnectionId))
                {
                    var userAnswers = userAnswersDict[user.ConnectionId];
                    answers = userAnswers
                        .Select(answer => (object)new { Answer = answer })
                        .ToList();
                }

                return new
                {
                    user.Name,
                    user.SwitchCount,
                    Answers = answers
                };
            }).ToList();

            _logger.LogInformation("ShowResults: Sending {ParticipantCount} participants with results", results.Count);
            foreach (var r in results)
            {
                _logger.LogInformation("  {Name}: {AnswerCount} answers", r.Name, r.Answers.Count);
            }

            await Clients.All.SendAsync("QuizResults", new
            {
                Results = results,
                TotalQuestions = _quizService.GetCurrentQuestionNumber()
            });

            _logger.LogInformation("Quiz results displayed with {ParticipantCount} participants", results.Count);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _quizService.RemoveUser(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}