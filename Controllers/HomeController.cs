using Microsoft.AspNetCore.Mvc;
using LinkShortener.Data;
using LinkShortener.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace LinkShortener.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index() => View();
        public IActionResult QRCode() => View();
        public IActionResult TrackUrlClicks() => View();
        public IActionResult Privacy() => View();
        public IActionResult CustomUrl() => View();




        [HttpPost]
        public async Task<IActionResult> CreateShortUrl(string longUrl)
        {
            if (string.IsNullOrWhiteSpace(longUrl) || !Uri.IsWellFormedUriString(longUrl, UriKind.Absolute))
            {
                ModelState.AddModelError("", "Please enter a valid URL.");
                return View("Index");
            }

            string shortCode;
            do
            {
                shortCode = Guid.NewGuid().ToString("N")[..6].ToLower();
            } while (await _context.UrlMappings.AnyAsync(x => x.ShortCode == shortCode));

            var shortUrl = new ShortUrl
            {
                ShortCode = shortCode,
                LongUrl = longUrl
            };

            await _context.UrlMappings.AddAsync(shortUrl);
            await _context.SaveChangesAsync();

            ViewBag.ShortUrl = $"{Request.Scheme}://{Request.Host}/{shortCode}";
            return View("Index");
        }

        [HttpGet("/{shortCode:length(6)}")]
        public async Task<IActionResult> RedirectToLongUrl(string shortCode)
        {
            var shortCodeLower = shortCode.ToLower();
            var urlMapping = await _context.UrlMappings.FirstOrDefaultAsync(x => x.ShortCode == shortCodeLower);

            if (urlMapping == null)
                return NotFound("Short URL not found.");

            // Null-safe access to headers
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
                return NotFound("Request context not available.");

            var headers = context.Request.Headers;
            string referrer = "Direct";

            if (headers.TryGetValue("Referer", out var refererValue))
            {
                var refererUri = refererValue.ToString();
                if (Uri.TryCreate(refererUri, UriKind.Absolute, out var uri))
                {
                    referrer = uri.Host;
                }
            }

            var click = new Click
            {
                ShortUrlId = urlMapping.Id,
                ClickDate = DateTime.UtcNow,
                Referrer = referrer
            };

            await _context.Clicks.AddAsync(click);
            await _context.SaveChangesAsync();

            return Redirect(urlMapping.LongUrl);
        }

        [HttpGet("/api/stats/{shortCode}")]
        public async Task<IActionResult> GetLinkStats(string shortCode)
        {
            var shortUrl = await _context.UrlMappings
                .Include(u => u.Clicks)
                .FirstOrDefaultAsync(u => u.ShortCode == shortCode.ToLower());

            if (shortUrl == null)
                return NotFound();

            var totalClicks = shortUrl.Clicks.Count;

            var clicksByDay = shortUrl.Clicks
                .Where(c => c.ClickDate > DateTime.UtcNow.AddDays(-7))
                .GroupBy(c => c.ClickDate.Date)
                .Select(g => new { Date = g.Key.ToString("MMM d"), Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToList();

            var topReferrers = shortUrl.Clicks
                .GroupBy(c => c.Referrer)
                .Select(g => new { Referrer = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            var topCountries = new[]
            {
                new { Country = "Data Not Available", Count = totalClicks }
            };

            var stats = new
            {
                totalClicks,
                originalUrl = shortUrl.LongUrl,
                clicksByDay,
                topReferrers,
                topCountries
            };

            return Json(stats);
        }
    }
}
