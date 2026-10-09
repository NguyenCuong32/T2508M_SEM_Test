import { AssetRepository } from "../repositories/asset.repository";
import { Asset, CreateAssetDto } from "../models/asset.model";

/**
 * Business-logic layer for Asset operations.
 * Validates input before delegating persistence to the repository.
 */
export class AssetService {
  private readonly assetRepo: AssetRepository;

  constructor() {
    this.assetRepo = new AssetRepository();
  }

  /**
   * Creates a new asset after validating the required fields.
   * @throws Error when required fields are missing or LevelRequire < 1.
   */
  async createAsset(dto: CreateAssetDto): Promise<Asset> {
    if (!dto.AssetName || dto.AssetName.trim() === "") {
      throw new Error("AssetName is required.");
    }
    if (dto.LevelRequire === undefined || dto.LevelRequire === null) {
      throw new Error("LevelRequire is required.");
    }
    if (dto.LevelRequire < 1) {
      throw new Error("LevelRequire must be at least 1.");
    }

    return this.assetRepo.create({
      AssetName: dto.AssetName.trim(),
      LevelRequire: dto.LevelRequire,
    });
  }

  async getAllAssets(): Promise<Asset[]> {
    return this.assetRepo.findAll();
  }
}
