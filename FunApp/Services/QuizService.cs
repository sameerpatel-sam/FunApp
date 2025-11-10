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
        private readonly object _lock = new();

        public User? GetUser(string connectionId)
        {
            return _users.TryGetValue(connectionId, out var user) ? user : null;
        }

        public User AddUser(string connectionId, string name)
        {
            var user = new User { ConnectionId = connectionId, Name = name };
            lock (_lock)
            {
                _users[connectionId] = user;
            }
            return user;
        }

        public void RemoveUser(string connectionId)
        {
            lock (_lock)
            {
                _users.Remove(connectionId);
                _answers.Remove(connectionId);
            }
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

        // Increment and return the number of times the user switched away
        public int IncrementSwitchCount(string connectionId)
        {
            lock (_lock)
            {
                if (_users.TryGetValue(connectionId, out var user))
                {
                    user.SwitchCount++;
                    return user.SwitchCount;
                }
            }
            return 0;
        }

        // Optional: get a snapshot of current users
        public IEnumerable<User> GetAllUsers()
        {
            lock (_lock)
            {
                return _users.Values.ToList();
            }
        }
    }
}