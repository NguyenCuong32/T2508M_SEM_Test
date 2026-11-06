using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SunriseHotel.Application.Abstractions.Ai;
using SunriseHotel.Application.Abstractions.Persistence;
using SunriseHotel.Application.Abstractions.Time;
using SunriseHotel.Infrastructure.Ai.Gemini;
using SunriseHotel.Infrastructure.Persistence;
using SunriseHotel.Infrastructure.Persistence.Repositories;
using SunriseHotel.Infrastructure.Time;

namespace SunriseHotel.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SunriseHotel")
                               ?? throw new InvalidOperationException("Connection string 'SunriseHotel' is missing.");

        services.AddDbContext<HotelDbContext>(o => o.UseSqlServer(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<HotelDbContext>());
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IDateTimeProvider, VietnamDateTimeProvider>();

        services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));
        services.AddHttpClient<IGenerativeAiClient, GeminiClient>((sp, http) =>
        {
            var gemini = sp.GetRequiredService<IOptions<GeminiOptions>>().Value;
            http.BaseAddress = new Uri(gemini.BaseUrl);
            http.Timeout = TimeSpan.FromSeconds(gemini.TimeoutSeconds);
        });

        return services;
    }
}
