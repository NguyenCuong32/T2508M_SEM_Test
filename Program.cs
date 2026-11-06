using SunriseHotel.Repositories;
using SunriseHotel.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddHttpClient<IGeminiService, GeminiService>(client => client.Timeout = TimeSpan.FromSeconds(90));

var app = builder.Build();
app.UseExceptionHandler(handler => handler.Run(async context =>
{
    var ex = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    context.Response.StatusCode = ex is GeminiException gemini ? gemini.Status : 500;
    await context.Response.WriteAsJsonAsync(new { error = ex is GeminiException ? ex.Message : "Đã xảy ra lỗi máy chủ hoặc kết nối cơ sở dữ liệu." });
}));
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.Run();
