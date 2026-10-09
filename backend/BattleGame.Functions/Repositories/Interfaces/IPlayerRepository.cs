using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleGame.Functions.Models;

namespace BattleGame.Functions.Repositories.Interfaces
{
    public interface IPlayerRepository
    {
        Task<Player?> GetByIdAsync(Guid playerId);
        Task<Player?> GetByNameAsync(string playerName);
        Task<bool> ExistsByNameAsync(string playerName);
        Task<IEnumerable<Player>> GetAllAsync();
        Task AddAsync(Player player);
        Task SaveChangesAsync();
    }
}
