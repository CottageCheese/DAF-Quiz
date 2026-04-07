using Microsoft.EntityFrameworkCore;
using QuizProject.Web.Models.Domain;

namespace QuizProject.Web.Data;

public static class SeedData
{
    public static async Task InitialiseAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        if (await context.Quizzes.AnyAsync())
            return;

        var quizzes = new List<Quiz>
        {
            new()
            {
                Title = "General Knowledge",
                Description = "Test your knowledge across a variety of topics.",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Questions = new List<Question>
                {
                    MakeQuestion("What is the capital city of France?", "Paris", "London", "Berlin", "Madrid", 1),
                    MakeQuestion("How many sides does a hexagon have?", "6", "5", "7", "8", 1),
                    MakeQuestion("Which planet is known as the Red Planet?", "Mars", "Venus", "Jupiter", "Saturn", 1),
                    MakeQuestion("What is the largest ocean on Earth?", "Pacific Ocean", "Atlantic Ocean", "Indian Ocean", "Arctic Ocean", 1),
                    MakeQuestion("Who painted the Mona Lisa?", "Leonardo da Vinci", "Michelangelo", "Raphael", "Donatello", 1),
                }
            },
            new()
            {
                Title = "Science Basics",
                Description = "Fundamental science questions covering physics, chemistry, and biology.",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Questions = new List<Question>
                {
                    MakeQuestion("What is the chemical symbol for water?", "H₂O", "CO₂", "O₂", "H₂SO₄", 1),
                    MakeQuestion("What force keeps planets in orbit around the Sun?", "Gravity", "Magnetism", "Friction", "Inertia", 1),
                    MakeQuestion("How many chromosomes do humans typically have?", "46", "23", "48", "44", 1),
                    MakeQuestion("What is the speed of light (approx.) in a vacuum?", "300,000 km/s", "150,000 km/s", "450,000 km/s", "30,000 km/s", 1),
                    MakeQuestion("Which gas do plants absorb during photosynthesis?", "Carbon dioxide", "Oxygen", "Nitrogen", "Hydrogen", 1),
                }
            }
        };

        context.Quizzes.AddRange(quizzes);
        await context.SaveChangesAsync();
    }

    private static Question MakeQuestion(
        string text,
        string correctAnswer,
        string wrong1,
        string wrong2,
        string wrong3,
        int order)
    {
        return new Question
        {
            Text = text,
            DisplayOrder = order,
            Answers = new List<Answer>
            {
                new() { Text = correctAnswer, IsCorrect = true },
                new() { Text = wrong1,        IsCorrect = false },
                new() { Text = wrong2,        IsCorrect = false },
                new() { Text = wrong3,        IsCorrect = false },
            }
        };
    }
}
