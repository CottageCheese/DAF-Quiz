using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizProject.Api.Models.Domain;

namespace QuizProject.Api.Data;

public static class SeedData
{
    public static async Task InitialiseAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        await SeedRolesAndAdminAsync(serviceProvider);
        await SeedQuizzesAsync(context);
    }

    private static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        // Ensure Admin role exists
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        // Ensure default admin user exists
        const string adminEmail = "admin@quiz.local";
        const string adminPassword = "Admin123!";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail };
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    private static async Task SeedQuizzesAsync(ApplicationDbContext context)
    {
        if (await context.Quizzes.AnyAsync())
            return;

        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@quiz.local");
        var adminId = adminUser?.Id;
        const string adminEmail = "admin@quiz.local";

        var quizzes = new List<Quiz>
        {
            new()
            {
                Title = "General Knowledge",
                Description = "Test your knowledge across a variety of topics.",
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = adminId,
                CreatedByEmail = adminEmail,
                PublishedAt = DateTime.UtcNow,
                Questions = new List<Question>
                {
                    MakeQuestion("What is the capital city of France?", "Paris", "London", "Berlin", "Madrid", 1),
                    MakeQuestion("How many sides does a hexagon have?", "6", "5", "7", "8", 2),
                    MakeQuestion("Which planet is known as the Red Planet?", "Mars", "Venus", "Jupiter", "Saturn", 3),
                    MakeQuestion("What is the largest ocean on Earth?", "Pacific Ocean", "Atlantic Ocean",
                        "Indian Ocean", "Arctic Ocean", 4),
                    MakeQuestion("Who painted the Mona Lisa?", "Leonardo da Vinci", "Michelangelo", "Raphael",
                        "Donatello", 5)
                }
            },
            new()
            {
                Title = "Science Basics",
                Description = "Fundamental science questions covering physics, chemistry, and biology.",
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = adminId,
                CreatedByEmail = adminEmail,
                PublishedAt = DateTime.UtcNow,
                Questions = new List<Question>
                {
                    MakeQuestion("What is the chemical symbol for water?", "H₂O", "CO₂", "O₂", "H₂SO₄", 1),
                    MakeQuestion("What force keeps planets in orbit around the Sun?", "Gravity", "Magnetism",
                        "Friction", "Inertia", 2),
                    MakeQuestion("How many chromosomes do humans typically have?", "46", "23", "48", "44", 3),
                    MakeQuestion("What is the speed of light (approx.) in a vacuum?", "300,000 km/s", "150,000 km/s",
                        "450,000 km/s", "30,000 km/s", 4),
                    MakeQuestion("Which gas do plants absorb during photosynthesis?", "Carbon dioxide", "Oxygen",
                        "Nitrogen", "Hydrogen", 5)
                }
            }
        };

        context.Quizzes.AddRange(quizzes);
        await context.SaveChangesAsync();
    }

    private static Question MakeQuestion(
        string text, string correctAnswer,
        string wrong1, string wrong2, string wrong3, int order)
    {
        return new Question
        {
            Text = text,
            DisplayOrder = order,
            Answers = new List<Answer>
            {
                new() { Text = correctAnswer, IsCorrect = true },
                new() { Text = wrong1, IsCorrect = false },
                new() { Text = wrong2, IsCorrect = false },
                new() { Text = wrong3, IsCorrect = false }
            }
        };
    }
}
