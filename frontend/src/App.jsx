import React, { useState, useEffect } from 'react';
import { 
  Gamepad2, 
  Users, 
  ShieldAlert, 
  Sword, 
  PlusCircle, 
  RefreshCw, 
  Search, 
  UserPlus, 
  Sparkles, 
  CheckCircle2, 
  AlertCircle,
  X,
  Link as LinkIcon,
  Layers
} from 'lucide-react';
import './App.css';

const API_BASE_URL = 'http://localhost:7071/api';

export default function App() {
  const [reports, setReports] = useState([]);
  const [players, setPlayers] = useState([]);
  const [assets, setAssets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [isApiOnline, setIsApiOnline] = useState(false);
  const [toast, setToast] = useState(null);

  // Modal visibility
  const [showRegisterModal, setShowRegisterModal] = useState(false);
  const [showAssetModal, setShowAssetModal] = useState(false);
  const [showAssignModal, setShowAssignModal] = useState(false);

  // Form states
  const [newPlayer, setNewPlayer] = useState({
    playerName: '',
    fullName: '',
    age: '',
    level: 1,
    email: ''
  });

  const [newAsset, setNewAsset] = useState({
    assetName: '',
    levelRequire: 1
  });

  const [assignment, setAssignment] = useState({
    playerId: '',
    assetId: ''
  });

  // Show Toast
  const notify = (message, type = 'success') => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 4000);
  };

  // Fetch Requirement 3 Report Table Data
  const fetchReports = async () => {
    setLoading(true);
    try {
      const res = await fetch(`${API_BASE_URL}/getassetsbyplayer`);
      if (res.ok) {
        const data = await res.json();
        setReports(Array.isArray(data) ? data : []);
        setIsApiOnline(true);
      } else {
        throw new Error('Failed to fetch data');
      }
    } catch (err) {
      console.error('Error fetching reports:', err);
      setIsApiOnline(false);
    } finally {
      setLoading(false);
    }
  };

  // Fetch dropdown options for players & assets
  const fetchDropdowns = async () => {
    try {
      const [resPlayers, resAssets] = await Promise.all([
        fetch(`${API_BASE_URL}/players`),
        fetch(`${API_BASE_URL}/assets`)
      ]);
      if (resPlayers.ok) {
        const pData = await resPlayers.json();
        setPlayers(pData);
      }
      if (resAssets.ok) {
        const aData = await resAssets.json();
        setAssets(aData);
      }
    } catch (err) {
      console.error('Error fetching dropdowns:', err);
    }
  };

  useEffect(() => {
    fetchReports();
    fetchDropdowns();
  }, []);

  // Handle Requirement 1: Register Player
  const handleRegisterPlayer = async (e) => {
    e.preventDefault();
    if (!newPlayer.playerName || !newPlayer.fullName || !newPlayer.email) {
      notify('Please fill in all required fields.', 'error');
      return;
    }

    try {
      const res = await fetch(`${API_BASE_URL}/registerplayer`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          ...newPlayer,
          level: parseInt(newPlayer.level, 10) || 1
        })
      });

      const result = await res.json();
      if (res.ok && (result.success || result.data || result.playerId)) {
        notify(`Player '${newPlayer.playerName}' registered successfully!`);
        setShowRegisterModal(false);
        setNewPlayer({ playerName: '', fullName: '', age: '', level: 1, email: '' });
        fetchReports();
        fetchDropdowns();
      } else {
        notify(result.message || 'Failed to register player', 'error');
      }
    } catch (err) {
      notify(`Connection error: ${err.message}`, 'error');
    }
  };

  // Handle Requirement 2: Create Asset
  const handleCreateAsset = async (e) => {
    e.preventDefault();
    if (!newAsset.assetName) {
      notify('Asset name is required.', 'error');
      return;
    }

    try {
      const res = await fetch(`${API_BASE_URL}/createasset`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          ...newAsset,
          levelRequire: parseInt(newAsset.levelRequire, 10) || 1
        })
      });

      const result = await res.json();
      if (res.ok && (result.success || result.data || result.assetId)) {
        notify(`Asset '${newAsset.assetName}' created successfully!`);
        setShowAssetModal(false);
        setNewAsset({ assetName: '', levelRequire: 1 });
        fetchReports();
        fetchDropdowns();
      } else {
        notify(result.message || 'Failed to create asset', 'error');
      }
    } catch (err) {
      notify(`Connection error: ${err.message}`, 'error');
    }
  };

  // Handle Assign Asset
  const handleAssignAsset = async (e) => {
    e.preventDefault();
    if (!assignment.playerId || !assignment.assetId) {
      notify('Please select both a Player and an Asset.', 'error');
      return;
    }

    try {
      const res = await fetch(`${API_BASE_URL}/assignasset`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(assignment)
      });

      const result = await res.json();
      if (res.ok && result.success) {
        notify('Asset assigned to player successfully!');
        setShowAssignModal(false);
        setAssignment({ playerId: '', assetId: '' });
        fetchReports();
      } else {
        notify(result.message || 'Failed to assign asset', 'error');
      }
    } catch (err) {
      notify(`Connection error: ${err.message}`, 'error');
    }
  };

  // Filtered reports
  const filteredReports = reports.filter((item) => {
    const q = searchTerm.toLowerCase();
    return (
      (item.playerName && item.playerName.toLowerCase().includes(q)) ||
      (item.assetName && item.assetName.toLowerCase().includes(q)) ||
      (item.level && item.level.toString().includes(q))
    );
  });

  return (
    <div className="app-container">
      {/* Toast Notification */}
      {toast && (
        <div className="toast-container">
          <div className={`toast ${toast.type}`}>
            {toast.type === 'success' ? (
              <CheckCircle2 size={18} />
            ) : (
              <AlertCircle size={18} />
            )}
            <span>{toast.message}</span>
          </div>
        </div>
      )}

      {/* Header */}
      <header className="app-header">
        <div className="brand-wrapper">
          <div className="logo-badge">
            <Gamepad2 size={28} />
          </div>
          <div>
            <h1 className="brand-title">BATTLEGAME</h1>
            <p className="brand-subtitle">Azure Functions Cloud Solutions &bull; SET01</p>
          </div>
        </div>

        <div className="header-status-badge">
          <span className={`status-dot ${isApiOnline ? 'online' : 'offline'}`}></span>
          <span>Azure Function API: {isApiOnline ? 'Online (Port 7071)' : 'Offline / Checking...'}</span>
        </div>
      </header>

      {/* Metric Cards */}
      <section className="stats-grid">
        <div className="stat-card">
          <div className="stat-icon cyan">
            <Layers size={24} />
          </div>
          <div className="stat-info">
            <span className="stat-value">{reports.length}</span>
            <span className="stat-label">Report Records</span>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon purple">
            <Users size={24} />
          </div>
          <div className="stat-info">
            <span className="stat-value">{players.length || reports.length}</span>
            <span className="stat-label">Registered Players</span>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon blue">
            <Sword size={24} />
          </div>
          <div className="stat-info">
            <span className="stat-value">{assets.length || 4}</span>
            <span className="stat-label">Game Assets</span>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon amber">
            <Sparkles size={24} />
          </div>
          <div className="stat-info">
            <span className="stat-value">
              {reports.length > 0
                ? Math.round(reports.reduce((acc, cur) => acc + (cur.level || 0), 0) / reports.length)
                : 0}
            </span>
            <span className="stat-label">Average Player Level</span>
          </div>
        </div>
      </section>

      {/* Action / Control Bar */}
      <section className="control-bar">
        <div className="search-box">
          <Search size={18} color="var(--text-dim)" />
          <input
            type="text"
            id="searchInput"
            placeholder="Search player, level or asset..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>

        <div className="action-buttons">
          <button 
            id="btnRegisterPlayer" 
            className="btn btn-primary" 
            onClick={() => setShowRegisterModal(true)}
          >
            <UserPlus size={16} />
            Register Player
          </button>

          <button 
            id="btnCreateAsset" 
            className="btn btn-purple" 
            onClick={() => setShowAssetModal(true)}
          >
            <Sword size={16} />
            Create Asset
          </button>

          <button 
            id="btnAssignAsset" 
            className="btn btn-secondary" 
            onClick={() => {
              fetchDropdowns();
              setShowAssignModal(true);
            }}
          >
            <LinkIcon size={16} />
            Assign Asset
          </button>

          <button 
            id="btnRefresh" 
            className="btn btn-refresh" 
            onClick={() => {
              fetchReports();
              fetchDropdowns();
            }}
            title="Refresh table"
          >
            <RefreshCw size={16} className={loading ? 'animate-spin' : ''} />
          </button>
        </div>
      </section>

      {/* Requirement 3 Table */}
      <main className="table-card">
        <div className="table-header-bar">
          <h2 className="table-title">
            <ShieldAlert size={22} color="var(--accent-cyan)" />
            Requirement 3: Player Assets Report
          </h2>
          <span className="table-badge">API: getassetsbyplayer</span>
        </div>

        <div className="table-responsive">
          {loading ? (
            <div className="state-box">
              <RefreshCw size={36} className="animate-spin" color="var(--accent-cyan)" />
              <p>Fetching report data from Azure Function API...</p>
            </div>
          ) : filteredReports.length === 0 ? (
            <div className="state-box">
              <AlertCircle size={40} color="var(--text-dim)" />
              <p>No records found. Try registering players or assigning assets.</p>
            </div>
          ) : (
            <table className="battle-table" id="reportTable">
              <thead>
                <tr>
                  <th className="col-no">No</th>
                  <th>Player name</th>
                  <th>Level</th>
                  <th>Age</th>
                  <th>Asset name</th>
                </tr>
              </thead>
              <tbody>
                {filteredReports.map((row) => (
                  <tr key={`${row.no}-${row.playerName}-${row.assetName}`}>
                    <td className="col-no">{row.no}</td>
                    <td>
                      <div className="player-cell">
                        <div className="player-avatar">
                          {row.playerName ? row.playerName.charAt(0).toUpperCase() : 'P'}
                        </div>
                        <span>{row.playerName}</span>
                      </div>
                    </td>
                    <td>
                      <span className="level-badge">
                        Lv. {row.level}
                      </span>
                    </td>
                    <td>
                      <span className="age-badge">{row.age} yrs</span>
                    </td>
                    <td>
                      <span className="asset-badge">
                        <Sword size={14} />
                        {row.assetName}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </main>

      {/* Modal 1: Register Player (Requirement 1) */}
      {showRegisterModal && (
        <div className="modal-overlay" onClick={() => setShowRegisterModal(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3 className="modal-title">
                <UserPlus size={20} color="var(--accent-cyan)" />
                Requirement 1: Register Player
              </h3>
              <button className="modal-close" onClick={() => setShowRegisterModal(false)}>
                <X size={20} />
              </button>
            </div>
            <form onSubmit={handleRegisterPlayer}>
              <div className="modal-body">
                <div className="form-group">
                  <label className="form-label">Player Name *</label>
                  <input
                    type="text"
                    required
                    className="form-input"
                    placeholder="e.g. Player 4"
                    value={newPlayer.playerName}
                    onChange={(e) => setNewPlayer({ ...newPlayer, playerName: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Full Name *</label>
                  <input
                    type="text"
                    required
                    className="form-input"
                    placeholder="e.g. Nguyen Van D"
                    value={newPlayer.fullName}
                    onChange={(e) => setNewPlayer({ ...newPlayer, fullName: e.target.value })}
                  />
                </div>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                  <div className="form-group">
                    <label className="form-label">Age *</label>
                    <input
                      type="text"
                      required
                      className="form-input"
                      placeholder="e.g. 25"
                      value={newPlayer.age}
                      onChange={(e) => setNewPlayer({ ...newPlayer, age: e.target.value })}
                    />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Level *</label>
                    <input
                      type="number"
                      required
                      min="1"
                      className="form-input"
                      placeholder="e.g. 15"
                      value={newPlayer.level}
                      onChange={(e) => setNewPlayer({ ...newPlayer, level: e.target.value })}
                    />
                  </div>
                </div>
                <div className="form-group">
                  <label className="form-label">Email *</label>
                  <input
                    type="email"
                    required
                    className="form-input"
                    placeholder="e.g. player@example.com"
                    value={newPlayer.email}
                    onChange={(e) => setNewPlayer({ ...newPlayer, email: e.target.value })}
                  />
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={() => setShowRegisterModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary">
                  Submit (registerplayer)
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal 2: Create Asset (Requirement 2) */}
      {showAssetModal && (
        <div className="modal-overlay" onClick={() => setShowAssetModal(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3 className="modal-title">
                <Sword size={20} color="#c084fc" />
                Requirement 2: Create Asset
              </h3>
              <button className="modal-close" onClick={() => setShowAssetModal(false)}>
                <X size={20} />
              </button>
            </div>
            <form onSubmit={handleCreateAsset}>
              <div className="modal-body">
                <div className="form-group">
                  <label className="form-label">Asset Name *</label>
                  <input
                    type="text"
                    required
                    className="form-input"
                    placeholder="e.g. Hero 3, Titan Shield"
                    value={newAsset.assetName}
                    onChange={(e) => setNewAsset({ ...newAsset, assetName: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Required Level *</label>
                  <input
                    type="number"
                    required
                    min="1"
                    className="form-input"
                    placeholder="e.g. 5"
                    value={newAsset.levelRequire}
                    onChange={(e) => setNewAsset({ ...newAsset, levelRequire: e.target.value })}
                  />
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={() => setShowAssetModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-purple">
                  Create Asset (createasset)
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal 3: Assign Asset */}
      {showAssignModal && (
        <div className="modal-overlay" onClick={() => setShowAssignModal(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3 className="modal-title">
                <LinkIcon size={20} color="var(--accent-blue)" />
                Assign Asset to Player
              </h3>
              <button className="modal-close" onClick={() => setShowAssignModal(false)}>
                <X size={20} />
              </button>
            </div>
            <form onSubmit={handleAssignAsset}>
              <div className="modal-body">
                <div className="form-group">
                  <label className="form-label">Select Player *</label>
                  <select
                    required
                    className="form-select"
                    value={assignment.playerId}
                    onChange={(e) => setAssignment({ ...assignment, playerId: e.target.value })}
                  >
                    <option value="">-- Choose Player --</option>
                    {players.map((p) => (
                      <option key={p.playerId} value={p.playerId}>
                        {p.playerName} (Lv. {p.level})
                      </option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label className="form-label">Select Asset *</label>
                  <select
                    required
                    className="form-select"
                    value={assignment.assetId}
                    onChange={(e) => setAssignment({ ...assignment, assetId: e.target.value })}
                  >
                    <option value="">-- Choose Asset --</option>
                    {assets.map((a) => (
                      <option key={a.assetId} value={a.assetId}>
                        {a.assetName} (Req. Lv {a.levelRequire})
                      </option>
                    ))}
                  </select>
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={() => setShowAssignModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary">
                  Assign (assignasset)
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Footer */}
      <footer className="app-footer">
        <p>Developing Microsoft Azure Solutions - SET01 &bull; Class T2508M Semester 2</p>
        <div className="footer-badges">
          <span className="footer-tag">Azure Functions v4</span>
          <span className="footer-tag">.NET 8 Isolated Worker</span>
          <span className="footer-tag">MySQL Database (BATTLEGAME)</span>
          <span className="footer-tag">React + Vite</span>
        </div>
      </footer>
    </div>
  );
}
