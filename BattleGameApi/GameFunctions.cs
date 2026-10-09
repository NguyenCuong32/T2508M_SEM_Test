using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BattleGameApi
{
    public class GameFunctions
    {
        // 1. API: registerplayer
        [Function("registerplayer")]
        public async Task<IActionResult> RegisterPlayer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var player = JsonConvert.DeserializeObject<Player>(requestBody);
            
            if (string.IsNullOrEmpty(player.PlayerId))
            {
                player.PlayerId = Guid.NewGuid().ToString();
            }

            using (var db = new AppDbContext())
            {
                db.Players.Add(player);
                await db.SaveChangesAsync();
            }

            return new OkObjectResult(player);
        }

        // 2. API: createasset
        [Function("createasset")]
        public async Task<IActionResult> CreateAsset(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var asset = JsonConvert.DeserializeObject<Asset>(requestBody);

            if (string.IsNullOrEmpty(asset.AssetId))
            {
                asset.AssetId = Guid.NewGuid().ToString();
            }

            using (var db = new AppDbContext())
            {
                db.Assets.Add(asset);
                await db.SaveChangesAsync();
            }

            return new OkObjectResult(asset);
        }

        // 3. API: getassetsbyplayer
        [Function("getassetsbyplayer")]
        public async Task<IActionResult> GetAssetsByPlayer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            using (var db = new AppDbContext())
            {
                var result = await (from pa in db.PlayerAssets
                                    join p in db.Players on pa.PlayerId equals p.PlayerId
                                    join a in db.Assets on pa.AssetId equals a.AssetId
                                    select new
                                    {
                                        PlayerName = p.PlayerName,
                                        Level = p.Level,
                                        Age = p.Age,
                                        AssetName = a.AssetName
                                    }).ToListAsync();

                return new OkObjectResult(result);
            }
        }
    }
}