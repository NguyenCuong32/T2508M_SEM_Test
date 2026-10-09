using BattleGameFunction.DTOs;
using BattleGameFunction.Models;
using BattleGameFunction.Repositories;

namespace BattleGameFunction.Services;

public class ReportService : IReportService
{
    private readonly IPlayerAssetRepository _playerAssetRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IAssetRepository _assetRepository;

    public ReportService(
        IPlayerAssetRepository playerAssetRepository,
        IPlayerRepository playerRepository,
        IAssetRepository assetRepository)
    {
        _playerAssetRepository = playerAssetRepository;
        _playerRepository = playerRepository;
        _assetRepository = assetRepository;
    }

    public async Task<List<PlayerAssetReportDto>> GetAssetsByPlayerReportAsync()
    {
        return await _playerAssetRepository.GetReportListAsync();
    }

    public async Task<object> AssignAssetToPlayerAsync(AssignAssetRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PlayerId) || string.IsNullOrWhiteSpace(request.AssetId))
        {
            throw new ArgumentException("PlayerId and AssetId are required.");
        }

        var player = await _playerRepository.GetByIdAsync(request.PlayerId);
        if (player == null)
        {
            throw new KeyNotFoundException($"Player with ID '{request.PlayerId}' not found.");
        }

        var asset = await _assetRepository.GetByIdAsync(request.AssetId);
        if (asset == null)
        {
            throw new KeyNotFoundException($"Asset with ID '{request.AssetId}' not found.");
        }

        var alreadyAssigned = await _playerAssetRepository.ExistsAsync(request.PlayerId, request.AssetId);
        if (alreadyAssigned)
        {
            throw new InvalidOperationException("This asset is already assigned to the player.");
        }

        var playerAsset = new PlayerAsset
        {
            PlayerId = request.PlayerId,
            AssetId = request.AssetId
        };

        await _playerAssetRepository.AddAsync(playerAsset);
        await _playerAssetRepository.SaveChangesAsync();

        return new
        {
            playerId = player.PlayerId,
            playerName = player.PlayerName,
            assetId = asset.AssetId,
            assetName = asset.AssetName
        };
    }
}
