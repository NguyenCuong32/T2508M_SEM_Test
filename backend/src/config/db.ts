import sql, { ConnectionPool } from "mssql";
import { dbConfig } from "./database.config";

/** Singleton connection pool shared across all function invocations. */
let pool: ConnectionPool | null = null;

export async function getPool(): Promise<ConnectionPool> {
  if (pool && pool.connected) {
    return pool;
  }
  pool = await sql.connect(dbConfig);
  return pool;
}
