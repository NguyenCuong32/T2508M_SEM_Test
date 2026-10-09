import React, { useEffect, useState } from "react";
import { PlayerAssetReport } from "../types/player-asset.types";
import { fetchPlayerAssetsReport } from "../services/api.service";
import PlayerAssetsTable from "../components/PlayerAssetsTable";

/**
 * Main page that loads and displays the player-assets report.
 */
const ReportPage: React.FC = () => {
  const [report, setReport] = useState<PlayerAssetReport[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadReport = async () => {
      try {
        const data = await fetchPlayerAssetsReport();
        setReport(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Failed to load data.");
      } finally {
        setLoading(false);
      }
    };

    loadReport();
  }, []);

  return (
    <main style={styles.page}>
      <header style={styles.header}>
        <h1 style={styles.title}>⚔️ BattleGame</h1>
        <p style={styles.subtitle}>Player Assets Report</p>
      </header>

      <section style={styles.content}>
        {loading && <p style={styles.status}>Loading report…</p>}

        {error && (
          <p style={styles.error} role="alert">
            ⚠️ {error}
          </p>
        )}

        {!loading && !error && <PlayerAssetsTable data={report} />}
      </section>
    </main>
  );
};

const styles: Record<string, React.CSSProperties> = {
  page: {
    minHeight: "100vh",
    background: "#f5f7fa",
    padding: "40px 20px",
    boxSizing: "border-box",
  },
  header: {
    textAlign: "center",
    marginBottom: "32px",
  },
  title: {
    fontSize: "32px",
    fontWeight: 700,
    color: "#1a1a2e",
    margin: 0,
  },
  subtitle: {
    fontSize: "16px",
    color: "#555",
    marginTop: "8px",
  },
  content: {
    maxWidth: "860px",
    margin: "0 auto",
    background: "#fff",
    padding: "24px",
    borderRadius: "12px",
    boxShadow: "0 4px 16px rgba(0,0,0,0.08)",
  },
  status: {
    color: "#555",
    textAlign: "center",
  },
  error: {
    color: "#c0392b",
    background: "#fdecea",
    padding: "12px 16px",
    borderRadius: "6px",
  },
};

export default ReportPage;
