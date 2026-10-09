import sql from "mssql";
import { v4 as uuidv4 } from "uuid";
import { getPool } from "../config/db";
import { Player, RegisterPlayerDto } from "../models/player.model";

/**
 * Data-access layer for the [Player] table.
 */
export class PlayerRepository {
  /**
   * Inserts a new player and returns the created record.
   */
  async create(dto: RegisterPlayerDto): Promise<Player> {
    const pool = await getPool();
    const newId = uuidv4();
    const level = dto.Level ?? 1;

    await pool
      .request()
      .input("PlayerId", sql.UniqueIdentifier, newId)
      .input("PlayerName", sql.NVarChar(64), dto.PlayerName)
      .input("FullName", sql.NVarChar(128), dto.FullName)
      .input("Age", sql.NVarChar(10), dto.Age)
      .input("Level", sql.Int, level)
      .input("Email", sql.NVarChar(64), dto.Email)
      .query(
        `INSERT INTO Player (PlayerId, PlayerName, FullName, Age, [Level], Email)
         VALUES (@PlayerId, @PlayerName, @FullName, @Age, @Level, @Email)`
      );

    return {
      PlayerId: newId,
      PlayerName: dto.PlayerName,
      FullName: dto.FullName,
      Age: dto.Age,
      Level: level,
      Email: dto.Email,
    };
  }

  /**
   * Returns all players from the database.
   */
  async findAll(): Promise<Player[]> {
    const pool = await getPool();
    const result = await pool.request().query<Player>("SELECT * FROM Player");
    return result.recordset;
  }

  /**
   * Returns a single player by GUID, or null if not found.
   */
  async findById(playerId: string): Promise<Player | null> {
    const pool = await getPool();
    const result = await pool
      .request()
      .input("PlayerId", sql.UniqueIdentifier, playerId)
      .query<Player>("SELECT * FROM Player WHERE PlayerId = @PlayerId");

    return result.recordset[0] ?? null;
  }
}
