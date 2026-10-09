function ShieldIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <path d="M12 2.75 20 6v5.5c0 4.82-3.08 8.78-8 10.5-4.92-1.72-8-5.68-8-10.5V6l8-3.25Z" />
      <path d="m8.5 12 2.25 2.25L15.75 9" />
    </svg>
  )
}

function LoadingRows() {
  return Array.from({ length: 4 }, (_, index) => (
    <tr className="skeleton-row" key={index} aria-hidden="true">
      <td><span /></td>
      <td><span /></td>
      <td><span /></td>
      <td><span /></td>
      <td><span /></td>
    </tr>
  ))
}

export default function ReportTable({ rows, loading }) {
  return (
    <div className="table-shell">
      <table>
        <thead>
          <tr>
            <th scope="col">No</th>
            <th scope="col">Player name</th>
            <th scope="col">Level</th>
            <th scope="col">Age</th>
            <th scope="col">Asset name</th>
          </tr>
        </thead>
        <tbody>
          {loading ? <LoadingRows /> : rows.map((row) => (
            <tr key={`${row.playerId}-${row.assetId}`}>
              <td data-label="No" className="row-number">{row.no}</td>
              <td data-label="Player name">
                <div className="player-cell">
                  <span className="avatar">{row.playerName.slice(0, 1).toUpperCase()}</span>
                  <strong>{row.playerName}</strong>
                </div>
              </td>
              <td data-label="Level"><span className="level-badge">Lv. {row.level}</span></td>
              <td data-label="Age">{row.age}</td>
              <td data-label="Asset name">
                <div className="asset-cell"><ShieldIcon /><span>{row.assetName}</span></div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {!loading && rows.length === 0 && (
        <div className="empty-state">
          <div className="empty-icon"><ShieldIcon /></div>
          <h2>No assets found</h2>
          <p>The report does not contain any player assets yet.</p>
        </div>
      )}
    </div>
  )
}
