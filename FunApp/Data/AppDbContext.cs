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
