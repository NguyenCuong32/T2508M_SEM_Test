import { getPool } from "../config/db";
import { PlayerAssetReport } from "../models/player-asset.model";

/**
 * Data-access layer for the [PlayerAsset] junction table.
 * Also hosts the joined report query used by getassetsbyplayer.
 */
export class PlayerAssetRepository {
  /**
   * Returns a joined report of all player-asset relationships.
   * Columns match the report table in the exam specification:
   *   No | PlayerName | Level | Age | AssetName
   */
  async getAssetsReport(): Promise<PlayerAssetReport[]> {
    const pool = await getPool();

    const result = await pool.request().query<PlayerAssetReport>(
      `SELECT
         ROW_NUMBER() OVER (ORDER BY p.PlayerName)  AS [No],
         p.PlayerName,
         p.[Level],
         p.Age,
         a.AssetName
       FROM PlayerAsset pa
       INNER JOIN Player p ON pa.PlayerId = p.PlayerId
       INNER JOIN Asset  a ON pa.AssetId  = a.AssetId
       ORDER BY p.PlayerName`
    );

    return result.recordset;
  }
}
