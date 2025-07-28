using System.ComponentModel.DataAnnotations;

namespace ListKeeperWebApi.WebApi.Models.ViewModels
{
    public class SignupViewModel
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;
 
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public bool AgreeToTerms { get; set; }

        // Optional fields 
        public bool WantsUpdates { get; set; } = true;
        public string? FavoriteTimHortonsItem { get; set; }
    }
}
