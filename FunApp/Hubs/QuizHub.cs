using Microsoft.AspNetCore.SignalR;
using FunApp.Models;
using FunApp.Services;

namespace FunApp.Hubs
{
    public class QuizHub : Hub
    {
        private readonly QuizService _quizService;

        public QuizHub(QuizService quizService)
        {
            _quizService = quizService;
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