import React from "react";
import { PlayerAssetReport } from "../types/player-asset.types";

interface Props {
  data: PlayerAssetReport[];
}

/**
 * Renders the player-assets report as an HTML table.
 * Columns: No | Player name | Level | Age | Asset name
 */
const PlayerAssetsTable: React.FC<Props> = ({ data }) => {
  if (data.length === 0) {
    return <p style={styles.empty}>No data available.</p>;
  }

  return (
    <div style={styles.wrapper}>
      <table style={styles.table} aria-label="Player Assets Report">
        <thead>
          <tr>
            <th style={styles.th}>No</th>
            <th style={styles.th}>Player name</th>
            <th style={styles.th}>Level</th>
            <th style={styles.th}>Age</th>
            <th style={styles.th}>Asset name</th>
          </tr>
        </thead>
        <tbody>
          {data.map((row) => (
            <tr key={row.No} style={styles.tr}>
              <td style={styles.td}>{row.No}</td>
              <td style={styles.td}>{row.PlayerName}</td>
              <td style={styles.td}>{row.Level}</td>
              <td style={styles.td}>{row.Age}</td>
              <td style={styles.td}>{row.AssetName}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

const styles: Record<string, React.CSSProperties> = {
  wrapper: {
    overflowX: "auto",
    borderRadius: "8px",
    boxShadow: "0 2px 8px rgba(0,0,0,0.15)",
  },
  table: {
    width: "100%",
    borderCollapse: "collapse",
    fontFamily: "'Segoe UI', sans-serif",
    fontSize: "14px",
  },
  th: {
    background: "#1a73e8",
    color: "#fff",
    padding: "12px 16px",
    textAlign: "left",
    fontWeight: 600,
    letterSpacing: "0.5px",
  },
  tr: {
    borderBottom: "1px solid #e0e0e0",
  },
  td: {
    padding: "10px 16px",
    color: "#333",
  },
  empty: {
    color: "#888",
    fontStyle: "italic",
    padding: "16px 0",
  },
};

export default PlayerAssetsTable;
