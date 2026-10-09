using System.ComponentModel.DataAnnotations;
using BattleGame.DTOs;
using BattleGame.Models;
using BattleGame.Repositories.Interfaces;
using BattleGame.Services.Interfaces;

namespace BattleGame.Services;

public sealed class AssetService(IAssetRepository repository) : IAssetService
{
    public async Task<Asset> CreateAssetAsync(CreateAssetDto dto)
    {
        var assetName = InputValidation.RequiredText(dto.AssetName, "AssetName", 64);
        if (dto.LevelRequire < 0)
            throw new ValidationException("LevelRequire must be greater than or equal to zero.");
        return await repository.AddAsync(new Asset
        {
            AssetId = Guid.NewGuid(), AssetName = assetName, LevelRequire = dto.LevelRequire
        });
    }
}
