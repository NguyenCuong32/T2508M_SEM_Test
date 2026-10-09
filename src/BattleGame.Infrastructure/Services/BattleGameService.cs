using BattleGame.Core.Dtos;
using BattleGame.Core.Entities;
using BattleGame.Core.Interfaces;
using BattleGame.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BattleGame.Infrastructure.Services;

public class BattleGameService : IBattleGameService
{
    private readonly BattleGameDbContext _context;

    public BattleGameService(BattleGameDbContext context)
    {
        _context = context;
    }

    public async Task<Player> RegisterPlayerAsync(RegisterPlayerDto dto)
    {
        var player = new Player
        {
            PlayerId = Guid.NewGuid(),
            PlayerName = dto.PlayerName,
            FullName = dto.FullName,
            Age = dto.Age,
            Level = dto.Level,
            Email = dto.Email
        };

        _context.Players.Add(player);
        await _context.SaveChangesAsync();
        return player;
    }

    public async Task<Asset> CreateAssetAsync(CreateAssetDto dto)
    {
        var asset = new Asset
        {
            AssetId = Guid.NewGuid(),
            AssetName = dto.AssetName,
            LevelRequire = dto.LevelRequire
        };

        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();
        return asset;
    }

    public async Task<bool> AssignAssetAsync(AssignAssetDto dto)
    {
        var exists = await _context.PlayerAssets
            .AnyAsync(pa => pa.PlayerId == dto.PlayerId && pa.AssetId == dto.AssetId);

        if (exists)
        {
            return true;
        }

        var playerAsset = new PlayerAsset
        {
            PlayerId = dto.PlayerId,
            AssetId = dto.AssetId
        };

        _context.PlayerAssets.Add(playerAsset);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<PlayerAssetReportDto>> GetAssetsByPlayerAsync()
    {
        // Join PlayerAsset -> Player & Asset
        var query = await _context.PlayerAssets
            .Include(pa => pa.Player)
            .Include(pa => pa.Asset)
            .OrderBy(pa => pa.Player!.PlayerName)
            .Select(pa => new
            {
                PlayerId = pa.PlayerId,
                PlayerName = pa.Player != null ? pa.Player.PlayerName : string.Empty,
                Level = pa.Player != null ? pa.Player.Level : 0,
                Age = pa.Player != null ? pa.Player.Age : string.Empty,
                AssetId = pa.AssetId,
                AssetName = pa.Asset != null ? pa.Asset.AssetName : string.Empty
            })
            .ToListAsync();

        var result = new List<PlayerAssetReportDto>();
        int index = 1;
        foreach (var item in query)
        {
            result.Add(new PlayerAssetReportDto
            {
                No = index++,
                PlayerId = item.PlayerId,
                PlayerName = item.PlayerName,
                Level = item.Level,
                Age = item.Age,
                AssetId = item.AssetId,
                AssetName = item.AssetName
            });
        }

        // If no player assets exist yet, fallback to listing players with or without assets
        if (!result.Any())
        {
            var players = await _context.Players.ToListAsync();
            foreach (var p in players)
            {
                result.Add(new PlayerAssetReportDto
                {
                    No = index++,
                    PlayerId = p.PlayerId,
                    PlayerName = p.PlayerName,
                    Level = p.Level,
                    Age = p.Age,
                    AssetId = null,
                    AssetName = "N/A"
                });
            }
        }

        return result;
    }

    public async Task<IEnumerable<Player>> GetAllPlayersAsync()
    {
        return await _context.Players.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Asset>> GetAllAssetsAsync()
    {
        return await _context.Assets.AsNoTracking().ToListAsync();
    }

    public async Task SeedInitialDataAsync()
    {
        await _context.Database.EnsureCreatedAsync();

        if (await _context.Players.AnyAsync())
        {
            return;
        }

        var p1 = new Player { PlayerId = Guid.NewGuid(), PlayerName = "Player 1", FullName = "Nguyen Van A", Age = "20", Level = 10, Email = "player1@example.com" };
        var p2 = new Player { PlayerId = Guid.NewGuid(), PlayerName = "Player 2", FullName = "Tran Thi B", Age = "19", Level = 3, Email = "player2@example.com" };
        var p3 = new Player { PlayerId = Guid.NewGuid(), PlayerName = "Player 3", FullName = "Le Van C", Age = "23", Level = 10, Email = "player3@example.com" };

        var a1 = new Asset { AssetId = Guid.NewGuid(), AssetName = "Hero 1", LevelRequire = 5 };
        var a2 = new Asset { AssetId = Guid.NewGuid(), AssetName = "Hero 2", LevelRequire = 3 };
        var a3 = new Asset { AssetId = Guid.NewGuid(), AssetName = "Mythic Sword", LevelRequire = 10 };

        _context.Players.AddRange(p1, p2, p3);
        _context.Assets.AddRange(a1, a2, a3);
        await _context.SaveChangesAsync();

        _context.PlayerAssets.AddRange(
            new PlayerAsset { PlayerId = p1.PlayerId, AssetId = a1.AssetId },
            new PlayerAsset { PlayerId = p2.PlayerId, AssetId = a2.AssetId },
            new PlayerAsset { PlayerId = p3.PlayerId, AssetId = a1.AssetId }
        );

        await _context.SaveChangesAsync();
    }
}
