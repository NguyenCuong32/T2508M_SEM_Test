import { config as SqlConfig } from "mssql";

/**
 * Builds the mssql connection config from environment variables.
 * All values must be set in local.settings.json (local) or
 * Application Settings (Azure).
 */
export const dbConfig: SqlConfig = {
  server: process.env.SQL_SERVER ?? "localhost",
  database: process.env.SQL_DATABASE ?? "BATTLEGAME",
  user: process.env.SQL_USER ?? "sa",
  password: process.env.SQL_PASSWORD ?? "",
  port: parseInt(process.env.SQL_PORT ?? "1433", 10),
  options: {
    encrypt: true,          // required for Azure SQL
    trustServerCertificate: true, // set false in production with valid cert
  },
  pool: {
    max: 10,
    min: 0,
    idleTimeoutMillis: 30000,
  },
};
