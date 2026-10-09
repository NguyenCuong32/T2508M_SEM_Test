import { useCallback, useEffect, useMemo, useState } from 'react'

const emptyPlayer = { playerName: '', fullName: '', age: '', currentLevel: 1 }
const emptyAsset = { name: '', type: 'Hero', description: '' }

async function api(path, options) {
  const response = await fetch(path, {
    ...options,
    headers: { 'Content-Type': 'application/json', ...options?.headers },
  })
  const body = response.status === 204 ? null : await response.json().catch(() => null)
  if (!response.ok) {
    const detail = body?.message || body?.title || Object.values(body?.errors || {}).flat().join(' ')
    throw new Error(detail || `Yêu cầu thất bại (${response.status}).`)
  }
  return body
}

function App() {
  const [players, setPlayers] = useState([])
  const [assets, setAssets] = useState([])
  const [rows, setRows] = useState([])
  const [filter, setFilter] = useState('all')
  const [playerForm, setPlayerForm] = useState(emptyPlayer)
  const [assetForm, setAssetForm] = useState(emptyAsset)
  const [assignment, setAssignment] = useState({ playerId: '', assetId: '' })
  const [notice, setNotice] = useState(null)
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)

  const refresh = useCallback(async () => {
    setLoading(true)
    try {
      const [playerData, assetData, reportData] = await Promise.all([
        api('/api/players'), api('/api/assets'), api('/api/getassetsbyplayer'),
      ])
      setPlayers(playerData)
      setAssets(assetData)
      setRows(reportData)
      setAssignment((current) => ({
        playerId: current.playerId || String(playerData[0]?.id || ''),
        assetId: current.assetId || String(assetData[0]?.id || ''),
      }))
    } catch (error) {
      setNotice({ kind: 'error', text: `${error.message} Hãy kiểm tra API và SQL Server.` })
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { refresh() }, [refresh])

  const visibleRows = useMemo(() => filter === 'all'
    ? rows
    : rows.filter((row) => String(row.playerId) === filter), [filter, rows])

  async function submit(action, successText) {
    setBusy(true)
    setNotice(null)
    try {
      await action()
      setNotice({ kind: 'success', text: successText })
      await refresh()
    } catch (error) {
      setNotice({ kind: 'error', text: error.message })
    } finally {
      setBusy(false)
    }
  }

  function registerPlayer(event) {
    event.preventDefault()
    return submit(async () => {
      await api('/api/registerplayer', { method: 'POST', body: JSON.stringify({
        ...playerForm, age: Number(playerForm.age), currentLevel: Number(playerForm.currentLevel),
      }) })
      setPlayerForm(emptyPlayer)
    }, 'Đã đăng ký người chơi.')
  }

  function createAsset(event) {
    event.preventDefault()
    return submit(async () => {
      await api('/api/createasset', { method: 'POST', body: JSON.stringify(assetForm) })
      setAssetForm(emptyAsset)
    }, 'Đã tạo asset.')
  }

  function assignAsset(event) {
    event.preventDefault()
    return submit(async () => {
      await api('/api/assignasset', { method: 'POST', body: JSON.stringify({
        playerId: Number(assignment.playerId), assetId: Number(assignment.assetId),
      }) })
    }, 'Đã gán asset cho người chơi.')
  }

  return (
    <main className="shell">
      <header className="topbar">
        <a className="brand" href="#top" aria-label="BattleGame trang chủ">
          <span className="brand-mark">B</span><span>BATTLEGAME<span className="brand-dot">.</span></span>
        </a>
        <div className="topbar-right"><span className="live-dot" /> HỆ THỐNG HOẠT ĐỘNG <span className="topbar-divider" /> ADMIN CONSOLE</div>
      </header>

      <section className="hero" id="top">
        <div className="hero-copy">
          <div className="eyebrow"><span /> PLAYER MANAGEMENT</div>
          <h1>Tài sản<br /><em>người chơi.</em></h1>
          <p>Theo dõi cấp độ, người chơi và bộ sưu tập vật phẩm trong một nơi.</p>
        </div>
        <div className="hero-stats">
          <div className="stat"><span>NGƯỜI CHƠI</span><strong>{players.length.toString().padStart(2, '0')}</strong></div>
          <div className="stat"><span>ASSET</span><strong>{assets.length.toString().padStart(2, '0')}</strong></div>
          <div className="stat"><span>ĐANG SỞ HỮU</span><strong>{rows.length.toString().padStart(2, '0')}</strong></div>
        </div>
      </section>

      {notice && <div className={`notice ${notice.kind}`} role="status"><span>{notice.kind === 'error' ? '!' : '✓'}</span>{notice.text}<button onClick={() => setNotice(null)} aria-label="Đóng">×</button></div>}

      <section className="report-section">
        <div className="section-heading">
          <div><div className="eyebrow muted"><span /> LIVE DATABASE</div><h2>Báo cáo tài sản</h2><p>Danh sách asset theo từng người chơi</p></div>
          <div className="report-actions">
            <label className="select-wrap"><span className="sr-only">Lọc người chơi</span>
              <select value={filter} onChange={(event) => setFilter(event.target.value)}>
                <option value="all">Tất cả người chơi</option>
                {players.map((player) => <option value={player.id} key={player.id}>{player.playerName}</option>)}
              </select>
            </label>
            <button className="refresh-button" onClick={refresh} disabled={loading} aria-label="Làm mới">↻</button>
          </div>
        </div>
        <div className="table-frame">
          <table>
            <thead><tr><th>NO.</th><th>PLAYER NAME</th><th>LEVEL</th><th>AGE</th><th>ASSET NAME</th><th>TYPE</th></tr></thead>
            <tbody>
              {loading ? <tr><td colSpan="6" className="empty">Đang tải dữ liệu…</td></tr>
                : visibleRows.length === 0 ? <tr><td colSpan="6" className="empty"><span className="empty-icon">◇</span><strong>Chưa có dữ liệu tài sản</strong><span>Đăng ký người chơi, tạo asset và gán vật phẩm để bắt đầu.</span></td></tr>
                  : visibleRows.map((row, index) => <tr key={`${row.playerId}-${row.assetId}`}>
                    <td className="row-number">{String(index + 1).padStart(2, '0')}</td>
                    <td><span className="player-avatar">{row.playerName.slice(0, 1).toUpperCase()}</span><strong>{row.playerName}</strong></td>
                    <td><span className="level">LVL {row.level}</span></td><td>{row.age}</td>
                    <td className="asset-name">{row.assetName}</td><td><span className="type-tag">{row.assetType}</span></td>
                  </tr>)}
            </tbody>
          </table>
          <div className="table-footer"><span>HIỂN THỊ <b>{visibleRows.length}</b> BẢN GHI</span><span>DATABASE <b>BATTLEGAME</b></span></div>
        </div>
      </section>

      <section className="admin-section">
        <div className="section-heading admin-heading"><div><div className="eyebrow muted"><span /> QUICK ACTIONS</div><h2>Quản lý game</h2><p>Tạo dữ liệu và cập nhật bộ sưu tập</p></div></div>
        <div className="forms-grid">
          <form className="form-card" onSubmit={registerPlayer}>
            <div className="form-title"><span className="form-icon">♙</span><div><h3>Người chơi mới</h3><p>Đăng ký tài khoản game</p></div></div>
            <label>PLAYER NAME<input required maxLength="50" placeholder="vd. Player 1" value={playerForm.playerName} onChange={(e) => setPlayerForm({ ...playerForm, playerName: e.target.value })} /></label>
            <label>HỌ VÀ TÊN<input required maxLength="120" placeholder="Nguyễn Văn An" value={playerForm.fullName} onChange={(e) => setPlayerForm({ ...playerForm, fullName: e.target.value })} /></label>
            <div className="field-row"><label>TUỔI<input required type="number" min="1" max="120" value={playerForm.age} onChange={(e) => setPlayerForm({ ...playerForm, age: e.target.value })} /></label><label>LEVEL<input required type="number" min="1" value={playerForm.currentLevel} onChange={(e) => setPlayerForm({ ...playerForm, currentLevel: e.target.value })} /></label></div>
            <button className="primary-button" disabled={busy}>ĐĂNG KÝ NGƯỜI CHƠI <span>↗</span></button>
          </form>

          <form className="form-card" onSubmit={createAsset}>
            <div className="form-title"><span className="form-icon asset-icon">✧</span><div><h3>Tạo asset</h3><p>Thêm hero hoặc vật phẩm</p></div></div>
            <label>TÊN ASSET<input required maxLength="100" placeholder="vd. Hero 1" value={assetForm.name} onChange={(e) => setAssetForm({ ...assetForm, name: e.target.value })} /></label>
            <label>LOẠI ASSET<select value={assetForm.type} onChange={(e) => setAssetForm({ ...assetForm, type: e.target.value })}><option>Hero</option><option>Equipment</option><option>Skin</option><option>Other</option></select></label>
            <label>MÔ TẢ <span className="optional">(TÙY CHỌN)</span><textarea rows="2" maxLength="500" placeholder="Thông tin ngắn về asset" value={assetForm.description} onChange={(e) => setAssetForm({ ...assetForm, description: e.target.value })} /></label>
            <button className="primary-button" disabled={busy}>TẠO ASSET <span>↗</span></button>
          </form>

          <form className="form-card assign-card" onSubmit={assignAsset}>
            <div className="form-title"><span className="form-icon assign-icon">↗</span><div><h3>Gán asset</h3><p>Thêm vật phẩm vào hồ sơ player</p></div></div>
            <label>NGƯỜI CHƠI<select required value={assignment.playerId} onChange={(e) => setAssignment({ ...assignment, playerId: e.target.value })}><option value="">Chọn người chơi</option>{players.map((player) => <option key={player.id} value={player.id}>{player.playerName} · LVL {player.currentLevel}</option>)}</select></label>
            <label>ASSET<select required value={assignment.assetId} onChange={(e) => setAssignment({ ...assignment, assetId: e.target.value })}><option value="">Chọn asset</option>{assets.map((asset) => <option key={asset.id} value={asset.id}>{asset.name} · {asset.type}</option>)}</select></label>
            <div className="assign-hint">Chọn một player và asset đã tạo ở trên.</div>
            <button className="primary-button" disabled={busy || !players.length || !assets.length}>GÁN VÀO PLAYER <span>↗</span></button>
          </form>
        </div>
      </section>

      <footer><span>© 2026 BATTLEGAME</span><span>PLAYER ASSET MANAGEMENT <i>●</i></span></footer>
    </main>
  )
}

export default App
