using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleGame.Functions.Data;
using BattleGame.Functions.Models;
using BattleGame.Functions.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BattleGame.Functions.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly BattleGameDbContext _context;

        public PlayerRepository(BattleGameDbContext context)
        {
            _context = context;
        }

        public async Task<Player?> GetByIdAsync(Guid playerId)
        {
            return await _context.Players.FindAsync(playerId);
        }

        public async Task<Player?> GetByNameAsync(string playerName)
        {
            return await _context.Players
                .FirstOrDefaultAsync(p => p.PlayerName.ToLower() == playerName.ToLower());
        }

        public async Task<bool> ExistsByNameAsync(string playerName)
        {
            return await _context.Players
                .AnyAsync(p => p.PlayerName.ToLower() == playerName.ToLower());
        }

        public async Task<IEnumerable<Player>> GetAllAsync()
        {
            return await _context.Players.AsNoTracking().ToListAsync();
        }

        public async Task AddAsync(Player player)
        {
            await _context.Players.AddAsync(player);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
