using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SunriseHotel.Data;
using SunriseHotel.Services;

var envLocations = new[]
{
    Path.Combine(Directory.GetCurrentDirectory(), ".env"),
    Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"),
    Path.Combine(AppContext.BaseDirectory, ".env")
};

foreach (var envPath in envLocations)
{
    if (File.Exists(envPath))
    {
        var lines = File.ReadAllLines(envPath);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;
            var parts = trimmed.Split('=', 2);
            if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[0]))
            {
                var val = parts[1].Trim().Trim('\"', '\'');
                Environment.SetEnvironmentVariable(parts[0].Trim(), val);
            }
        }
        break;
    }
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "server=localhost;port=3306;database=SUNRISEHOTEL;user=root;password=;CharSet=utf8mb4;";

var serverVersion = new MySqlServerVersion(new Version(8, 0, 30));

builder.Services.AddDbContext<SunriseDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddSingleton<IGeminiService, GeminiService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SunriseDbContext>();
    await DbInitializer.InitializeAsync(context);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.MapControllers();

app.Run();
