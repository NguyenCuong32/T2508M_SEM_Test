import { PlayerAssetRepository } from "../repositories/player-asset.repository";
import { PlayerAssetReport } from "../models/player-asset.model";

/**
 * Business-logic layer for the PlayerAsset report.
 */
export class PlayerAssetService {
  private readonly playerAssetRepo: PlayerAssetRepository;

  constructor() {
    this.playerAssetRepo = new PlayerAssetRepository();
  }

  /**
   * Returns the full player-asset report.
   * Each row contains: No, PlayerName, Level, Age, AssetName.
   */
  async getAssetsReport(): Promise<PlayerAssetReport[]> {
    return this.playerAssetRepo.getAssetsReport();
  }
}
