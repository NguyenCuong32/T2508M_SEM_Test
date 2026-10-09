using BattleGame.DTOs;
using BattleGame.Models;

namespace BattleGame.Services.Interfaces;

public interface IAssetService
{
    Task<Asset> CreateAssetAsync(CreateAssetDto dto);
}
