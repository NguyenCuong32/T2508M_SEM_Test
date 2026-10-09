using System.Threading.Tasks;
using BattleGame.Functions.DTOs;
using BattleGame.Functions.Models;

namespace BattleGame.Functions.Services.Interfaces
{
    public interface IPlayerService
    {
        Task<Player> RegisterPlayerAsync(RegisterPlayerRequest request);
    }
}
