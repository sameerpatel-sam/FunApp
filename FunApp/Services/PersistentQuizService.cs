using FunApp.Models;
using FunApp.Data;
using Microsoft.EntityFrameworkCore;

namespace FunApp.Services
{
    public class PersistentQuizService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly QuizService _memory;
        private readonly object _lock = new();
        private int? _currentSessionId;

        public PersistentQuizService(IDbContextFactory<AppDbContext> factory, QuizService memory)
        {
            _factory = factory;
            _memory = memory;
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
    }
}
