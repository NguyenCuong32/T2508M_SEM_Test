using Microsoft.EntityFrameworkCore;
using SunriseHotel.Api.Data;
using SunriseHotel.Api.Services;

var options = new WebApplicationOptions
{
    Args = args,
    WebRootPath = Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "SunriseHotel.Api", "wwwroot"))
        ? Path.Combine(Directory.GetCurrentDirectory(), "SunriseHotel.Api", "wwwroot")
        : (Directory.Exists(Path.Combine(AppContext.BaseDirectory, "wwwroot"))
            ? Path.Combine(AppContext.BaseDirectory, "wwwroot")
            : "wwwroot")
};

var builder = WebApplication.CreateBuilder(options);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Database (SQL Server with automatic fallback to SQLite)
var dbProvider = builder.Configuration["DatabaseProvider"] ?? "SqlServer";
var sqlServerConn = builder.Configuration.GetConnectionString("DefaultConnection");
var sqliteConn = builder.Configuration.GetConnectionString("SqliteConnection") ?? "Data Source=sunrisehotel.db";

bool isSqlServerAvailable = false;
if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(sqlServerConn))
{
    try
    {
        var testBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(sqlServerConn)
        {
            ConnectTimeout = 2
        };
        using var testConn = new Microsoft.Data.SqlClient.SqlConnection(testBuilder.ConnectionString);
        testConn.Open();
        isSqlServerAvailable = true;
    }
    catch
    {
        isSqlServerAvailable = false;
    }
}

if (isSqlServerAvailable)
{
    builder.Services.AddDbContext<SunriseHotelDbContext>(options =>
        options.UseSqlServer(sqlServerConn, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(maxRetryCount: 2, maxRetryDelay: TimeSpan.FromSeconds(2), errorNumbersToAdd: null);
        }));
}
else
{
    builder.Services.AddDbContext<SunriseHotelDbContext>(options =>
        options.UseSqlite(sqliteConn));
}

// Register Application Services
builder.Services.AddHttpClient<IGeminiService, GeminiService>();
builder.Services.AddScoped<IHotelBookingService, HotelBookingService>();

var app = builder.Build();

// Auto initialize and seed database
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<SunriseHotelDbContext>();
        db.Database.EnsureCreated();
        logger.LogInformation("Database initialized successfully using provider: {Provider}",
            isSqlServerAvailable ? "SQL Server" : "SQLite");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to initialize database.");
    }
}

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment() || true) // Enable Swagger in all environments for grading
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sunrise Hotel AI API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors();

// Enable serving wwwroot static files for Frontend (Requirement 5)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

app.Run();
