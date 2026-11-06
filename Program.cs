using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using baithi.Repositories;
using baithi.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Controllers & JSON configurations
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// 2. Add DbContext with SQL Server
builder.Services.AddDbContext<SunriseDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

// 3. Register Repositories & Services
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddHttpClient<IGeminiService, GeminiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});

// 4. Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SunriseHotel_Secret_Key_For_Jwt_Authentication_2026_Set02!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "SunriseHotel";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "SunriseHotelApi";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 5. Configure Swagger with JWT Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Sunrise Hotel - AI Booking & Management API",
        Version = "v1",
        Description = "Hệ thống quản lý đặt phòng và trợ lý Generative AI cho Sunrise Hotel (Nha Trang).\n" +
                      "- POST /api/auth/token: Lấy mã JWT Token mẫu để bấm Authorize\n" +
                      "- POST /api/ai/ask: AI Lễ tân tư vấn dịch vụ\n" +
                      "- POST /api/bookings/parse: Bóc tách thông tin đặt phòng tự nhiên bằng JSON Mode\n" +
                      "- POST /api/assistant/chat: Trợ lý ảo thực thi Function Calling trực tiếp vào DB\n" +
                      "- GET /api/reports/bookings: Báo cáo danh sách đặt phòng đã xác nhận",
        Contact = new OpenApiContact
        {
            Name = "Sunrise Hotel IT Department",
            Email = "contact@sunrisehotel.vn"
        }
    });

    // Cấu hình JWT Bearer trong Swagger UI
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập mã JWT Token vào ô bên dưới (Ví dụ: Bearer {token} hoặc chỉ cần nhập {token}):"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    c.AddSecurityRequirement((document) => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// 6. Initialize & Seed Database automatically on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Đang kiểm tra và khởi tạo cơ sở dữ liệu SUNRISEHOTEL...");
        var bookingRepo = services.GetRequiredService<IBookingRepository>();
        await bookingRepo.EnsureDatabaseCreatedAndSeededAsync();
        logger.LogInformation("Khởi tạo và nạp dữ liệu mẫu SUNRISEHOTEL thành công!");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Đã xảy ra lỗi trong quá trình khởi tạo cơ sở dữ liệu.");
    }
}

// 7. Configure HTTP pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sunrise Hotel API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Sunrise Hotel API Documentation";
});

// Phục vụ giao diện web tĩnh từ wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

// Bật Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
