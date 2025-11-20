using FunApp.Models;
using Microsoft.Extensions.Logging;

namespace FunApp.Services
{
    public class QuizService
    {
        private readonly Dictionary<string, User> _users = new();
        private readonly Dictionary<string, User> _usersArchive = new();
        private readonly Dictionary<string, List<string>> _allAnswers = new();
        private readonly Dictionary<string, UserAnswer> _currentAnswers = new();
        private readonly HashSet<string> _joinedUsernames = new();
        private readonly Dictionary<string, int> _coupleScores = new(); // LastName -> Total Score
        private GameMode _currentGameMode = GameMode.Individual;
        private int _currentQuestionIndex = -1;
        private int? _currentQuestionId = null;
        private readonly object _lock = new();
        private readonly ILogger<QuizService>? _logger;

        public QuizService(ILogger<QuizService>? logger = null)
        {
            _logger = logger;
        }

        public void SetGameMode(GameMode mode)
        {
            _currentGameMode = mode;
            _currentQuestionIndex = -1;
            _currentQuestionId = null;
            if (mode == GameMode.Couple)
            {
                _coupleScores.Clear();
            }
        }

        public GameMode GetGameMode() => _currentGameMode;

        public int AdvanceIndex(int totalCount)
        {
            if (totalCount <= 0) { _currentQuestionIndex = -1; return -1; }
            _currentQuestionIndex = (_currentQuestionIndex + 1) % totalCount;
            return _currentQuestionIndex;
        }

        public int GetCurrentQuestionNumber() => _currentQuestionIndex + 1;
        public void SetCurrentQuestionId(int id) => _currentQuestionId = id;
        public int? GetCurrentQuestionId() => _currentQuestionId;

        public User? GetUser(string connectionId)
        {
            lock (_lock) { return _users.TryGetValue(connectionId, out var user) ? user : null; }
        }

        public User? GetArchivedUser(string connectionId)
        {
            lock (_lock) { return _usersArchive.TryGetValue(connectionId, out var user) ? user : null; }
        }

        private (string firstName, string lastName) ParseName(string fullName)
        {
            var parts = fullName.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return ("", "");
            if (parts.Length == 1) return (parts[0], "");
            
            // First part is first name, last part is last name
            var firstName = parts[0];
            var lastName = parts[parts.Length - 1];
            return (firstName, lastName);
        }

        public User? AddUser(string connectionId, string name)
        {
            lock (_lock)
            {
                var normalized = name.Trim().ToLower();
                if (_users.ContainsKey(connectionId) || _joinedUsernames.Contains(normalized)) return null;
                
                var (firstName, lastName) = ParseName(name);
                var user = new User 
                { 
                    ConnectionId = connectionId, 
                    Name = name.Trim(),
                    FirstName = firstName,
                    LastName = lastName,
                    Score = 0
                };
                
                _users[connectionId] = user;
                _usersArchive[connectionId] = user;
                _joinedUsernames.Add(normalized);
                if (!_allAnswers.ContainsKey(connectionId)) 
                    _allAnswers[connectionId] = new List<string>();
                
                return user;
            }
        }

