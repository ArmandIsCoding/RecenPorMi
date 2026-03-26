using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RecenPorMi.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(50)]
        public string DisplayName { get; set; } = string.Empty;
    }
}
