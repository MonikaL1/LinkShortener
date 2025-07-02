//
// INSTRUCTIONS: Replace the entire contents of your Models/ShortUrl.cs file
// with the code below.
//
using System.Collections.Generic;

namespace LinkShortener.Models
{
    public class ShortUrl
    {
        public int Id { get; set; }
        public string ShortCode { get; set; } = string.Empty;
        public string LongUrl { get; set; } = string.Empty;

        public ICollection<Click> Clicks { get; set; } = new List<Click>();
    }
}
