using BattleGame.Data;
using BattleGame.Models;
using BattleGame.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BattleGame.Repositories;

public sealed class AssetRepository(BattleGameDbContext context) : IAssetRepository
{
    public Task<Asset?> GetByIdAsync(Guid id) =>
        context.Assets.AsNoTracking().SingleOrDefaultAsync(asset => asset.AssetId == id);

    public async Task<Asset> AddAsync(Asset asset)
    {
        context.Assets.Add(asset);
        await context.SaveChangesAsync();
        return asset;
    }
}
