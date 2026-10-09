import { useEffect, useState } from 'react'
import { getAssetsByPlayer, type PlayerAssetRow } from './api'

export default function App() {
  const [rows, setRows] = useState<PlayerAssetRow[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const controller = new AbortController()
    getAssetsByPlayer(controller.signal)
      .then(setRows)
      .catch((e: Error) => {
        if (e.name !== 'AbortError') setError(e.message)
      })
      .finally(() => setLoading(false))
    return () => controller.abort()
  }, [])

  return (
    <main className="container">
      <h1>Assets by Player</h1>
      {loading && <p>Loading...</p>}
      {error && <p className="error">Could not load data: {error}</p>}
      {!loading && !error && rows.length === 0 && <p>No data.</p>}
      {rows.length > 0 && (
        <table>
          <thead>
            <tr>
              <th>No</th>
              <th>Player name</th>
              <th>Level</th>
              <th>Age</th>
              <th>Asset name</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((r) => (
              <tr key={r.no}>
                <td>{r.no}</td>
                <td>{r.playerName}</td>
                <td>{r.level}</td>
                <td>{r.age}</td>
                <td>{r.assetName}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </main>
  )
}
