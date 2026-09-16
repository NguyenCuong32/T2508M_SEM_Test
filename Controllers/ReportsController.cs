using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ComicSystem.Services;
using ComicSystem.ViewModels;

namespace ComicSystem.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IRentalService _rentalService;

        public ReportsController(IRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        // GET: Reports
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            var rentalDetailsList = await _rentalService.GetReportDetailsAsync(startDate, endDate);

            var reportItems = new List<ReportItemViewModel>();
            int stt = 1;

            foreach (var detail in rentalDetailsList)
            {
                reportItems.Add(new ReportItemViewModel
                {
                    No = stt++,
                    BookName = detail.ComicBook?.Title ?? "N/A",
                    RentalDate = detail.Rental?.RentalDate ?? DateTime.MinValue,
                    ReturnDate = detail.Rental?.ReturnDate ?? DateTime.MinValue,
                    CustomerName = detail.Rental?.Customer?.FullName ?? "N/A",
                    Quantity = detail.Quantity
                });
            }

            var viewModel = new ReportFilterViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                Reports = reportItems
            };

            return View(viewModel);
        }
    }
}
