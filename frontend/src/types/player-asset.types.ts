/**
 * Matches the shape returned by GET /api/getassetsbyplayer
 */
export interface PlayerAssetReport {
  No: number;
  PlayerName: string;
  Level: number;
  Age: string;
  AssetName: string;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
}
