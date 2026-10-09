using BattleGameFunction.DTOs;
using BattleGameFunction.Models;

namespace BattleGameFunction.Services;

public interface IAssetService
{
    Task<Asset> CreateAssetAsync(CreateAssetRequest request);
    Task<IEnumerable<Asset>> GetAllAssetsAsync();
    Task<Asset?> GetAssetByIdAsync(string assetId);
}
