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
using CloudinaryDotNet;
using dotenv.net;

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
.AddCookie(options =>
{
    options.LoginPath = "/Customer/Account/Login";
    options.AccessDeniedPath = "/Customer/Account/AccessDenied";
})
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

DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
Cloudinary cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
cloudinary.Api.Secure = true;

builder.Services.AddSingleton(cloudinary);

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

// Route mặc định redirect đến Customer area
app.MapControllerRoute(
    name: "default_to_customer",
    pattern: "",
    defaults: new { area = "Customer", controller = "Home", action = "Index" });

// Route cho areas
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Route thông thường (không có area)
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
