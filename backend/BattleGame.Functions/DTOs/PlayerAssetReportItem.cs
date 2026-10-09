using System;
using System.Text.Json.Serialization;

namespace BattleGame.Functions.DTOs
{
    public class PlayerAssetReportItem
    {
        [JsonPropertyName("no")]
        public int No { get; set; }

        [JsonPropertyName("playerName")]
        public string PlayerName { get; set; } = string.Empty;

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("age")]
        public string Age { get; set; } = string.Empty;

        [JsonPropertyName("assetName")]
        public string AssetName { get; set; } = string.Empty;

        [JsonPropertyName("playerId")]
        public Guid? PlayerId { get; set; }

        [JsonPropertyName("assetId")]
        public Guid? AssetId { get; set; }
    }
}
