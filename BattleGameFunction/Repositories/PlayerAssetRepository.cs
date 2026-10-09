using Microsoft.EntityFrameworkCore;
using BattleGameFunction.Data;
using BattleGameFunction.DTOs;
using BattleGameFunction.Models;

namespace BattleGameFunction.Repositories;

public class PlayerAssetRepository : IPlayerAssetRepository
{
    private readonly BattleGameDbContext _context;

    public PlayerAssetRepository(BattleGameDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PlayerAsset>> GetAllWithDetailsAsync()
    {
        return await _context.PlayerAsset
            .Include(pa => pa.Player)
            .Include(pa => pa.Asset)
            .OrderBy(pa => pa.Player!.PlayerName)
            .ThenBy(pa => pa.Asset!.AssetName)
            .ToListAsync();
    }

    public async Task<PlayerAsset?> GetAsync(string playerId, string assetId)
    {
        return await _context.PlayerAsset.FindAsync(playerId, assetId);
    }

    public async Task<PlayerAsset> AddAsync(PlayerAsset playerAsset)
    {
        await _context.PlayerAsset.AddAsync(playerAsset);
        return playerAsset;
    }

    public async Task<bool> ExistsAsync(string playerId, string assetId)
    {
        return await _context.PlayerAsset.AnyAsync(pa => pa.PlayerId == playerId && pa.AssetId == assetId);
    }

    public async Task<List<PlayerAssetReportDto>> GetReportListAsync()
    {
        var rawList = await _context.PlayerAsset
            .Include(pa => pa.Player)
            .Include(pa => pa.Asset)
            .OrderBy(pa => pa.Player!.PlayerName)
            .ThenBy(pa => pa.Asset!.AssetName)
            .Select(pa => new
            {
                PlayerName = pa.Player != null ? pa.Player.PlayerName : string.Empty,
                Level = pa.Player != null ? pa.Player.Level : 0,
                Age = pa.Player != null ? pa.Player.Age : string.Empty,
                AssetName = pa.Asset != null ? pa.Asset.AssetName : string.Empty
            })
            .ToListAsync();

        return rawList.Select((item, index) => new PlayerAssetReportDto
        {
            No = index + 1,
            PlayerName = item.PlayerName,
            Level = item.Level,
            Age = item.Age,
            AssetName = item.AssetName
        }).ToList();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
