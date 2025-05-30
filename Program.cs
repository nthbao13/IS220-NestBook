using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BookNest.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using BookNest.Helper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using System.Text.Json;
using BookNest.Models;
using BookNest.Models.MappingDBModel;
using BookNest.Services;

var builder = WebApplication.CreateBuilder(args);

var googleApiPath = Path.Combine(builder.Environment.ContentRootPath, "google_api_key.json");

var jsonString = File.ReadAllText(googleApiPath);

var googleKeys = JsonSerializer.Deserialize<GoogleApiKeys>(jsonString);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = googleKeys.ClientId;
    googleOptions.ClientSecret = googleKeys.ClientSecret;
    googleOptions.CallbackPath = "/signin-google";
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<AspNetUser, AspNetRole>(options =>
{
    // Cấu hình password
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;

    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddScoped<IVnPayService, VnPayService>();

builder.Services.AddMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Định nghĩa class để deserialize google_api_key.json
public class GoogleApiKeys
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}
