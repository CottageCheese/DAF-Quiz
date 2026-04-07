using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuizProject.Web.Models.Domain;

namespace QuizProject.Web.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<QuizAttemptAnswer> QuizAttemptAnswers => Set<QuizAttemptAnswer>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Quiz>(e =>
        {
            e.Property(q => q.Title).HasMaxLength(200).IsRequired();
            e.Property(q => q.Description).HasMaxLength(1000);
        });

        builder.Entity<Question>(e =>
        {
            e.Property(q => q.Text).HasMaxLength(1000).IsRequired();
            e.HasOne(q => q.Quiz)
             .WithMany(quiz => quiz.Questions)
             .HasForeignKey(q => q.QuizId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Answer>(e =>
        {
            e.Property(a => a.Text).HasMaxLength(500).IsRequired();
            e.HasOne(a => a.Question)
             .WithMany(q => q.Answers)
             .HasForeignKey(a => a.QuestionId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<QuizAttempt>(e =>
        {
            e.HasOne(a => a.User)
             .WithMany()
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Quiz)
             .WithMany(q => q.Attempts)
             .HasForeignKey(a => a.QuizId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<QuizAttemptAnswer>(e =>
        {
            e.HasOne(a => a.Attempt)
             .WithMany(qa => qa.AttemptAnswers)
             .HasForeignKey(a => a.AttemptId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Question)
             .WithMany()
             .HasForeignKey(a => a.QuestionId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.SelectedAnswer)
             .WithMany()
             .HasForeignKey(a => a.SelectedAnswerId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
