using MySqlConnector;
using SunriseHotel.API.Data;
using SunriseHotel.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton(_ => new MySqlDataSource(
    builder.Configuration.GetConnectionString("SunriseHotel")
        ?? throw new InvalidOperationException("Missing SUNRISEHOTEL connection string.")));
builder.Services.AddScoped<BookingRepository>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddHttpClient<GeminiService>(client =>
{
    client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/");
    client.Timeout = TimeSpan.FromSeconds(45);
});

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    var (status, title) = exception switch
    {
        RequestException error => (error.StatusCode, error.Message),
        MySqlException => (503, "Không thể truy cập cơ sở dữ liệu SUNRISEHOTEL."),
        _ => (500, "Máy chủ gặp lỗi khi xử lý yêu cầu.")
    };

    await Results.Problem(statusCode: status, title: title).ExecuteAsync(context);
}));
app.UseStatusCodePages();
app.UseStaticFiles();
app.MapControllers();

app.Run();
