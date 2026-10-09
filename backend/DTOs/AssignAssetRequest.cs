using System.ComponentModel.DataAnnotations;

namespace BattleGame.Api.DTOs;

public class AssignAssetRequest
{
    [Required(ErrorMessage = "PlayerId is required")]
    public Guid PlayerId { get; set; }

    [Required(ErrorMessage = "AssetId is required")]
    public Guid AssetId { get; set; }
}
