using System.ComponentModel.DataAnnotations;

namespace LinkShortener.Models // Make sure this namespace matches your project's namespace
{
    public class CustomUrlViewModel
    {
        [Required(ErrorMessage = "Please enter the URL you want to shorten.")]
        [Url(ErrorMessage = "Please enter a valid URL.")]
        [Display(Name = "Your Long URL")]
        public required string LongUrl { get; set; }

        [Required(ErrorMessage = "Please enter a custom alias.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "The alias must be between 3 and 100 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\-]+$", ErrorMessage = "The alias can only contain letters, numbers, and hyphens.")]
        [Display(Name = "Your Desired Alias")]
        public required string CustomAlias { get; set; }
    }
}
