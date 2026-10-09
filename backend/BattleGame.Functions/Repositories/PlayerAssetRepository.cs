using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattleGame.Functions.Data;
using BattleGame.Functions.DTOs;
using BattleGame.Functions.Models;
using BattleGame.Functions.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BattleGame.Functions.Repositories
{
    public class PlayerAssetRepository : IPlayerAssetRepository
    {
        private readonly BattleGameDbContext _context;

        public PlayerAssetRepository(BattleGameDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(Guid playerId, Guid assetId)
        {
            return await _context.PlayerAssets
                .AnyAsync(pa => pa.PlayerId == playerId && pa.AssetId == assetId);
        }

        public async Task<List<PlayerAssetReportItem>> GetReportAsync()
        {
            var rawList = await _context.PlayerAssets
                .Include(pa => pa.Player)
                .Include(pa => pa.Asset)
                .OrderBy(pa => pa.Player!.PlayerName)
                .ThenBy(pa => pa.Asset!.AssetName)
                .Select(pa => new
                {
                    PlayerName = pa.Player != null ? pa.Player.PlayerName : string.Empty,
                    Level = pa.Player != null ? pa.Player.Level : 0,
                    Age = pa.Player != null ? pa.Player.Age : string.Empty,
                    AssetName = pa.Asset != null ? pa.Asset.AssetName : string.Empty,
                    PlayerId = pa.PlayerId,
                    AssetId = pa.AssetId
                })
                .ToListAsync();

            var result = new List<PlayerAssetReportItem>();
            int index = 1;
            foreach (var item in rawList)
            {
                result.Add(new PlayerAssetReportItem
                {
                    No = index++,
                    PlayerName = item.PlayerName,
                    Level = item.Level,
                    Age = item.Age,
                    AssetName = item.AssetName,
                    PlayerId = item.PlayerId,
                    AssetId = item.AssetId
                });
            }

            return result;
        }

        public async Task AddAsync(PlayerAsset playerAsset)
        {
            await _context.PlayerAssets.AddAsync(playerAsset);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
