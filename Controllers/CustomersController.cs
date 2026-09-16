using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ComicSystem.Models;
using ComicSystem.Services;

namespace ComicSystem.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            var customers = await _customerService.GetAllAsync();
            return View(customers);
        }

        // GET: Customers/Register
        public IActionResult Register()
        {
            var customer = new Customer
            {
                RegisterDate = DateTime.Today
            };
            return View(customer);
        }

        // POST: Customers/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("CustomerID,FullName,PhoneNumber,RegisterDate")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                await _customerService.AddAsync(customer);
                TempData["SuccessMessage"] = $"Đăng ký thành công cho khách hàng: {customer.FullName}!";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _customerService.GetByIdAsync(id.Value);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _customerService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Đã xóa khách hàng và toàn bộ đơn cho thuê liên quan!";
            return RedirectToAction(nameof(Index));
        }
    }
}
