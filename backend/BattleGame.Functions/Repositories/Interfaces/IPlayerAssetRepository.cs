using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleGame.Functions.DTOs;
using BattleGame.Functions.Models;

namespace BattleGame.Functions.Repositories.Interfaces
{
    public interface IPlayerAssetRepository
    {
        Task<bool> ExistsAsync(Guid playerId, Guid assetId);
        Task<List<PlayerAssetReportItem>> GetReportAsync();
        Task AddAsync(PlayerAsset playerAsset);
        Task SaveChangesAsync();
    }
}
