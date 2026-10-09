using BattleGameFunction.DTOs;

namespace BattleGameFunction.Services;

public interface IReportService
{
    Task<List<PlayerAssetReportDto>> GetAssetsByPlayerReportAsync();
    Task<object> AssignAssetToPlayerAsync(AssignAssetRequest request);
}
