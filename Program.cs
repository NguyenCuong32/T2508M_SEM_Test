using Microsoft.EntityFrameworkCore;
using SunriseHotel.Data;
using SunriseHotel.Services;

// Load .env file if exists
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#')) continue;
        var parts = trimmed.Split('=', 2);
        if (parts.Length == 2)
        {
            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
        }
    }
}

var builder = WebApplication.CreateBuilder(args);

// Load separate gemini.json file if exists
builder.Configuration.AddJsonFile("gemini.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllers();

// Configure MySQL DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=localhost;Port=3306;Database=SUNRISEHOTEL;User=root;Password=root;";

builder.Services.AddDbContext<SunriseHotelDbContext>(options =>
{
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 44)));
});

// Configure HttpClient for AI integration
builder.Services.AddHttpClient();
builder.Services.AddScoped<IGeminiService, GeminiService>();
builder.Services.AddScoped<IBookingToolService, BookingToolService>();

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Serve static files for frontend website
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
