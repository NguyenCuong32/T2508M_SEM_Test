using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ComicSystem.Models;

namespace ComicSystem.Controllers;

public class RentalsController : Controller
{
    private readonly ComicDbContext _context;

    public RentalsController(ComicDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.CustomerID = new SelectList(_context.Customers, "CustomerID", "FullName");
        ViewBag.ComicBookID = new SelectList(_context.ComicBooks, "ComicBookID", "Title");
        return View(new RentalViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RentalViewModel model)
    {
        if (ModelState.IsValid)
        {
            var book = await _context.ComicBooks.FindAsync(model.ComicBookID);
            if (book != null)
            {
                // Lưu vào bảng Rentals
                var rental = new Rental
                {
                    CustomerID = model.CustomerID,
                    RentalDate = model.RentalDate,
                    ReturnDate = model.ReturnDate,
                    Status = "Đang thuê"
                };
                _context.Rentals.Add(rental);
                await _context.SaveChangesAsync();

                // Lưu tiếp vào bảng RentalDetails với RentalID vừa tạo
                var detail = new RentalDetail
                {
                    RentalID = rental.RentalID,
                    ComicBookID = book.ComicBookID,
                    Quantity = model.Quantity,
                    PricePerDay = book.PricePerDay
                };
                _context.RentalDetails.Add(detail);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Cho thuê sách thành công!";
                return RedirectToAction(nameof(Create));
            }
        }

        ViewBag.CustomerID = new SelectList(_context.Customers, "CustomerID", "FullName", model.CustomerID);
        ViewBag.ComicBookID = new SelectList(_context.ComicBooks, "ComicBookID", "Title", model.ComicBookID);
        return View(model);
    }
}