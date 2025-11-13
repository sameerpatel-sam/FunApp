using Microsoft.EntityFrameworkCore;
using FunApp.Models;

namespace FunApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Question> Questions => Set<Question>();
        public DbSet<QuizSession> QuizSessions => Set<QuizSession>();
        public DbSet<QuizResponse> QuizResponses => Set<QuizResponse>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Question>(e =>
            {
                e.HasKey(q => q.Id);
                e.Property(q => q.Text).IsRequired();
                e.Property(q => q.GameMode).IsRequired();

                // Seed default questions
                e.HasData(
                    new Question { Id = 1, Text = "What's your favorite color?", GameMode = GameMode.Individual },
                    new Question { Id = 2, Text = "If you could be any animal, what would you be?", GameMode = GameMode.Individual },
                    new Question { Id = 3, Text = "What's your dream vacation destination?", GameMode = GameMode.Individual },
                    new Question { Id = 4, Text = "What superpower would you choose?", GameMode = GameMode.Individual },
                    new Question { Id = 5, Text = "What's your favorite food?", GameMode = GameMode.Individual },
                    new Question { Id = 6, Text = "How did you two meet?", GameMode = GameMode.Couple },
                    new Question { Id = 7, Text = "What's your favorite memory together?", GameMode = GameMode.Couple },
                    new Question { Id = 8, Text = "What do you love most about your partner?", GameMode = GameMode.Couple }
                );
            });

            modelBuilder.Entity<QuizSession>(e =>
            {
                e.HasKey(s => s.Id);
                e.Property(s => s.Id).ValueGeneratedOnAdd();
                e.Property(s => s.Mode).IsRequired();
                e.Property(s => s.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.Property(s => s.IsActive).HasDefaultValue(true);
            });

            modelBuilder.Entity<QuizResponse>(e =>
            {
                e.HasKey(r => r.Id);
                e.Property(r => r.ParticipantName).IsRequired();
                e.Property(r => r.Answer).IsRequired();
                e.HasOne<QuizSession>().WithMany().HasForeignKey(r => r.QuizSessionId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne<Question>().WithMany().HasForeignKey(r => r.QuestionId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
