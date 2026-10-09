using BattleGame.DTOs;
using BattleGame.Repositories.Interfaces;
using BattleGame.Services.Interfaces;

namespace BattleGame.Services;

public sealed class PlayerAssetService(IPlayerAssetRepository repository) : IPlayerAssetService
{
    public Task<List<PlayerAssetReportDto>> GetAssetsByPlayerAsync() => repository.GetAssetsByPlayerAsync();
}
