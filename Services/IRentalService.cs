using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ComicSystem.Models;

namespace ComicSystem.Services
{
    public interface IRentalService
    {
        Task<IEnumerable<Rental>> GetAllAsync();
        Task AddRentalWithDetailAsync(Rental rental, RentalDetail detail);
        Task<IEnumerable<RentalDetail>> GetReportDetailsAsync(DateTime? startDate, DateTime? endDate);
    }
}
