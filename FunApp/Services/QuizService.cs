using FunApp.Models;

namespace FunApp.Services
{
    public class QuizService
    {
        private readonly Dictionary<string, User> _users = new();
        private readonly Dictionary<string, List<string>> _allAnswers = new(); // Track all answers per user
        private readonly Dictionary<string, UserAnswer> _currentAnswers = new(); // Current question answers
        private readonly HashSet<string> _joinedUsernames = new();
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
            lock (_lock)
            {
                return _users.TryGetValue(connectionId, out var user) ? user : null;
            }
        }

        public User? AddUser(string connectionId, string name)
        {
            lock (_lock)
            {
                var normalizedName = name.Trim().ToLower();

                if (_users.ContainsKey(connectionId))
                {
                    return null;
                }

                if (_joinedUsernames.Contains(normalizedName))
                {
                    return null;
                }

                var user = new User { ConnectionId = connectionId, Name = name.Trim() };
                _users[connectionId] = user;
                _joinedUsernames.Add(normalizedName);
                _allAnswers[connectionId] = new List<string>(); // Initialize answer list for this user
                return user;
            }
        }

        public void RemoveUser(string connectionId)
        {
            lock (_lock)
            {
                if (_users.TryGetValue(connectionId, out var user))
                {
                    _joinedUsernames.Remove(user.Name.ToLower());
                    _users.Remove(connectionId);
                    _currentAnswers.Remove(connectionId);
                    _allAnswers.Remove(connectionId);
                }
            }
        }

        public UserAnswer? SubmitAnswer(string connectionId, string answer)
        {
            lock (_lock)
            {
                if (!_users.ContainsKey(connectionId)) return null;

                // Store in current answers (for display during quiz)
                var userAnswer = new UserAnswer
                {
                    User = _users[connectionId],
                    Answer = answer
                };
                _currentAnswers[connectionId] = userAnswer;

                // Store in all answers (for final results)
                if (_allAnswers.ContainsKey(connectionId))
                {
                    _allAnswers[connectionId].Add(answer);
                }

                return userAnswer;
            }
        }

        public string GetNextQuestion()
        {
            _currentQuestionIndex = (_currentQuestionIndex + 1) % _questions.Count;
            return _questions[_currentQuestionIndex];
        }

        public void ClearAnswers()
        {
            _currentAnswers.Clear(); // Clear current question answers
        }

        public IEnumerable<UserAnswer> GetCurrentAnswers()
        {
            return _currentAnswers.Values.ToList();
        }

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

        public IEnumerable<User> GetAllUsers()
        {
            lock (_lock)
            {
                return _users.Values.ToList();
            }
        }

        public Dictionary<string, List<string>> GetAllUserAnswers()
        {
            lock (_lock)
            {
                return new Dictionary<string, List<string>>(_allAnswers);
            }
        }

        public int GetCurrentQuestionNumber()
        {
            return _currentQuestionIndex + 1;
        }
    }
}