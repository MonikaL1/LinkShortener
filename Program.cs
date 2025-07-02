using Microsoft.EntityFrameworkCore;
using LinkShortener.Data;
// === UPDATE 1: Add a using statement for Forwarded Headers ===
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// --- SERVICES ---

// === UPDATE 2: Make the Database Path Reliable ===
var contentRoot = builder.Environment.ContentRootPath;
var dbPath = Path.Combine(contentRoot, "urls.db");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));


builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// === UPDATE 3: Configure Forwarded Headers Service ===
// This is essential for HTTPS to work correctly on Elastic Beanstalk
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});


var app = builder.Build();

// --- DATABASE MIGRATION ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// --- MIDDLEWARE PIPELINE ---

// === UPDATE 4: Add Production Configuration ===
// This is crucial for security and proper error handling when live
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Assumes you have an Error action in HomeController
    app.UseHsts();
}

// === UPDATE 5: Use Forwarded Headers & Add HTTPS Redirection ===
// UseForwardedHeaders MUST be one of the first middleware components.
app.UseForwardedHeaders();
app.UseHttpsRedirection(); // Now that ForwardedHeaders is configured, this will work correctly.


app.UseStaticFiles();
app.UseRouting();

// YOUR ROUTING IS CORRECT - NO CHANGES NEEDED HERE
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();