using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ComicSystem.Models;
using ComicSystem.Services;

namespace ComicSystem.Controllers
{
    [ApiController]
    [Route("api/customers")]
    [Route("api/khachhang")]
    public class CustomersApiController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersApiController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // GET: api/customers or api/khachhang
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            var customers = await _customerService.GetAllAsync();
            return Ok(customers);
        }

        // GET: api/customers/5 or api/khachhang/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound(new { message = $"Không tìm thấy khách hàng với ID = {id}" });
            }
            return Ok(customer);
        }
    }
}
