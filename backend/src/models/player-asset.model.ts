/**
 * Represents a row in the [PlayerAsset] junction table.
 */
export interface PlayerAsset {
  PlayerId: string;
  AssetId: string;
}

/**
 * Flattened report row returned by the getassetsbyplayer report query.
 */
export interface PlayerAssetReport {
  No: number;
  PlayerName: string;
  Level: number;
  Age: string;
  AssetName: string;
}
