using BattleGame.DTOs;

namespace BattleGame.Repositories.Interfaces;

public interface IPlayerAssetRepository
{
    Task<List<PlayerAssetReportDto>> GetAssetsByPlayerAsync();
}
