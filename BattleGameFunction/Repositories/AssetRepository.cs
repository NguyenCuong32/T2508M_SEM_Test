using Microsoft.EntityFrameworkCore;
using BattleGameFunction.Data;
using BattleGameFunction.Models;

namespace BattleGameFunction.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly BattleGameDbContext _context;

    public AssetRepository(BattleGameDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Asset>> GetAllAsync()
    {
        return await _context.Asset.OrderBy(a => a.AssetName).ToListAsync();
    }

    public async Task<Asset?> GetByIdAsync(string assetId)
    {
        return await _context.Asset.FindAsync(assetId);
    }

    public async Task<Asset> AddAsync(Asset asset)
    {
        await _context.Asset.AddAsync(asset);
        return asset;
    }

    public async Task<bool> ExistsAsync(string assetId)
    {
        return await _context.Asset.AnyAsync(a => a.AssetId == assetId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
