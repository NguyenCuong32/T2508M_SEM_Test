import { useCallback, useEffect, useMemo, useState } from 'react'
import ReportTable from './components/ReportTable.jsx'
import { getPlayerAssets } from './services/api.js'

function RefreshIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <path d="M20 7v5h-5" />
      <path d="M18.5 9.5A7 7 0 1 0 19 14" />
    </svg>
  )
}

function SearchIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <circle cx="11" cy="11" r="6.5" />
      <path d="m16 16 4 4" />
    </svg>
  )
}

export default function App() {
  const [rows, setRows] = useState([])
  const [query, setQuery] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [lastUpdated, setLastUpdated] = useState(null)

  const loadReport = useCallback(async (signal) => {
    setLoading(true)
    setError('')

    try {
      const data = await getPlayerAssets({ signal })
      setRows(Array.isArray(data) ? data : [])
      setLastUpdated(new Date())
    } catch (requestError) {
      if (requestError.name !== 'AbortError') {
        setError(requestError.message || 'Unable to load the report.')
      }
    } finally {
      if (!signal?.aborted) setLoading(false)
    }
  }, [])

  useEffect(() => {
    const controller = new AbortController()
    loadReport(controller.signal)
    return () => controller.abort()
  }, [loadReport])

  const filteredRows = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase()
    if (!normalizedQuery) return rows

    return rows.filter((row) =>
      row.playerName.toLowerCase().includes(normalizedQuery)
      || row.assetName.toLowerCase().includes(normalizedQuery),
    )
  }, [query, rows])

  const uniquePlayers = new Set(rows.map((row) => row.playerId)).size

  return (
    <div className="app-shell">
      <header className="topbar">
        <a className="brand" href="/" aria-label="BattleGame home">
          <span className="brand-mark">B</span>
          <span>BATTLE<span>GAME</span></span>
        </a>
        <span className="environment"><i /> LIVE REPORT</span>
      </header>

      <main>
        <section className="hero" aria-labelledby="page-title">
          <div>
            <p className="eyebrow">Command center / Assets</p>
            <h1 id="page-title">Player asset report</h1>
            <p className="hero-copy">Track every player and the assets assigned to their battle inventory.</p>
          </div>
          <div className="hero-metric" aria-label={`${rows.length} asset assignments`}>
            <span>Assignments</span>
            <strong>{loading ? '—' : rows.length.toString().padStart(2, '0')}</strong>
          </div>
        </section>

        <section className="report-card" aria-label="Player asset report">
          <div className="report-toolbar">
            <div className="summary">
              <span className="summary-value">{loading ? '—' : uniquePlayers}</span>
              <span className="summary-label">Active players</span>
            </div>

            <div className="toolbar-actions">
              <label className="search-field">
                <SearchIcon />
                <span className="sr-only">Search report</span>
                <input
                  type="search"
                  placeholder="Search player or asset"
                  value={query}
                  onChange={(event) => setQuery(event.target.value)}
                />
              </label>
              <button className="refresh-button" type="button" onClick={() => loadReport()} disabled={loading}>
                <RefreshIcon />
                <span>{loading ? 'Loading' : 'Refresh'}</span>
              </button>
            </div>
          </div>

          {error && (
            <div className="error-banner" role="alert">
              <strong>Report unavailable.</strong> {error}
            </div>
          )}

          <ReportTable rows={filteredRows} loading={loading} />

          <footer className="report-footer">
            <span>Showing {filteredRows.length} of {rows.length} assignments</span>
            <span>{lastUpdated ? `Updated ${lastUpdated.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}` : 'Waiting for data'}</span>
          </footer>
        </section>
      </main>
    </div>
  )
}
