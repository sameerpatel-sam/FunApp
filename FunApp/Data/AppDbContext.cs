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
        public DbSet<CoupleScore> CoupleScores => Set<CoupleScore>();
        public DbSet<IndividualScore> IndividualScores => Set<IndividualScore>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Question>(e =>
            {
                e.HasKey(q => q.Id);
                e.Property(q => q.Text).IsRequired();
                e.Property(q => q.GameMode).IsRequired();
                e.Property(q => q.CorrectAnswer).IsRequired(false); // Optional, only for Individual mode
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

            modelBuilder.Entity<CoupleScore>(e =>
            {
                e.HasKey(c => c.Id);
                e.Property(c => c.LastName).IsRequired();
                e.Property(c => c.PointsAwarded).IsRequired();
                e.HasOne<QuizSession>().WithMany().HasForeignKey(c => c.QuizSessionId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne<Question>().WithMany().HasForeignKey(c => c.QuestionId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<IndividualScore>(e =>
            {
                e.HasKey(i => i.Id);
                e.Property(i => i.ParticipantName).IsRequired();
                e.Property(i => i.UserAnswer).IsRequired();
                e.Property(i => i.CorrectAnswer).IsRequired();
                e.Property(i => i.PointsAwarded).IsRequired();
                e.HasOne<QuizSession>().WithMany().HasForeignKey(i => i.QuizSessionId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne<Question>().WithMany().HasForeignKey(i => i.QuestionId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
