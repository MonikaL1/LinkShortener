namespace LinkShortener.Models
{
    public class UrlMapping
    {
        public int Id { get; set; }
        public string ShortCode { get; set; } = null!;
        public string LongUrl { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
