using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SunriseHotel.Api.ErrorHandling;
using SunriseHotel.Application;
using SunriseHotel.Infrastructure;
using SunriseHotel.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Layers (Clean Architecture / SOLID): Api -> Application -> Domain, Infrastructure implements Application abstractions.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new() { Title = "Sunrise Hotel API", Version = "v1" });
    o.MapType<DateOnly>(() => new() { Type = "string", Format = "date" });
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// Create / upgrade the SUNRISEHOTEL database (Code First migrations + seed data for Room).
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<HotelDbContext>().Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllers();
app.MapRazorPages();

app.Run();
