using BattleGameFunction.DTOs;
using BattleGameFunction.Models;
using BattleGameFunction.Repositories;

namespace BattleGameFunction.Services;

public class AssetService : IAssetService
{
    private readonly IAssetRepository _assetRepository;

    public AssetService(IAssetRepository assetRepository)
    {
        _assetRepository = assetRepository;
    }

    public async Task<Asset> CreateAssetAsync(CreateAssetRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AssetName))
        {
            throw new ArgumentException("AssetName is required.");
        }

        var asset = new Asset
        {
            AssetId = string.IsNullOrWhiteSpace(request.AssetId) ? Guid.NewGuid().ToString() : request.AssetId.Trim(),
            AssetName = request.AssetName.Trim(),
            LevelRequire = request.LevelRequire
        };

        await _assetRepository.AddAsync(asset);
        await _assetRepository.SaveChangesAsync();

        return asset;
    }

    public async Task<IEnumerable<Asset>> GetAllAssetsAsync()
    {
        return await _assetRepository.GetAllAsync();
    }

    public async Task<Asset?> GetAssetByIdAsync(string assetId)
    {
        return await _assetRepository.GetByIdAsync(assetId);
    }
}
