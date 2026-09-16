using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ComicSystem.Models;
using ComicSystem.Repositories;

namespace ComicSystem.Services
{
    public class RentalService : IRentalService
    {
        private readonly IRentalRepository _rentalRepository;

        public RentalService(IRentalRepository rentalRepository)
        {
            _rentalRepository = rentalRepository;
        }

        public async Task<IEnumerable<Rental>> GetAllAsync()
        {
            return await _rentalRepository.GetAllAsync();
        }

        public async Task AddRentalWithDetailAsync(Rental rental, RentalDetail detail)
        {
            await _rentalRepository.AddRentalWithDetailAsync(rental, detail);
        }

        public async Task<IEnumerable<RentalDetail>> GetReportDetailsAsync(DateTime? startDate, DateTime? endDate)
        {
            return await _rentalRepository.GetReportDetailsAsync(startDate, endDate);
        }
    }
}
