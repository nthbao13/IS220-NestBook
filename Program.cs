using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BookNest.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using BookNest.Helper;
using System.Text.Json;
using BookNest.Models;
using BookNest.Models.MappingDBModel;
using BookNest.Services;
using CloudinaryDotNet;
using dotenv.net;

var builder = WebApplication.CreateBuilder(args);

// Load Google API keys
var googleApiPath = Path.Combine(builder.Environment.ContentRootPath, "google_api_key.json");
var jsonString = File.ReadAllText(googleApiPath);
var googleKeys = JsonSerializer.Deserialize<GoogleApiKeys>(jsonString);

// Database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity configuration
builder.Services.AddIdentity<AspNetUser, AspNetRole>(options =>
{
    // Password configuration
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Identity's Cookie Authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Customer/Account/Login";
    options.AccessDeniedPath = "/Customer/Account/AccessDenied";
    options.LogoutPath = "/Customer/Account/Logout";
    options.ReturnUrlParameter = "ReturnUrl";
});

// External authentication (Google)
builder.Services.AddAuthentication()
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = googleKeys.ClientId;
    googleOptions.ClientSecret = googleKeys.ClientSecret;
    googleOptions.CallbackPath = "/signin-google";
});

// Other services
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<IVnPayService, VnPayService>();

// Cloudinary configuration
DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
Cloudinary cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
cloudinary.Api.Secure = true;
builder.Services.AddSingleton(cloudinary);

// Session
builder.Services.AddMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

// Middleware pipeline
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

// Routing configuration - Areas first!
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default_to_customer",
    pattern: "",
    defaults: new { area = "Customer", controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Google API Keys model
public class GoogleApiKeys
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}