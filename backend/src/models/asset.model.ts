/**
 * Represents a row in the [Asset] table.
 */
export interface Asset {
  AssetId: string;
  AssetName: string;
  LevelRequire: number;
}

/**
 * Payload expected when creating a new asset (AssetId is auto-generated).
 */
export interface CreateAssetDto {
  AssetName: string;
  LevelRequire: number;
}
