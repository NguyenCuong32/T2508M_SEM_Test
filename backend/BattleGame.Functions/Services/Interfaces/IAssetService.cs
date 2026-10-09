using System.Threading.Tasks;
using BattleGame.Functions.DTOs;
using BattleGame.Functions.Models;

namespace BattleGame.Functions.Services.Interfaces
{
    public interface IAssetService
    {
        Task<Asset> CreateAssetAsync(CreateAssetRequest request);
    }
}
