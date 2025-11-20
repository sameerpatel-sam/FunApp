using FunApp.Models;
using FunApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FunApp.Services
{
    public class PersistentQuizService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly QuizService _memory;
        private readonly ILogger<PersistentQuizService> _logger;
        private readonly object _lock = new();
        private int? _currentSessionId;

        public PersistentQuizService(IDbContextFactory<AppDbContext> factory, QuizService memory, ILogger<PersistentQuizService> logger)
        {
            _factory = factory;
            _memory = memory;
            _logger = logger;
        }

        public int? GetCurrentSessionId()
        {
            lock (_lock) return _currentSessionId;
        }

        public async Task<QuizSession> StartSessionAsync()
        {
            using var db = _factory.CreateDbContext();
            lock (_lock)
            {
                if (_currentSessionId.HasValue)
                {
                    var prev = db.QuizSessions.Find(_currentSessionId.Value);
                    if (prev != null && prev.IsActive)
                    {
                        prev.IsActive = false;
                        db.SaveChanges();
                    }
                }
            }

            var session = new QuizSession { Mode = _memory.GetGameMode(), IsActive = true, CreatedAt = DateTime.UtcNow };
            db.QuizSessions.Add(session);
            await db.SaveChangesAsync();
            lock (_lock) { _currentSessionId = session.Id; }
            return session;
        }

        public async Task<QuizSession> EnsureSessionAsync()
        {
            lock (_lock)
            {
                if (_currentSessionId.HasValue)
                {
                    using var dbCheck = _factory.CreateDbContext();
                    var s = dbCheck.QuizSessions.Find(_currentSessionId.Value);
                    if (s != null) return s;
                }
            }
            return await StartSessionAsync();
        }

        public async Task<Question> AddQuestionAsync(string text, GameMode mode)
        {
            using var db = _factory.CreateDbContext();
            var q = new Question { Text = text, GameMode = mode, CreatedAt = DateTime.UtcNow };
            db.Questions.Add(q);
            await db.SaveChangesAsync();
            return q;
        }

        public async Task<Question?> UpdateQuestionAsync(int id, string text)
        {
            using var db = _factory.CreateDbContext();
            var q = await db.Questions.FindAsync(id);
            if (q == null) return null;
            q.Text = text;
            await db.SaveChangesAsync();
            return q;
        }

        public async Task<bool> DeleteQuestionAsync(int id)
        {
            using var db = _factory.CreateDbContext();
            var q = await db.Questions.FindAsync(id);
            if (q == null) return false;
            db.Questions.Remove(q);
            await db.SaveChangesAsync();
            return true;
        }

        public Task<List<Question>> GetQuestionsAsync(GameMode mode)
        {
            using var db = _factory.CreateDbContext();
            return db.Questions.Where(q => q.GameMode == mode).OrderBy(q => q.Id).ToListAsync();
        }

        public async Task<QuizResponse?> AddResponseAsync(string participantName, int questionId, string answer)
        {
            var session = await EnsureSessionAsync();
            using var db = _factory.CreateDbContext();
            var response = new QuizResponse
            {
                QuizSessionId = session.Id,
                QuestionId = questionId,
                ParticipantName = participantName,
                Answer = answer,
                CreatedAt = DateTime.UtcNow
            };
            db.QuizResponses.Add(response);
            await db.SaveChangesAsync();
            return response;
        }

        public Task<List<QuizResponse>> GetResponsesForSessionAsync(int sessionId)
        {
            using var db = _factory.CreateDbContext();
            return db.QuizResponses.Where(r => r.QuizSessionId == sessionId).OrderBy(r => r.Id).ToListAsync();
        }

        public List<Question> GetAllQuestions()
        {
            using var db = _factory.CreateDbContext();
            return db.Questions.OrderBy(q => q.Id).ToList();
        }

        // Couple score persistence methods
        public async Task<CoupleScore> SaveCoupleScoreAsync(string lastName, int questionId, bool answersMatched,
            string partner1Answer, string partner2Answer)
        {
            var session = await EnsureSessionAsync();
            using var db = _factory.CreateDbContext();

            var coupleScore = new CoupleScore
            {
                QuizSessionId = session.Id,
                LastName = lastName,
                QuestionId = questionId,
                AnswersMatched = answersMatched,
                PointsAwarded = answersMatched ? 1 : 0,
                Partner1Answer = partner1Answer,
                Partner2Answer = partner2Answer,
                CreatedAt = DateTime.UtcNow
            };

            db.CoupleScores.Add(coupleScore);
            await db.SaveChangesAsync();

            _logger.LogInformation("[DB SAVE] Saved couple score: SessionId={SessionId}, LastName={LastName}, QuestionId={QuestionId}, Matched={Matched}, Points={Points}",
                session.Id, lastName, questionId, answersMatched, coupleScore.PointsAwarded);

            return coupleScore;
        }

        public async Task<List<CoupleScore>> GetCoupleScoresForSessionAsync(int sessionId)
        {
            using var db = _factory.CreateDbContext();
            var scores = await db.CoupleScores
                .Where(c => c.QuizSessionId == sessionId)
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.QuestionId)
                .ToListAsync();

            _logger.LogInformation("[DB QUERY] GetCoupleScoresForSession({SessionId}): Found {Count} scores", sessionId, scores.Count);
            return scores;
        }

        public async Task<Dictionary<string, int>> GetCoupleTotalScoresAsync(int sessionId)
        {
            using var db = _factory.CreateDbContext();

            _logger.LogInformation("[GetCoupleTotalScores] Starting query for session {SessionId}", sessionId);

            // First, get all scores to see what's in the database
            var allScores = await db.CoupleScores
                .Where(c => c.QuizSessionId == sessionId)
                .ToListAsync();

            _logger.LogInformation("[GetCoupleTotalScores] Found {Count} total score entries in database", allScores.Count);
            
            if (allScores.Count == 0)
            {
                _logger.LogWarning("[GetCoupleTotalScores] NO SCORES FOUND IN DATABASE for session {SessionId}!", sessionId);
                _logger.LogWarning("[GetCoupleTotalScores] This means either:");
                _logger.LogWarning("[GetCoupleTotalScores]   1. No questions were answered in couple mode, OR");
                _logger.LogWarning("[GetCoupleTotalScores]   2. SaveCoupleScoreAsync was never called, OR");
                _logger.LogWarning("[GetCoupleTotalScores]   3. Wrong session ID is being queried");
            }
            
            foreach (var score in allScores)
            {
                _logger.LogInformation("[GetCoupleTotalScores]   - LastName='{LastName}', QuestionId={QuestionId}, PointsAwarded={PointsAwarded}",
                    score.LastName, score.QuestionId, score.PointsAwarded);
            }

            // Use case-insensitive grouping but preserve original casing
            var grouped = allScores
                .GroupBy(c => c.LastName, StringComparer.OrdinalIgnoreCase)
                .Select(g => new { LastName = g.First().LastName, TotalScore = g.Sum(c => c.PointsAwarded) })
                .ToList();

            _logger.LogInformation("[GetCoupleTotalScores] Grouped into {Count} couples:", grouped.Count);
            foreach (var item in grouped)
            {
                _logger.LogInformation("[GetCoupleTotalScores]   - '{LastName}': {TotalScore} points", item.LastName, item.TotalScore);
            }

            // Create result dictionary with case-insensitive key comparer
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in grouped)
            {
                result[item.LastName] = item.TotalScore;
            }

            _logger.LogInformation("[GetCoupleTotalScores] Returning {Count} couple totals", result.Count);
            
            return result;
        }
    }
}
