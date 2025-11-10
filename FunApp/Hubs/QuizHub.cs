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
            await Clients.All.SendAsync("UserJoined", user);
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

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _quizService.RemoveUser(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}