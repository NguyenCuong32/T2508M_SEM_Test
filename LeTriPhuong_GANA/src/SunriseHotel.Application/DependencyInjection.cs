using Microsoft.Extensions.DependencyInjection;
using SunriseHotel.Application.Features.Assistant;
using SunriseHotel.Application.Features.Assistant.Functions;
using SunriseHotel.Application.Features.BookingParsing;
using SunriseHotel.Application.Features.Bookings;
using SunriseHotel.Application.Features.HotelQa;
using SunriseHotel.Application.Features.Reports;

namespace SunriseHotel.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IHotelQaService, HotelQaService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IParsedBookingValidator, ParsedBookingValidator>();
        services.AddScoped<IBookingParserService, BookingParserService>();
        services.AddScoped<IBookingReportService, BookingReportService>();
        services.AddScoped<IBookingAssistantService, BookingAssistantService>();

        // Functions exposed to Gemini (Function Calling). Register a new class here to add a new tool.
        services.AddScoped<IAssistantFunction, CheckAvailabilityFunction>();
        services.AddScoped<IAssistantFunction, GetBookingsByPhoneFunction>();
        services.AddScoped<IAssistantFunction, CancelBookingFunction>();

        return services;
    }
}
