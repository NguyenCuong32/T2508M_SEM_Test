using BattleGame.DTOs;
using BattleGame.Models;

namespace BattleGame.Services.Interfaces;

public interface IPlayerService
{
    Task<Player> RegisterPlayerAsync(RegisterPlayerDto dto);
}
