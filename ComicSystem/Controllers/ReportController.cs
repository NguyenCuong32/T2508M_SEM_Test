using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ComicSystem.Models;

namespace ComicSystem.Controllers;

public class ReportController : Controller
{
    private readonly ComicDbContext _context;

    public ReportController(ComicDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.RentalDetails
            .Include(rd => rd.ComicBook)
            .Include(rd => rd.Rental)
                .ThenInclude(r => r!.Customer)
            .AsQueryable();

        // Lọc theo khoảng ngày thuê (RentalDate)
        if (startDate.HasValue)
        {
            query = query.Where(rd => rd.Rental!.RentalDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(rd => rd.Rental!.RentalDate <= endDate.Value);
        }

        var reportData = await query.Select(rd => new RentalReportViewModel
        {
            BookName = rd.ComicBook!.Title,
            RentalDate = rd.Rental!.RentalDate,
            ReturnDate = rd.Rental!.ReturnDate,
            CustomerName = rd.Rental!.Customer!.FullName,
            Quantity = rd.Quantity
        }).ToListAsync();

        ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
        ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

        return View(reportData);
    }
}