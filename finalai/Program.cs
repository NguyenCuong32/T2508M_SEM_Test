using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SunriseHotelAI.Data;
using SunriseHotelAI.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Database (SQL Server SUNRISEHOTEL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=localhost;Database=SUNRISEHOTEL;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;";

builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Register Application Services & HttpClient for Gemini
builder.Services.AddHttpClient<IGeminiService, GeminiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});
builder.Services.AddScoped<IHotelService, HotelService>();

// 3. Controllers and JSON serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// 4. CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 5. Swagger / OpenAPI Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Sunrise Hotel AI Booking Assistant API",
        Version = "v1",
        Description = "API endpoints for Sunrise Hotel AI Assistant powered by Google Gemini (Set02 Practical Test)."
    });
});

var app = builder.Build();

// 6. Ensure Database Created & Seeded with Initial Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<HotelDbContext>();
        await DbInitializer.InitializeAsync(context);
        app.Logger.LogInformation("Database SUNRISEHOTEL initialized and seeded successfully.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

// 7. HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sunrise Hotel AI API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");

// Serve static files for frontend website
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

app.Run();
