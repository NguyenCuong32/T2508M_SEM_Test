using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleGame.Functions.DTOs;
using BattleGame.Functions.Models;
using BattleGame.Functions.Repositories.Interfaces;
using BattleGame.Functions.Services.Interfaces;

namespace BattleGame.Functions.Services
{
    public class PlayerAssetService : IPlayerAssetService
    {
        private readonly IPlayerAssetRepository _playerAssetRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IAssetRepository _assetRepository;

        public PlayerAssetService(
            IPlayerAssetRepository playerAssetRepository,
            IPlayerRepository playerRepository,
            IAssetRepository assetRepository)
        {
            _playerAssetRepository = playerAssetRepository;
            _playerRepository = playerRepository;
            _assetRepository = assetRepository;
        }

        public async Task<List<PlayerAssetReportItem>> GetReportAsync()
        {
            return await _playerAssetRepository.GetReportAsync();
        }

        public async Task<object> AssignAssetAsync(AssignAssetRequest request)
        {
            if (request.PlayerId == Guid.Empty || request.AssetId == Guid.Empty)
            {
                throw new ArgumentException("Valid PlayerId and AssetId are required.");
            }

            var player = await _playerRepository.GetByIdAsync(request.PlayerId);
            if (player == null)
            {
                throw new KeyNotFoundException($"Player with ID '{request.PlayerId}' not found.");
            }

            var asset = await _assetRepository.GetByIdAsync(request.AssetId);
            if (asset == null)
            {
                throw new KeyNotFoundException($"Asset with ID '{request.AssetId}' not found.");
            }

            bool alreadyAssigned = await _playerAssetRepository.ExistsAsync(request.PlayerId, request.AssetId);
            if (alreadyAssigned)
            {
                throw new InvalidOperationException("Player already possesses this asset.");
            }

            var playerAsset = new PlayerAsset
            {
                PlayerId = request.PlayerId,
                AssetId = request.AssetId
            };

            await _playerAssetRepository.AddAsync(playerAsset);
            await _playerAssetRepository.SaveChangesAsync();

            return new
            {
                playerId = playerAsset.PlayerId,
                playerName = player.PlayerName,
                assetId = playerAsset.AssetId,
                assetName = asset.AssetName
            };
        }

        public async Task<string> SeedDataAsync()
        {
            var p1 = new Player
            {
                PlayerId = Guid.NewGuid(),
                PlayerName = "Player 1",
                FullName = "Le Trung Kien",
                Age = "20",
                Level = 10,
                Email = "player1@battlegame.vn"
            };

            var p2 = new Player
            {
                PlayerId = Guid.NewGuid(),
                PlayerName = "Player 2",
                FullName = "Tran Van B",
                Age = "19",
                Level = 3,
                Email = "player2@battlegame.vn"
            };

            var p3 = new Player
            {
                PlayerId = Guid.NewGuid(),
                PlayerName = "Player 3",
                FullName = "Nguyen Thi C",
                Age = "23",
                Level = 10,
                Email = "player3@battlegame.vn"
            };

            var a1 = new Asset
            {
                AssetId = Guid.NewGuid(),
                AssetName = "Hero 1",
                LevelRequire = 10
            };

            var a2 = new Asset
            {
                AssetId = Guid.NewGuid(),
                AssetName = "Hero 2",
                LevelRequire = 3
            };

            await _playerRepository.AddAsync(p1);
            await _playerRepository.AddAsync(p2);
            await _playerRepository.AddAsync(p3);
            await _playerRepository.SaveChangesAsync();

            await _assetRepository.AddAsync(a1);
            await _assetRepository.AddAsync(a2);
            await _assetRepository.SaveChangesAsync();

            await _playerAssetRepository.AddAsync(new PlayerAsset { PlayerId = p1.PlayerId, AssetId = a1.AssetId });
            await _playerAssetRepository.AddAsync(new PlayerAsset { PlayerId = p2.PlayerId, AssetId = a2.AssetId });
            await _playerAssetRepository.AddAsync(new PlayerAsset { PlayerId = p3.PlayerId, AssetId = a1.AssetId });
            await _playerAssetRepository.SaveChangesAsync();

            return "Database seeded with sample exam records.";
        }
    }
}
