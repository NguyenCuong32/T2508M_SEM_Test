import sql from "mssql";
import { v4 as uuidv4 } from "uuid";
import { getPool } from "../config/db";
import { Asset, CreateAssetDto } from "../models/asset.model";

/**
 * Data-access layer for the [Asset] table.
 * All SQL interactions are encapsulated here; no SQL leaks into services.
 */
export class AssetRepository {
  /**
   * Inserts a new asset and returns the created record.
   */
  async create(dto: CreateAssetDto): Promise<Asset> {
    const pool = await getPool();
    const newId = uuidv4();

    await pool
      .request()
      .input("AssetId", sql.UniqueIdentifier, newId)
      .input("AssetName", sql.NVarChar(64), dto.AssetName)
      .input("LevelRequire", sql.Int, dto.LevelRequire)
      .query(
        `INSERT INTO Asset (AssetId, AssetName, LevelRequire)
         VALUES (@AssetId, @AssetName, @LevelRequire)`
      );

    return { AssetId: newId, AssetName: dto.AssetName, LevelRequire: dto.LevelRequire };
  }

  /**
   * Returns all assets from the database.
   */
  async findAll(): Promise<Asset[]> {
    const pool = await getPool();
    const result = await pool.request().query<Asset>("SELECT * FROM Asset");
    return result.recordset;
  }

  /**
   * Returns a single asset by its GUID, or null if not found.
   */
  async findById(assetId: string): Promise<Asset | null> {
    const pool = await getPool();
    const result = await pool
      .request()
      .input("AssetId", sql.UniqueIdentifier, assetId)
      .query<Asset>("SELECT * FROM Asset WHERE AssetId = @AssetId");

    return result.recordset[0] ?? null;
  }
}
