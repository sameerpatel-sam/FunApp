using FunApp.Models;

namespace FunApp.Services
{
    public class QuizService
    {
        private readonly Dictionary<string, User> _users = new();
        private readonly Dictionary<string, User> _usersArchive = new();
        private readonly Dictionary<string, List<string>> _allAnswers = new();
        private readonly Dictionary<string, UserAnswer> _currentAnswers = new();
        private readonly HashSet<string> _joinedUsernames = new();
        private GameMode _currentGameMode = GameMode.Individual;
        private int _currentQuestionIndex = -1; // index within persisted question list (managed externally)
        private int? _currentQuestionId = null; // current persisted question id
        private readonly object _lock = new();

        // Removed in-memory questions and seeding; persistence handled by PersistentQuizService

        public void SetGameMode(GameMode mode)
        {
            _currentGameMode = mode;
            _currentQuestionIndex = -1;
            _currentQuestionId = null;
        }

        public GameMode GetGameMode() => _currentGameMode;

        // Index helpers used by hub when advancing persisted question list
        public int AdvanceIndex(int totalCount)
        {
            if (totalCount <= 0) { _currentQuestionIndex = -1; return -1; }
            _currentQuestionIndex = (_currentQuestionIndex + 1) % totalCount;
            return _currentQuestionIndex;
        }

        public int GetCurrentQuestionNumber() => _currentQuestionIndex + 1; // 1-based for UI
        public void SetCurrentQuestionId(int id) => _currentQuestionId = id;
        public int? GetCurrentQuestionId() => _currentQuestionId;

        public User? GetUser(string connectionId)
        {
            lock (_lock)
            {
                return _users.TryGetValue(connectionId, out var user) ? user : null;
            }
        }

        public User? GetArchivedUser(string connectionId)
        {
            lock (_lock)
            {
                return _usersArchive.TryGetValue(connectionId, out var user) ? user : null;
            }
        }

        public User? AddUser(string connectionId, string name)
        {
            lock (_lock)
            {
                var normalizedName = name.Trim().ToLower();
                if (_users.ContainsKey(connectionId)) return null;
                if (_joinedUsernames.Contains(normalizedName)) return null;

                var user = new User { ConnectionId = connectionId, Name = name.Trim() };
                _users[connectionId] = user;
                _usersArchive[connectionId] = user; // keep archived for results
                _joinedUsernames.Add(normalizedName);
                if (!_allAnswers.ContainsKey(connectionId)) _allAnswers[connectionId] = new List<string>();
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
                }
                _users.Remove(connectionId);
                _currentAnswers.Remove(connectionId);
            }
        }

        public UserAnswer? SubmitAnswer(string connectionId, string answer)
        {
            lock (_lock)
            {
                if (!_users.ContainsKey(connectionId) && !_usersArchive.ContainsKey(connectionId)) return null;
                var user = _users.ContainsKey(connectionId) ? _users[connectionId] : _usersArchive[connectionId];
                var userAnswer = new UserAnswer { User = user, Answer = answer };
                _currentAnswers[connectionId] = userAnswer;
                if (_allAnswers.ContainsKey(connectionId)) _allAnswers[connectionId].Add(answer);
                else _allAnswers[connectionId] = new List<string> { answer };
                return userAnswer;
            }
        }

        public void ClearAnswers() => _currentAnswers.Clear();

        public IEnumerable<UserAnswer> GetCurrentAnswers() => _currentAnswers.Values.ToList();

        public int IncrementSwitchCount(string connectionId)
        {
            lock (_lock)
            {
                if (_users.TryGetValue(connectionId, out var user))
                {
                    user.SwitchCount++;
                    if (_usersArchive.ContainsKey(connectionId)) _usersArchive[connectionId].SwitchCount = user.SwitchCount;
                    return user.SwitchCount;
                }
                else if (_usersArchive.TryGetValue(connectionId, out var archived))
                {
                    archived.SwitchCount++;
                    return archived.SwitchCount;
                }
            }
            return 0;
        }

        public IEnumerable<User> GetAllUsers()
        {
            lock (_lock) { return _users.Values.ToList(); }
        }

        public Dictionary<string, List<string>> GetAllUserAnswers()
        {
            lock (_lock) { return new Dictionary<string, List<string>>(_allAnswers); }
        }
    }
}