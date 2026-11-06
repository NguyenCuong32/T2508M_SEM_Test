using SunriseHotel.Models;
namespace SunriseHotel.Repositories;

public interface IReportRepository { Task<List<ReportRow>> Report(DateOnly from, DateOnly to); }
