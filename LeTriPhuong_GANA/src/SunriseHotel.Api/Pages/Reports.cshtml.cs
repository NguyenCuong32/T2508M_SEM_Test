using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SunriseHotel.Application.Common.Exceptions;
using SunriseHotel.Application.Features.Reports;

namespace SunriseHotel.Api.Pages;

/// <summary>Requirement 5: website showing the booking report (requirement 4) with a date-range filter.</summary>
public class ReportsModel(IBookingReportService reportService) : PageModel
{
    [BindProperty(SupportsGet = true)] public DateOnly? From { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? To { get; set; }

    public BookingReportDto? Report { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        try
        {
            Report = await reportService.GetReportAsync(From, To, ct);
        }
        catch (ValidationException ex)
        {
            ErrorMessage = string.Join(" ", ex.Errors);
        }
    }
}
