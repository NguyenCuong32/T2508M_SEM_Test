import { HttpResponseInit } from "@azure/functions";
import { PlayerAssetService } from "../services/player-asset.service";
import { successResponse, errorResponse } from "../shared/response.helper";

/**
 * Controller for the PlayerAsset report endpoint.
 */
export class PlayerAssetController {
  private readonly playerAssetService: PlayerAssetService;

  constructor() {
    this.playerAssetService = new PlayerAssetService();
  }

  /**
   * GET /api/getassetsbyplayer
   * Returns the full player-asset report table.
   */
  async getAssetsByPlayer(): Promise<HttpResponseInit> {
    try {
      const report = await this.playerAssetService.getAssetsReport();
      return {
        status: 200,
        jsonBody: successResponse(report, "Report fetched successfully."),
      };
    } catch (err) {
      const message = err instanceof Error ? err.message : "Unexpected error.";
      return {
        status: 500,
        jsonBody: errorResponse(message),
      };
    }
  }
}
