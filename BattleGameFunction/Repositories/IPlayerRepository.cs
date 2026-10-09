using BattleGameFunction.Models;

namespace BattleGameFunction.Repositories;

public interface IPlayerRepository
{
    Task<IEnumerable<Player>> GetAllAsync();
    Task<Player?> GetByIdAsync(string playerId);
    Task<Player> AddAsync(Player player);
    Task<bool> ExistsAsync(string playerId);
    Task SaveChangesAsync();
}
