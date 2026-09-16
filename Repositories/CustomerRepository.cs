using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ComicSystem.Data;
using ComicSystem.Models;

namespace ComicSystem.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ComicDbContext _context;

        public CustomerRepository(ComicDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers.ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers.FindAsync(id);
        }

        public async Task AddAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                // Remove all rentals for this customer
                var rentals = await _context.Rentals
                    .Include(r => r.RentalDetails)
                    .Where(r => r.CustomerID == id)
                    .ToListAsync();

                if (rentals.Count > 0)
                {
                    _context.Rentals.RemoveRange(rentals);
                }

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }
    }
}
