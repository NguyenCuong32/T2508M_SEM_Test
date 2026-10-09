using BattleGame.Models;

namespace BattleGame.Repositories.Interfaces;

public interface IAssetRepository
{
    Task<Asset?> GetByIdAsync(Guid id);
    Task<Asset> AddAsync(Asset asset);
}
