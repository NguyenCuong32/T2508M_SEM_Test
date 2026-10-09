using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EXAM_Pram.Data;
using EXAM_Pram.DTOs;
using EXAM_Pram.Models;

namespace EXAM_Pram.Controllers
{
    [ApiController]
    [Route("api")]
    public class BattleGameController : ControllerBase
    {
        private readonly BattleGameDbContext _context;
        private readonly ILogger<BattleGameController> _logger;

        public BattleGameController(BattleGameDbContext context, ILogger<BattleGameController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Requirement 1: Register a new player
        /// POST /api/registerplayer (also matches /registerplayer)
        /// </summary>
        [HttpPost("registerplayer")]
        [HttpPost("/registerplayer")]
        public async Task<IActionResult> RegisterPlayer([FromBody] RegisterPlayerDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var player = new Player
            {
                PlayerId = Guid.NewGuid(),
                PlayerName = dto.PlayerName.Trim(),
                FullName = dto.FullName?.Trim(),
                Age = dto.Age?.Trim(),
                Level = dto.Level < 1 ? 1 : dto.Level,
                Email = dto.Email?.Trim()
            };

            _context.Players.Add(player);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully registered player: {PlayerName} with ID: {PlayerId}", player.PlayerName, player.PlayerId);

            return CreatedAtAction(nameof(GetPlayers), new { id = player.PlayerId }, new
            {
                message = "Player registered successfully",
                data = player
            });
        }

        /// <summary>
        /// Requirement 2: Admin creates a new asset
        /// POST /api/createasset (also matches /createasset)
        /// </summary>
        [HttpPost("createasset")]
        [HttpPost("/createasset")]
        public async Task<IActionResult> CreateAsset([FromBody] CreateAssetDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var asset = new Asset
            {
                AssetId = Guid.NewGuid(),
                AssetName = dto.AssetName.Trim(),
                LevelRequire = dto.LevelRequire < 1 ? 1 : dto.LevelRequire
            };

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully created asset: {AssetName} with ID: {AssetId}", asset.AssetName, asset.AssetId);

            return CreatedAtAction(nameof(GetAssets), new { id = asset.AssetId }, new
            {
                message = "Asset created successfully",
                data = asset
            });
        }

        /// <summary>
        /// Requirement 3: Admin report assets of player
        /// Columns: No, Player name, Level, Age, Asset name
        /// GET /api/getassetsbyplayer (also matches /getassetsbyplayer)
        /// </summary>
        [HttpGet("getassetsbyplayer")]
        [HttpGet("/getassetsbyplayer")]
        public async Task<ActionResult<IEnumerable<PlayerAssetReportDto>>> GetAssetsByPlayer()
        {
            var rawData = await (from pa in _context.PlayerAssets
                                 join p in _context.Players on pa.PlayerId equals p.PlayerId
                                 join a in _context.Assets on pa.AssetId equals a.AssetId
                                 orderby p.PlayerName, a.AssetName
                                 select new
                                 {
                                     PlayerName = p.PlayerName,
                                     Level = p.Level,
                                     Age = p.Age,
                                     AssetName = a.AssetName
                                 }).ToListAsync();

            var reportList = rawData.Select((item, index) => new PlayerAssetReportDto
            {
                No = index + 1,
                PlayerName = item.PlayerName,
                Level = item.Level,
                Age = item.Age,
                AssetName = item.AssetName
            }).ToList();

            return Ok(reportList);
        }

        /// <summary>
        /// Helper: Get all players
        /// </summary>
        [HttpGet("players")]
        public async Task<IActionResult> GetPlayers()
        {
            var players = await _context.Players.ToListAsync();
            return Ok(players);
        }

        /// <summary>
        /// Helper: Get all assets
        /// </summary>
        [HttpGet("assets")]
        public async Task<IActionResult> GetAssets()
        {
            var assets = await _context.Assets.ToListAsync();
            return Ok(assets);
        }

        /// <summary>
        /// Helper: Assign an asset to a player
        /// </summary>
        [HttpPost("assignasset")]
        public async Task<IActionResult> AssignAsset([FromBody] AssignAssetDto dto)
        {
            var playerExists = await _context.Players.AnyAsync(p => p.PlayerId == dto.PlayerId);
            if (!playerExists)
            {
                return NotFound(new { message = "Player not found" });
            }

            var assetExists = await _context.Assets.AnyAsync(a => a.AssetId == dto.AssetId);
            if (!assetExists)
            {
                return NotFound(new { message = "Asset not found" });
            }

            var alreadyAssigned = await _context.PlayerAssets
                .AnyAsync(pa => pa.PlayerId == dto.PlayerId && pa.AssetId == dto.AssetId);

            if (alreadyAssigned)
            {
                return BadRequest(new { message = "Asset is already assigned to this player" });
            }

            var playerAsset = new PlayerAsset
            {
                PlayerId = dto.PlayerId,
                AssetId = dto.AssetId
            };

            _context.PlayerAssets.Add(playerAsset);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Asset assigned to player successfully" });
        }
    }
}
