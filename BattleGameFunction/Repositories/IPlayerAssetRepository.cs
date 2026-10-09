using BattleGameFunction.DTOs;
using BattleGameFunction.Models;

namespace BattleGameFunction.Repositories;

public interface IPlayerAssetRepository
{
    Task<IEnumerable<PlayerAsset>> GetAllWithDetailsAsync();
    Task<PlayerAsset?> GetAsync(string playerId, string assetId);
    Task<PlayerAsset> AddAsync(PlayerAsset playerAsset);
    Task<bool> ExistsAsync(string playerId, string assetId);
    Task<List<PlayerAssetReportDto>> GetReportListAsync();
    Task SaveChangesAsync();
}
