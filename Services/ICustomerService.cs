using System.Collections.Generic;
using System.Threading.Tasks;
using ComicSystem.Models;

namespace ComicSystem.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task AddAsync(Customer customer);
        Task DeleteAsync(int id);
    }
}
