using SunriseHotel.Models;
using SunriseHotel.Repositories;
namespace SunriseHotel.Services;

public class ReportService(IReportRepository repository) : IReportService
{
    public Task<List<ReportRow>> GetBookings(DateOnly from, DateOnly to) => repository.Report(from, to);
}
