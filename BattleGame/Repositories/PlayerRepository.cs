using BattleGame.Data;
using BattleGame.Models;
using BattleGame.Repositories.Interfaces;
using BattleGame.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BattleGame.Repositories;

public sealed class PlayerRepository(BattleGameDbContext context) : IPlayerRepository
{
    public Task<Player?> GetByIdAsync(Guid id) =>
        context.Players.AsNoTracking().SingleOrDefaultAsync(player => player.PlayerId == id);

    public Task<Player?> GetByEmailAsync(string email) =>
        context.Players.AsNoTracking().SingleOrDefaultAsync(player => player.Email == email);

    public async Task<Player> AddAsync(Player player)
    {
        context.Players.Add(player);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new DuplicateEmailException();
        }
        return player;
    }
}
