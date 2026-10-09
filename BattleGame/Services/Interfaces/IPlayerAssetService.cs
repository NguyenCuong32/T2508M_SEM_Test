using BattleGame.DTOs;

namespace BattleGame.Services.Interfaces;

public interface IPlayerAssetService
{
    Task<List<PlayerAssetReportDto>> GetAssetsByPlayerAsync();
}
