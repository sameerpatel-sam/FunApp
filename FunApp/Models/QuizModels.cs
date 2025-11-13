namespace FunApp.Models
{
    public class User
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int SwitchCount { get; set; } = 0;
    }

    public class UserAnswer
    {
        public User User { get; set; } = null!;
        public string Answer { get; set; } = string.Empty;
    }

    public enum GameMode
    {
        Individual,
        Couple
    }

    public class QuizSession
    {
        public int Id { get; set; }
        public GameMode Mode { get; set; } = GameMode.Individual;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }

    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public GameMode GameMode { get; set; } = GameMode.Individual;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class QuizResponse
    {
        public int Id { get; set; }
        public int QuizSessionId { get; set; }
        public int QuestionId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}