        public void RemoveUser(string connectionId)
        {
            lock (_lock)
            {
                if (_users.TryGetValue(connectionId, out var user))
                    _joinedUsernames.Remove(user.Name.ToLower());
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
                var ua = new UserAnswer { User = user, Answer = answer };
                _currentAnswers[connectionId] = ua;
                if (_allAnswers.ContainsKey(connectionId)) 
                    _allAnswers[connectionId].Add(answer); 
                else 
                    _allAnswers[connectionId] = new List<string> { answer };
                return ua;
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
                    if (_usersArchive.ContainsKey(connectionId)) 
                        _usersArchive[connectionId].SwitchCount = user.SwitchCount;
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

        // Couple game specific methods
        public Dictionary<string, List<User>> GetCouplesByLastName()
        {
            lock (_lock)
            {
                return _users.Values
                    .Where(u => !string.IsNullOrEmpty(u.LastName))
                    .GroupBy(u => u.LastName.ToLower())
                    .Where(g => g.Count() == 2) // Only pairs
                    .ToDictionary(g => g.Key, g => g.ToList());
            }
        }

        public List<(string lastNameKey, string partner1Answer, string partner2Answer, bool matched, string properLastName)> EvaluateCoupleAnswers()
        {
            lock (_lock)
            {
                var couples = GetCouplesByLastName();
                var results = new List<(string, string, string, bool, string)>();

                _logger?.LogInformation($"[DEBUG] EvaluateCoupleAnswers: Found {couples.Count} couples to evaluate");

                foreach (var (lastName, partners) in couples)
                {
                    if (partners.Count != 2)
                    {
                        _logger?.LogInformation($"[DEBUG] Couple {lastName}: Skipping - not exactly 2 partners");
                        continue;
                    }

                    var partner1 = partners[0];
                    var partner2 = partners[1];
                    var properLastName = partner1.LastName; // Get proper case from user object

                    _logger?.LogInformation($"[DEBUG] Couple {lastName}: Partner1={partner1.Name}, Partner2={partner2.Name}");

                    if (_currentAnswers.TryGetValue(partner1.ConnectionId, out var answer1) &&
                        _currentAnswers.TryGetValue(partner2.ConnectionId, out var answer2))
                    {
                        var ans1 = answer1.Answer.Trim();
                        var ans2 = answer2.Answer.Trim();
                        var matched = string.Equals(ans1, ans2, StringComparison.OrdinalIgnoreCase);
                        
                        _logger?.LogInformation($"[DEBUG] Couple {properLastName}: Comparing '{ans1}' vs '{ans2}' => {(matched ? "MATCHED" : "NOT MATCHED")}");
                        
                        if (matched)
                        {
                            // Award points
                            partner1.Score++;
                            partner2.Score++;
                            
                            if (!_coupleScores.ContainsKey(lastName))
                                _coupleScores[lastName] = 0;
                            _coupleScores[lastName]++;
                            
                            _logger?.LogInformation($"[DEBUG] Couple {properLastName}: Awarded 1 point. Total now: {_coupleScores[lastName]}");
                        }

                        results.Add((lastName, answer1.Answer, answer2.Answer, matched, properLastName));
                    }
                    else
                    {
                        _logger?.LogInformation($"[DEBUG] Couple {properLastName}: SKIPPED - One or both partners haven't answered");
                        _logger?.LogInformation($"[DEBUG]   Partner1 answered: {_currentAnswers.ContainsKey(partner1.ConnectionId)}");
                        _logger?.LogInformation($"[DEBUG]   Partner2 answered: {_currentAnswers.ContainsKey(partner2.ConnectionId)}");
                    }
                }

                _logger?.LogInformation($"[DEBUG] EvaluateCoupleAnswers: Evaluated {results.Count} couple answers");
                return results;
            }
        }

        public Dictionary<string, CoupleResult> GetCoupleResults()
        {
            lock (_lock)
            {
                _logger?.LogInformation("[GetCoupleResults] Starting...");
                
                // CRITICAL: Log all answers we have
                _logger?.LogInformation("[GetCoupleResults] _allAnswers contains {Count} entries", _allAnswers.Count);
                foreach (var (connectionId, answers) in _allAnswers)
                {
                    var user = _usersArchive.ContainsKey(connectionId) ? _usersArchive[connectionId].Name : "Unknown";
                    _logger?.LogInformation("[GetCoupleResults]   - {User} (connectionId={ConnectionId}): {AnswerCount} answers: [{Answers}]", 
                        user, connectionId.Substring(0, Math.Min(8, connectionId.Length)), answers.Count, string.Join(", ", answers));
                }
                
                // Get couples from archived users (includes disconnected users)
                var couples = _usersArchive.Values
                    .Where(u => !string.IsNullOrEmpty(u.LastName))
                    .GroupBy(u => u.LastName.ToLower())
                    .Where(g => g.Count() == 2)
                    .ToDictionary(g => g.Key, g => g.ToList());

                _logger?.LogInformation("[GetCoupleResults] Found {Count} couples in archive", couples.Count);

                var results = new Dictionary<string, CoupleResult>();

                foreach (var (lastName, partners) in couples)
                {
                    if (partners.Count != 2)
                    {
                        _logger?.LogWarning("[GetCoupleResults] Couple {LastName}: Skipping - not exactly 2 partners", lastName);
                        continue;
                    }

                    var partner1 = partners[0];
                    var partner2 = partners[1];

                    _logger?.LogInformation("[GetCoupleResults] Couple {LastName}: Processing {Partner1} (id={Id1}) and {Partner2} (id={Id2})", 
                        lastName, partner1.Name, partner1.ConnectionId.Substring(0, Math.Min(8, partner1.ConnectionId.Length)), 
                        partner2.Name, partner2.ConnectionId.Substring(0, Math.Min(8, partner2.ConnectionId.Length)));

                    var partner1Answers = _allAnswers.ContainsKey(partner1.ConnectionId) 
                        ? _allAnswers[partner1.ConnectionId] 
                        : new List<string>();
                    var partner2Answers = _allAnswers.ContainsKey(partner2.ConnectionId) 
                        ? _allAnswers[partner2.ConnectionId] 
                        : new List<string>();

                    _logger?.LogInformation("[GetCoupleResults] Couple {LastName}: Partner1 has {Count1} answers, Partner2 has {Count2} answers", 
                        lastName, partner1Answers.Count, partner2Answers.Count);
                    
                    if (partner1Answers.Count > 0)
                    {
                        _logger?.LogInformation("[GetCoupleResults] Couple {LastName}: Partner1 answers: [{Answers}]", 
                            lastName, string.Join(", ", partner1Answers));
                    }
                    if (partner2Answers.Count > 0)
                    {
                        _logger?.LogInformation("[GetCoupleResults] Couple {LastName}: Partner2 answers: [{Answers}]", 
                            lastName, string.Join(", ", partner2Answers));
                    }

                    // Calculate actual matched count from all answers
                    var matchedCount = 0;
                    var minLength = Math.Min(partner1Answers.Count, partner2Answers.Count);
                    
                    _logger?.LogInformation("[GetCoupleResults] Couple {LastName}: Comparing {MinLength} question pairs", 
                        lastName, minLength);
                    
                    for (int i = 0; i < minLength; i++)
                    {
                        var ans1 = partner1Answers[i].Trim();
                        var ans2 = partner2Answers[i].Trim();
                        var matches = string.Equals(ans1, ans2, StringComparison.OrdinalIgnoreCase);
                        
                        _logger?.LogInformation("[GetCoupleResults] Couple {LastName} Q{QuestionNum}: '{Ans1}' vs '{Ans2}' => {Result}", 
                            lastName, i+1, ans1, ans2, matches ? "MATCH" : "NO MATCH");
                        
                        if (matches)
                        {
                            matchedCount++;
                        }
                    }

                    _logger?.LogInformation("[GetCoupleResults] Couple {LastName}: Total matched: {Matched} out of {Total} questions", 
                        lastName, matchedCount, minLength);

                    // CRITICAL DEBUG: Log what we're about to set
                    _logger?.LogInformation("[GetCoupleResults] *** ABOUT TO CREATE RESULT: LastName={LastName}, matchedCount={MatchedCount} ***", 
                        partners[0].LastName, matchedCount);

                    // Create the result with calculated score
                    var result = new CoupleResult
                    {
                        LastName = partners[0].LastName, // Use proper case
                        TotalScore = matchedCount,
                        Partner1Answers = partner1Answers,
                        Partner2Answers = partner2Answers,
                        MatchedAnswers = matchedCount
                    };
                    
                    // CRITICAL DEBUG: Verify what was actually set
                    _logger?.LogInformation("[GetCoupleResults] *** RESULT CREATED: LastName={LastName}, TotalScore={TotalScore}, MatchedAnswers={MatchedAnswers} ***", 
                        result.LastName, result.TotalScore, result.MatchedAnswers);
                    
                    results[lastName] = result;
                    
                    _logger?.LogInformation("[GetCoupleResults] Couple {LastName}: Added to results dictionary with TotalScore={Score}, MatchedAnswers={Matched}", 
                        lastName, result.TotalScore, result.MatchedAnswers);
                }

                _logger?.LogInformation("[GetCoupleResults] Returning {Count} couple results with total scores", results.Count);
                
                // Log each result being returned
                foreach (var (key, result) in results)
                {
                    _logger?.LogInformation("[GetCoupleResults] FINAL - Key: {Key}, LastName: {LastName}, TotalScore: {TotalScore}, MatchedAnswers: {MatchedAnswers}", 
                        key, result.LastName, result.TotalScore, result.MatchedAnswers);
                }
                
                return results;
            }
        }

        public int GetCoupleScore(string lastName)
        {
            lock (_lock)
            {
                return _coupleScores.ContainsKey(lastName.ToLower()) 
                    ? _coupleScores[lastName.ToLower()] 
                    : 0;
            }
        }
    }
}