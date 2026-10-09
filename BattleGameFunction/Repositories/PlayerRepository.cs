using Microsoft.EntityFrameworkCore;
using BattleGameFunction.Data;
using BattleGameFunction.Models;

namespace BattleGameFunction.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly BattleGameDbContext _context;

    public PlayerRepository(BattleGameDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Player>> GetAllAsync()
    {
        return await _context.Player.OrderBy(p => p.PlayerName).ToListAsync();
    }

    public async Task<Player?> GetByIdAsync(string playerId)
    {
        return await _context.Player.FindAsync(playerId);
    }

    public async Task<Player> AddAsync(Player player)
    {
        await _context.Player.AddAsync(player);
        return player;
    }

    public async Task<bool> ExistsAsync(string playerId)
    {
        return await _context.Player.AnyAsync(p => p.PlayerId == playerId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
