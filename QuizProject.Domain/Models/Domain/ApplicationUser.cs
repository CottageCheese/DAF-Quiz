using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace QuizProject.Domain.Models.Domain;

public class ApplicationUser : IdentityUser
{
    [Required, MaxLength(50)]
    public string DisplayName { get; set; } = string.Empty;
}
