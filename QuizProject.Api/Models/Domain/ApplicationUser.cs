using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace QuizProject.Api.Models.Domain;

public class ApplicationUser : IdentityUser
{
    [Required, MaxLength(50)]
    public string DisplayName { get; set; } = string.Empty;
}
