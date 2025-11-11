using FunApp.Models;

namespace FunApp.Services
{
    public class QuizService
    {
        private readonly Dictionary<string, User> _users = new();
        private readonly Dictionary<string, List<string>> _allAnswers = new();
        private readonly Dictionary<string, UserAnswer> _currentAnswers = new();
        private readonly HashSet<string> _joinedUsernames = new();
        private readonly List<Question> _questions = new();
        private GameMode _currentGameMode = GameMode.Individual;
        private int _currentQuestionIndex = -1;
        private readonly object _lock = new();

        public QuizService()
        {
            InitializeDefaultQuestions();
        }

        private void InitializeDefaultQuestions()
        {
            lock (_lock)
            {
                _questions.AddRange(new[]
                {
                    new Question { Id = 1, Text = "What's your favorite color?", GameMode = GameMode.Individual },
                    new Question { Id = 2, Text = "If you could be any animal, what would you be?", GameMode = GameMode.Individual },
                    new Question { Id = 3, Text = "What's your dream vacation destination?", GameMode = GameMode.Individual },
                    new Question { Id = 4, Text = "What superpower would you choose?", GameMode = GameMode.Individual },
                    new Question { Id = 5, Text = "What's your favorite food?", GameMode = GameMode.Individual },
                    new Question { Id = 6, Text = "How did you two meet?", GameMode = GameMode.Couple },
                    new Question { Id = 7, Text = "What's your favorite memory together?", GameMode = GameMode.Couple },
                    new Question { Id = 8, Text = "What do you love most about your partner?", GameMode = GameMode.Couple }
                });
            }
        }

        public void SetGameMode(GameMode mode)
        {
            _currentGameMode = mode;
            _currentQuestionIndex = -1;
        }

        public GameMode GetGameMode() => _currentGameMode;

        public List<Question> GetQuestionsByMode(GameMode mode)
        {
            lock (_lock)
            {
                return _questions.Where(q => q.GameMode == mode).ToList();
            }
        }

        public void AddQuestion(string text, GameMode mode)
        {
            lock (_lock)
            {
                var maxId = _questions.Any() ? _questions.Max(q => q.Id) : 0;
                _questions.Add(new Question 
                { 
                    Id = maxId + 1, 
                    Text = text, 
                    GameMode = mode,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        public void UpdateQuestion(int id, string text)
        {
            lock (_lock)
            {
                var question = _questions.FirstOrDefault(q => q.Id == id);
                if (question != null)
                {
                    question.Text = text;
                }
            }
        }

        public void DeleteQuestion(int id)
        {
            lock (_lock)
            {
                _questions.RemoveAll(q => q.Id == id);
            }
        }

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
                _allAnswers[connectionId] = new List<string>();
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

                var userAnswer = new UserAnswer
                {
                    User = _users[connectionId],
                    Answer = answer
                };
                _currentAnswers[connectionId] = userAnswer;

                if (_allAnswers.ContainsKey(connectionId))
                {
                    _allAnswers[connectionId].Add(answer);
                }

                return userAnswer;
            }
        }

        public string GetNextQuestion()
        {
            var questionsForMode = _questions.Where(q => q.GameMode == _currentGameMode).ToList();
            if (questionsForMode.Count == 0)
                return "No questions available for this game mode.";

            _currentQuestionIndex = (_currentQuestionIndex + 1) % questionsForMode.Count;
            return questionsForMode[_currentQuestionIndex].Text;
        }

        public void ClearAnswers()
        {
            _currentAnswers.Clear();
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