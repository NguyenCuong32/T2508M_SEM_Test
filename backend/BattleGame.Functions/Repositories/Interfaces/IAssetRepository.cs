using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleGame.Functions.Models;

namespace BattleGame.Functions.Repositories.Interfaces
{
    public interface IAssetRepository
    {
        Task<Asset?> GetByIdAsync(Guid assetId);
        Task<Asset?> GetByNameAsync(string assetName);
        Task<bool> ExistsByNameAsync(string assetName);
        Task<IEnumerable<Asset>> GetAllAsync();
        Task AddAsync(Asset asset);
        Task SaveChangesAsync();
    }
}
