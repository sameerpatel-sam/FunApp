using FunApp.Models;

namespace FunApp.Services
{
    public class QuizService
    {
        private readonly Dictionary<string, User> _users = new();
        private readonly Dictionary<string, UserAnswer> _answers = new();
        private readonly List<string> _questions = new()
        {
            "What's your favorite color?",
            "If you could be any animal, what would you be?",
            "What's your dream vacation destination?",
            "What superpower would you choose?",
            "What's your favorite food?"
        };
        private int _currentQuestionIndex = -1;

        public User AddUser(string connectionId, string name)
        {
            var user = new User { ConnectionId = connectionId, Name = name };
            _users[connectionId] = user;
            return user;
        }

        public void RemoveUser(string connectionId)
        {
            _users.Remove(connectionId);
            _answers.Remove(connectionId);
        }

        public UserAnswer SubmitAnswer(string connectionId, string answer)
        {
            if (!_users.ContainsKey(connectionId)) return null!;
            
            var userAnswer = new UserAnswer 
            { 
                User = _users[connectionId], 
                Answer = answer 
            };
            _answers[connectionId] = userAnswer;
            return userAnswer;
        }

        public string GetNextQuestion()
        {
            _currentQuestionIndex = (_currentQuestionIndex + 1) % _questions.Count;
            return _questions[_currentQuestionIndex];
        }

        public void ClearAnswers()
        {
            _answers.Clear();
        }

        public IEnumerable<UserAnswer> GetCurrentAnswers()
        {
            return _answers.Values;
        }
    }
}