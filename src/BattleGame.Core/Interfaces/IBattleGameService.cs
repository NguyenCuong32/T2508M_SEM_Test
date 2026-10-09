using BattleGame.Core.Dtos;
using BattleGame.Core.Entities;

namespace BattleGame.Core.Interfaces;

public interface IBattleGameService
{
    Task<Player> RegisterPlayerAsync(RegisterPlayerDto dto);

    Task<Asset> CreateAssetAsync(CreateAssetDto dto);

    Task<bool> AssignAssetAsync(AssignAssetDto dto);

    Task<IEnumerable<PlayerAssetReportDto>> GetAssetsByPlayerAsync();

    Task<IEnumerable<Player>> GetAllPlayersAsync();

    Task<IEnumerable<Asset>> GetAllAssetsAsync();

    Task SeedInitialDataAsync();
}
