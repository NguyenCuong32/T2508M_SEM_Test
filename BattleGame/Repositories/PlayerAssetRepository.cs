using BattleGame.Data;
using BattleGame.DTOs;
using BattleGame.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BattleGame.Repositories;

public sealed class PlayerAssetRepository(BattleGameDbContext context) : IPlayerAssetRepository
{
    public async Task<List<PlayerAssetReportDto>> GetAssetsByPlayerAsync()
    {
        var report = await context.PlayerAssets.AsNoTracking()
            .OrderBy(playerAsset => playerAsset.Player.PlayerName)
            .ThenBy(playerAsset => playerAsset.PlayerId)
            .ThenBy(playerAsset => playerAsset.Asset.AssetName)
            .ThenBy(playerAsset => playerAsset.AssetId)
            .Select(playerAsset => new PlayerAssetReportDto
            {
                PlayerName = playerAsset.Player.PlayerName,
                Level = playerAsset.Player.Level,
                Age = playerAsset.Player.Age,
                AssetName = playerAsset.Asset.AssetName
            }).ToListAsync();
        for (var index = 0; index < report.Count; index++)
            report[index].No = index + 1;
        return report;
    }
}
