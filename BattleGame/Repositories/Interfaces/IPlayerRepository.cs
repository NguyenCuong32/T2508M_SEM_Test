using BattleGame.Models;

namespace BattleGame.Repositories.Interfaces;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(Guid id);
    Task<Player?> GetByEmailAsync(string email);
    Task<Player> AddAsync(Player player);
}
