using SunriseHotel.Models;
namespace SunriseHotel.Services;

public interface IReportService
{
    Task<List<ReportRow>> GetBookings(DateOnly from, DateOnly to);
}
