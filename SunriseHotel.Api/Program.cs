using Microsoft.EntityFrameworkCore;
using SunriseHotel.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SunriseHotelDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Add Services
builder.Services.AddScoped<SunriseHotel.Api.Services.GeminiService>();

// 3. Add Controllers & JSON Options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// 3. Add Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. Configure HTTP pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sunrise Hotel API v1");
    c.RoutePrefix = "swagger";
});

// 5. Static files for Frontend (Requirement 5)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
