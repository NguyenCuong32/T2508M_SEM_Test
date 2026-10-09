using System.Collections.Generic;
using System.Threading.Tasks;
using BattleGame.Functions.DTOs;

namespace BattleGame.Functions.Services.Interfaces
{
    public interface IPlayerAssetService
    {
        Task<List<PlayerAssetReportItem>> GetReportAsync();
        Task<object> AssignAssetAsync(AssignAssetRequest request);
        Task<string> SeedDataAsync();
    }
}
