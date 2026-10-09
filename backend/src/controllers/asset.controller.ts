import { HttpRequest, HttpResponseInit } from "@azure/functions";
import { AssetService } from "../services/asset.service";
import { CreateAssetDto } from "../models/asset.model";
import { successResponse, errorResponse } from "../shared/response.helper";

/**
 * Controller for asset-related HTTP actions.
 */
export class AssetController {
  private readonly assetService: AssetService;

  constructor() {
    this.assetService = new AssetService();
  }

  /**
   * POST /api/createasset
   * Creates a new game asset.
   */
  async createAsset(req: HttpRequest): Promise<HttpResponseInit> {
    try {
      const body = (await req.json()) as CreateAssetDto;
      const asset = await this.assetService.createAsset(body);

      return {
        status: 201,
        jsonBody: successResponse(asset, "Asset created successfully."),
      };
    } catch (err) {
      const message = err instanceof Error ? err.message : "Unexpected error.";
      return {
        status: 400,
        jsonBody: errorResponse(message),
      };
    }
  }
}
