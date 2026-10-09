using System;
using System.Threading.Tasks;
using BattleGame.Functions.DTOs;
using BattleGame.Functions.Models;
using BattleGame.Functions.Repositories.Interfaces;
using BattleGame.Functions.Services.Interfaces;

namespace BattleGame.Functions.Services
{
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

            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                throw new ArgumentException("FullName is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Age))
            {
                throw new ArgumentException("Age is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ArgumentException("Email is required.");
            }

            bool exists = await _playerRepository.ExistsByNameAsync(request.PlayerName.Trim());
            if (exists)
            {
                throw new InvalidOperationException($"Player with name '{request.PlayerName}' already exists.");
            }

            var player = new Player
            {
                PlayerId = Guid.NewGuid(),
                PlayerName = request.PlayerName.Trim(),
                FullName = request.FullName.Trim(),
                Age = request.Age.Trim(),
                Level = request.Level < 1 ? 1 : request.Level,
                Email = request.Email.Trim()
            };

            await _playerRepository.AddAsync(player);
            await _playerRepository.SaveChangesAsync();

            return player;
        }
    }
}
