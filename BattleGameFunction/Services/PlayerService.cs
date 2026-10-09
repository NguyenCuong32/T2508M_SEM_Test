using BattleGameFunction.DTOs;
using BattleGameFunction.Models;
using BattleGameFunction.Repositories;

namespace BattleGameFunction.Services;

public class PlayerService : IPlayerService
{
    private readonly IPlayerRepository _playerRepository;

    public PlayerService(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public async Task<Player> RegisterPlayerAsync(RegisterPlayerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PlayerName))
        {
            throw new ArgumentException("PlayerName is required.");
        }

        var player = new Player
        {
            PlayerId = string.IsNullOrWhiteSpace(request.PlayerId) ? Guid.NewGuid().ToString() : request.PlayerId.Trim(),
            PlayerName = request.PlayerName.Trim(),
            FullName = request.FullName?.Trim() ?? string.Empty,
            Age = string.IsNullOrWhiteSpace(request.Age) ? "0" : request.Age.Trim(),
            Level = request.Level,
            Email = request.Email?.Trim() ?? string.Empty
        };

        await _playerRepository.AddAsync(player);
        await _playerRepository.SaveChangesAsync();

        return player;
    }

    public async Task<IEnumerable<Player>> GetAllPlayersAsync()
    {
        return await _playerRepository.GetAllAsync();
    }

    public async Task<Player?> GetPlayerByIdAsync(string playerId)
    {
        return await _playerRepository.GetByIdAsync(playerId);
    }
}
