using System;
using System.ComponentModel.DataAnnotations;

namespace LinkShortener.Models
{
    public class Click
    {
        [Key]
        public int Id { get; set; } // A unique ID for each click
        public int ShortUrlId { get; set; } // Links this click to a specific ShortUrl
        public ShortUrl ShortUrl { get; set; } = null!; // The actual ShortUrl object
        public DateTime ClickDate { get; set; } // When the click happened
        public string Referrer { get; set; } = string.Empty; // Where the click came from (e.g., "google.com")
    }
}