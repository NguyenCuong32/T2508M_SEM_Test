using BattleGameFunction.DTOs;
using BattleGameFunction.Models;

namespace BattleGameFunction.Services;

public interface IPlayerService
{
    Task<Player> RegisterPlayerAsync(RegisterPlayerRequest request);
    Task<IEnumerable<Player>> GetAllPlayersAsync();
    Task<Player?> GetPlayerByIdAsync(string playerId);
}
