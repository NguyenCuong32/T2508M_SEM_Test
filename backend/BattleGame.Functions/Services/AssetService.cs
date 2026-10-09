using System;
using System.Threading.Tasks;
using BattleGame.Functions.DTOs;
using BattleGame.Functions.Models;
using BattleGame.Functions.Repositories.Interfaces;
using BattleGame.Functions.Services.Interfaces;

namespace BattleGame.Functions.Services
{
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

            bool exists = await _assetRepository.ExistsByNameAsync(request.AssetName.Trim());
            if (exists)
            {
                throw new InvalidOperationException($"Asset with name '{request.AssetName}' already exists.");
            }

            var asset = new Asset
            {
                AssetId = Guid.NewGuid(),
                AssetName = request.AssetName.Trim(),
                LevelRequire = request.LevelRequire < 1 ? 1 : request.LevelRequire
            };

            await _assetRepository.AddAsync(asset);
            await _assetRepository.SaveChangesAsync();

            return asset;
        }
    }
}
