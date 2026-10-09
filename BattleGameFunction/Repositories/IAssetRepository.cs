using BattleGameFunction.Models;

namespace BattleGameFunction.Repositories;

public interface IAssetRepository
{
    Task<IEnumerable<Asset>> GetAllAsync();
    Task<Asset?> GetByIdAsync(string assetId);
    Task<Asset> AddAsync(Asset asset);
    Task<bool> ExistsAsync(string assetId);
    Task SaveChangesAsync();
}
