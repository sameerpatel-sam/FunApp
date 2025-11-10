namespace FunApp.Models
{
    public class User
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class UserAnswer
    {
        public User User { get; set; } = null!;
        public string Answer { get; set; } = string.Empty;
    }
}