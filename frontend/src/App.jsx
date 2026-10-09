import React, { useState, useEffect, useMemo } from 'react';

// Default mock data aligned with database/schema.sql
const DEFAULT_MOCK_DATA = [
  { no: 1, playerName: "ShadowHunter", level: 45, age: "24", assetName: "Dragon Slayer Sword" },
  { no: 2, playerName: "ShadowHunter", level: 45, age: "24", assetName: "Shadow Walker Boots" },
  { no: 3, playerName: "MysticMage", level: 38, age: "21", assetName: "Staff of Arcane Light" },
  { no: 4, playerName: "MysticMage", level: 38, age: "21", assetName: "Shadow Walker Boots" },
  { no: 5, playerName: "IronVanguard", level: 60, age: "28", assetName: "Dragon Slayer Sword" },
  { no: 6, playerName: "IronVanguard", level: 60, age: "28", assetName: "Titanium Aegis Shield" },
  { no: 7, playerName: "CyberNinja", level: 15, age: "19", assetName: "Shadow Walker Boots" },
  { no: 8, playerName: "PhoenixQueen", level: 52, age: "26", assetName: "Phoenix Wings Armor" },
  { no: 9, playerName: "PhoenixQueen", level: 52, age: "26", assetName: "Frostbite Crossbow" },
  { no: 10, playerName: "PhoenixQueen", level: 52, age: "26", assetName: "Staff of Arcane Light" }
];

export default function App() {
  const [activeTab, setActiveTab] = useState('report'); // 'report' | 'register' | 'asset' | 'config'
  const [apiBaseUrl, setApiBaseUrl] = useState(() => {
    return localStorage.getItem('battlegame_api_url') || 'http://localhost:7071';
  });
  
  // Report state
  const [reportData, setReportData] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [levelFilter, setLevelFilter] = useState('ALL');
  const [usingMock, setUsingMock] = useState(false);

  // Form states for API 1: Register Player
  const [playerForm, setPlayerForm] = useState({
    playerName: '',
    fullName: '',
    age: '',
    level: 1,
    email: ''
  });
  const [playerSubmitting, setPlayerSubmitting] = useState(false);
  const [playerMessage, setPlayerMessage] = useState(null);

  // Form states for API 2: Create Asset
  const [assetForm, setAssetForm] = useState({
    assetName: '',
    levelRequire: 1
  });
  const [assetSubmitting, setAssetSubmitting] = useState(false);
  const [assetMessage, setAssetMessage] = useState(null);

  // Save base URL changes
  const handleSaveApiUrl = (e) => {
    e.preventDefault();
    localStorage.setItem('battlegame_api_url', apiBaseUrl.trim());
    fetchReportData(apiBaseUrl.trim());
  };

  // Fetch report: GET /api/getassetsbyplayer
  const fetchReportData = async (baseUrl = apiBaseUrl) => {
    setLoading(true);
    setError(null);
    setUsingMock(false);

    const cleanUrl = baseUrl.replace(/\/+$/, '');
    const endpoint = `${cleanUrl}/api/getassetsbyplayer`;

    try {
      const response = await fetch(endpoint, {
        method: 'GET',
        headers: {
          'Accept': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error(`HTTP Error ${response.status}: ${response.statusText}`);
      }

      const data = await response.json();
      
      // Standardize casing if necessary
      const normalizedData = (Array.isArray(data) ? data : []).map((item, idx) => ({
        no: item.no ?? item.No ?? (idx + 1),
        playerName: item.playerName ?? item.PlayerName ?? 'Unknown',
        level: item.level ?? item.Level ?? 0,
        age: item.age ?? item.Age ?? 'N/A',
        assetName: item.assetName ?? item.AssetName ?? 'Unknown'
      }));

      setReportData(normalizedData);
    } catch (err) {
      console.error('Fetch error:', err);
      setError(`Cannot connect to Backend API at [${endpoint}]. Please check if Azure Function is running on port 7071 and CORS is enabled.`);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchReportData();
  }, []);

  const handleUseMockData = () => {
    setReportData(DEFAULT_MOCK_DATA);
    setUsingMock(true);
    setError(null);
  };

  // Register Player submit handler: POST /api/registerplayer
  const handleRegisterPlayer = async (e) => {
    e.preventDefault();
    setPlayerSubmitting(true);
    setPlayerMessage(null);

    const cleanUrl = apiBaseUrl.replace(/\/+$/, '');
    const endpoint = `${cleanUrl}/api/registerplayer`;

    try {
      const payload = {
        playerName: playerForm.playerName.trim(),
        fullName: playerForm.fullName.trim() || null,
        age: playerForm.age.trim() || null,
        level: parseInt(playerForm.level, 10) || 1,
        email: playerForm.email.trim() || null
      };

      const response = await fetch(endpoint, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(payload)
      });

      const result = await response.json();

      if (!response.ok) {
        throw new Error(result.message || result.error || `HTTP ${response.status}`);
      }

      setPlayerMessage({
        type: 'success',
        text: `Player "${payload.playerName}" registered successfully! (ID: ${result.data?.playerId || 'Generated'})`
      });

      setPlayerForm({ playerName: '', fullName: '', age: '', level: 1, email: '' });
      fetchReportData(); // Refresh table
    } catch (err) {
      setPlayerMessage({
        type: 'error',
        text: `Registration failed: ${err.message}`
      });
    } finally {
      setPlayerSubmitting(false);
    }
  };

  // Create Asset submit handler: POST /api/createasset
  const handleCreateAsset = async (e) => {
    e.preventDefault();
    setAssetSubmitting(true);
    setAssetMessage(null);

    const cleanUrl = apiBaseUrl.replace(/\/+$/, '');
    const endpoint = `${cleanUrl}/api/createasset`;

    try {
      const payload = {
        assetName: assetForm.assetName.trim(),
        levelRequire: parseInt(assetForm.levelRequire, 10) || 1
      };

      const response = await fetch(endpoint, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(payload)
      });

      const result = await response.json();

      if (!response.ok) {
        throw new Error(result.message || result.error || `HTTP ${response.status}`);
      }

      setAssetMessage({
        type: 'success',
        text: `Asset "${payload.assetName}" created successfully! (Required Level: ${payload.levelRequire})`
      });

      setAssetForm({ assetName: '', levelRequire: 1 });
    } catch (err) {
      setAssetMessage({
        type: 'error',
        text: `Asset creation failed: ${err.message}`
      });
    } finally {
      setAssetSubmitting(false);
    }
  };

  // Filtered report data
  const filteredData = useMemo(() => {
    return reportData.filter(item => {
      const matchesSearch = 
        item.playerName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        item.assetName.toLowerCase().includes(searchTerm.toLowerCase());
      
      if (!matchesSearch) return false;

      if (levelFilter === 'LOW') return item.level < 25;
      if (levelFilter === 'MID') return item.level >= 25 && item.level <= 45;
      if (levelFilter === 'HIGH') return item.level > 45;
      return true;
    });
  }, [reportData, searchTerm, levelFilter]);

  // Quick stats calculations
  const stats = useMemo(() => {
    const totalRecords = reportData.length;
    const uniquePlayers = new Set(reportData.map(r => r.playerName)).size;
    const uniqueAssets = new Set(reportData.map(r => r.assetName)).size;
    const avgLevel = totalRecords > 0 
      ? Math.round(reportData.reduce((acc, curr) => acc + (curr.level || 0), 0) / totalRecords) 
      : 0;

    return { totalRecords, uniquePlayers, uniqueAssets, avgLevel };
  }, [reportData]);

  return (
    <div className="app-container">
      {/* Header Bar */}
      <header className="header-bar">
        <div className="brand-wrapper">
          <div className="brand-icon">
            <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" style={{ color: '#fff' }}>
              <polygon points="12 2 2 7 12 12 22 7 12 2" />
              <polyline points="2 17 12 22 22 17" />
              <polyline points="2 12 12 17 22 12" />
            </svg>
          </div>
          <div>
            <h1 className="brand-title">BATTLEGAME Operations Portal</h1>
            <p className="brand-subtitle">
              <span>Microsoft Azure Solutions</span>
              <span>•</span>
              <span>Azure Functions (.NET 8 Isolated) & SQL Server</span>
            </p>
          </div>
        </div>

        <div style={{ display: 'flex', gap: '10px', alignItems: 'center' }}>
          <div className={`badge ${error ? 'badge-asset' : 'badge-level'}`} style={{ padding: '6px 12px' }}>
            <span style={{ 
              display: 'inline-block', 
              width: '8px', 
              height: '8px', 
              borderRadius: '50%', 
              backgroundColor: error ? '#f43f5e' : (usingMock ? '#f59e0b' : '#10b981'),
              marginRight: '6px'
            }}></span>
            {error ? 'API Disconnected' : (usingMock ? 'Using Mock Preview' : 'Azure Function Live')}
          </div>
          <button 
            id="refresh-btn"
            className="btn btn-secondary" 
            onClick={() => fetchReportData()}
            disabled={loading}
            title="Reload data from Azure Function"
          >
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" className={loading ? "animate-spin" : ""}>
              <path d="M21.5 2v6h-6M21.34 15.57a10 10 0 1 1-.57-8.38l5.67-5.67" />
            </svg>
            Refresh
          </button>
        </div>
      </header>

      {/* KPI Stats Bar */}
      <div className="stats-grid">
        <div className="glass-panel stat-card card-purple">
          <div className="stat-label">Total Assigned Assets</div>
          <div className="stat-value">{stats.totalRecords}</div>
        </div>
        <div className="glass-panel stat-card card-cyan">
          <div className="stat-label">Active Players</div>
          <div className="stat-value">{stats.uniquePlayers}</div>
        </div>
        <div className="glass-panel stat-card card-emerald">
          <div className="stat-label">Unique Equipment</div>
          <div className="stat-value">{stats.uniqueAssets}</div>
        </div>
        <div className="glass-panel stat-card card-amber">
          <div className="stat-label">Average Player Level</div>
          <div className="stat-value">Lv. {stats.avgLevel}</div>
        </div>
      </div>

      {/* Navigation Tabs */}
      <div className="nav-tabs">
        <button 
          id="tab-report"
          className={`tab-btn ${activeTab === 'report' ? 'active' : ''}`}
          onClick={() => setActiveTab('report')}
        >
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <line x1="8" y1="6" x2="21" y2="6"></line>
            <line x1="8" y1="12" x2="21" y2="12"></line>
            <line x1="8" y1="18" x2="21" y2="18"></line>
            <line x1="3" y1="6" x2="3.01" y2="6"></line>
            <line x1="3" y1="12" x2="3.01" y2="12"></line>
            <line x1="3" y1="18" x2="3.01" y2="18"></line>
          </svg>
          Player & Asset Report (Table)
        </button>

        <button 
          id="tab-register"
          className={`tab-btn ${activeTab === 'register' ? 'active' : ''}`}
          onClick={() => setActiveTab('register')}
        >
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
            <circle cx="8.5" cy="7" r="4"></circle>
            <line x1="20" y1="8" x2="20" y2="14"></line>
            <line x1="23" y1="11" x2="17" y2="11"></line>
          </svg>
          Register Player API Test
        </button>

        <button 
          id="tab-asset"
          className={`tab-btn ${activeTab === 'asset' ? 'active' : ''}`}
          onClick={() => setActiveTab('asset')}
        >
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <circle cx="12" cy="12" r="10"></circle>
            <line x1="12" y1="8" x2="12" y2="16"></line>
            <line x1="8" y1="12" x2="16" y2="12"></line>
          </svg>
          Create Asset API Test
        </button>

        <button 
          id="tab-config"
          className={`tab-btn ${activeTab === 'config' ? 'active' : ''}`}
          onClick={() => setActiveTab('config')}
        >
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
            <circle cx="12" cy="12" r="3"></circle>
            <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1 0 2.83 2 2 0 0 1-2.83 0l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-2 2 2 2 0 0 1-2-2v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83 0 2 2 0 0 1 0-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1-2-2 2 2 0 0 1 2-2h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 0-2.83 2 2 0 0 1 2.83 0l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 2-2 2 2 0 0 1 2 2v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 0 2 2 0 0 1 0 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 2 2 2 2 0 0 1-2 2h-.09a1.65 1.65 0 0 0-1.51 1z"></path>
          </svg>
          API Settings & Azure Endpoints
        </button>
      </div>

      {/* TAB 1: REPORT VIEW (Primary requirement) */}
      {activeTab === 'report' && (
        <section>
          {/* Error Banner */}
          {error && (
            <div className="alert alert-error">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <circle cx="12" cy="12" r="10"></circle>
                <line x1="12" y1="8" x2="12" y2="12"></line>
                <line x1="12" y1="16" x2="12.01" y2="16"></line>
              </svg>
              <div style={{ flex: 1 }}>
                <strong>Connection Error:</strong> {error}
                <div style={{ marginTop: '8px', display: 'flex', gap: '8px' }}>
                  <button className="btn btn-secondary" style={{ padding: '4px 10px', fontSize: '12px' }} onClick={() => fetchReportData()}>
                    Retry Connection
                  </button>
                  <button className="btn btn-outline" style={{ padding: '4px 10px', fontSize: '12px' }} onClick={handleUseMockData}>
                    Load Mock Sample Data
                  </button>
                </div>
              </div>
            </div>
          )}

          {/* Using Mock Data Notification */}
          {usingMock && (
            <div className="alert alert-info">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <circle cx="12" cy="12" r="10"></circle>
                <line x1="12" y1="16" x2="12" y2="12"></line>
                <line x1="12" y1="8" x2="12.01" y2="8"></line>
              </svg>
              <div>
                <strong>Preview Mode:</strong> Currently displaying pre-loaded sample data from <code>database/schema.sql</code>. Once your backend Azure Function is running on <code>http://localhost:7071</code>, click <strong>Refresh</strong> to query SQL Server directly.
              </div>
            </div>
          )}

          {/* Toolbar */}
          <div className="toolbar-bar">
            <div className="search-box">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" style={{ color: '#64748b' }}>
                <circle cx="11" cy="11" r="8"></circle>
                <line x1="21" y1="21" x2="16.65" y2="16.65"></line>
              </svg>
              <input 
                id="search-input"
                type="text" 
                className="search-input"
                placeholder="Search by Player name or Asset name..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
              {searchTerm && (
                <button 
                  onClick={() => setSearchTerm('')} 
                  style={{ background: 'none', border: 'none', color: '#64748b', cursor: 'pointer', padding: '4px' }}
                >
                  ✕
                </button>
              )}
            </div>

            <div style={{ display: 'flex', gap: '10px', alignItems: 'center' }}>
              <select 
                className="form-select" 
                style={{ width: 'auto', padding: '8px 12px' }}
                value={levelFilter}
                onChange={(e) => setLevelFilter(e.target.value)}
              >
                <option value="ALL">All Levels</option>
                <option value="LOW">Level 1 - 24</option>
                <option value="MID">Level 25 - 45</option>
                <option value="HIGH">Level 46+</option>
              </select>

              <span style={{ fontSize: '13px', color: '#94a3b8' }}>
                Showing <strong>{filteredData.length}</strong> records
              </span>
            </div>
          </div>

          {/* Report Data Table */}
          <div className="table-wrapper">
            <table className="custom-table" id="player-asset-table">
              <thead>
                <tr>
                  <th style={{ width: '70px', textAlign: 'center' }}>No</th>
                  <th>Player name</th>
                  <th style={{ width: '130px' }}>Level</th>
                  <th style={{ width: '100px' }}>Age</th>
                  <th>Asset name</th>
                </tr>
              </thead>
              <tbody>
                {loading ? (
                  // Skeleton loader rows
                  Array.from({ length: 5 }).map((_, i) => (
                    <tr key={`skeleton-${i}`}>
                      <td colSpan={5} style={{ padding: '6px 0' }}>
                        <div className="skeleton-row"></div>
                      </td>
                    </tr>
                  ))
                ) : filteredData.length === 0 ? (
                  <tr>
                    <td colSpan={5} style={{ textAlign: 'center', padding: '48px 20px', color: '#94a3b8' }}>
                      <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" style={{ margin: '0 auto 12px', opacity: 0.5 }}>
                        <circle cx="12" cy="12" r="10"></circle>
                        <line x1="8" y1="12" x2="16" y2="12"></line>
                      </svg>
                      <div>No records found matching your filter criteria.</div>
                    </td>
                  </tr>
                ) : (
                  filteredData.map((item) => (
                    <tr key={`${item.no}-${item.playerName}-${item.assetName}`}>
                      <td style={{ textAlign: 'center' }}>
                        <span className="badge badge-no">{item.no}</span>
                      </td>
                      <td>
                        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                          <div style={{ 
                            width: '30px', 
                            height: '30px', 
                            borderRadius: '8px', 
                            background: 'rgba(99, 102, 241, 0.2)', 
                            display: 'flex', 
                            alignItems: 'center', 
                            justifyContent: 'center',
                            fontSize: '12px',
                            fontWeight: 'bold',
                            color: '#c7d2fe'
                          }}>
                            {item.playerName.charAt(0).toUpperCase()}
                          </div>
                          <span style={{ fontWeight: 600, color: '#f1f5f9' }}>{item.playerName}</span>
                        </div>
                      </td>
                      <td>
                        <span className={`badge ${item.level >= 45 ? 'badge-high-level' : 'badge-level'}`}>
                          Lv. {item.level}
                        </span>
                      </td>
                      <td>
                        <span style={{ color: '#94a3b8', fontFamily: 'var(--font-mono)' }}>{item.age}</span>
                      </td>
                      <td>
                        <span className="badge badge-asset">{item.assetName}</span>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </section>
      )}

      {/* TAB 2: REGISTER PLAYER FORM (API 1 Testing) */}
      {activeTab === 'register' && (
        <div className="glass-panel form-panel">
          <div style={{ marginBottom: '20px' }}>
            <h2 style={{ fontSize: '20px', fontWeight: 700, marginBottom: '6px' }}>Register New Player</h2>
            <p style={{ color: 'var(--text-muted)', fontSize: '13px' }}>
              Invokes <code>POST /api/registerplayer</code> to insert record into SQL Server table <code>dbo.Player</code>.
            </p>
          </div>

          {playerMessage && (
            <div className={`alert ${playerMessage.type === 'success' ? 'alert-success' : 'alert-error'}`}>
              <div>{playerMessage.text}</div>
            </div>
          )}

          <form onSubmit={handleRegisterPlayer}>
            <div className="form-group">
              <label className="form-label" htmlFor="input-player-name">Player Name (Required) *</label>
              <input 
                id="input-player-name"
                type="text" 
                className="form-input" 
                placeholder="e.g. DragonWarrior" 
                required
                value={playerForm.playerName}
                onChange={(e) => setPlayerForm({ ...playerForm, playerName: e.target.value })}
              />
              <div className="form-helper">Unique gaming nickname (up to 64 chars).</div>
            </div>

            <div className="form-group">
              <label className="form-label" htmlFor="input-full-name">Full Name</label>
              <input 
                id="input-full-name"
                type="text" 
                className="form-input" 
                placeholder="e.g. Tran Van Nam" 
                value={playerForm.fullName}
                onChange={(e) => setPlayerForm({ ...playerForm, fullName: e.target.value })}
              />
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px' }}>
              <div className="form-group">
                <label className="form-label" htmlFor="input-age">Age</label>
                <input 
                  id="input-age"
                  type="text" 
                  className="form-input" 
                  placeholder="e.g. 23" 
                  value={playerForm.age}
                  onChange={(e) => setPlayerForm({ ...playerForm, age: e.target.value })}
                />
              </div>
              <div className="form-group">
                <label className="form-label" htmlFor="input-level">Starting Level</label>
                <input 
                  id="input-level"
                  type="number" 
                  min="1"
                  className="form-input" 
                  placeholder="1" 
                  value={playerForm.level}
                  onChange={(e) => setPlayerForm({ ...playerForm, level: e.target.value })}
                />
              </div>
            </div>

            <div className="form-group">
              <label className="form-label" htmlFor="input-email">Email</label>
              <input 
                id="input-email"
                type="email" 
                className="form-input" 
                placeholder="e.g. player@battlegame.io" 
                value={playerForm.email}
                onChange={(e) => setPlayerForm({ ...playerForm, email: e.target.value })}
              />
            </div>

            <button 
              id="submit-register-btn"
              type="submit" 
              className="btn btn-primary" 
              style={{ width: '100%', justifyContent: 'center' }}
              disabled={playerSubmitting}
            >
              {playerSubmitting ? (
                <>
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" className="animate-spin">
                    <circle cx="12" cy="12" r="10"></circle>
                    <path d="M12 2a10 10 0 0 1 10 10"></path>
                  </svg>
                  Registering Player...
                </>
              ) : 'Register Player'}
            </button>
          </form>
        </div>
      )}

      {/* TAB 3: CREATE ASSET FORM (API 2 Testing) */}
      {activeTab === 'asset' && (
        <div className="glass-panel form-panel">
          <div style={{ marginBottom: '20px' }}>
            <h2 style={{ fontSize: '20px', fontWeight: 700, marginBottom: '6px' }}>Create New Asset</h2>
            <p style={{ color: 'var(--text-muted)', fontSize: '13px' }}>
              Invokes <code>POST /api/createasset</code> to register equipment in SQL Server table <code>dbo.Asset</code>.
            </p>
          </div>

          {assetMessage && (
            <div className={`alert ${assetMessage.type === 'success' ? 'alert-success' : 'alert-error'}`}>
              <div>{assetMessage.text}</div>
            </div>
          )}

          <form onSubmit={handleCreateAsset}>
            <div className="form-group">
              <label className="form-label" htmlFor="input-asset-name">Asset Name (Required) *</label>
              <input 
                id="input-asset-name"
                type="text" 
                className="form-input" 
                placeholder="e.g. Excalibur Holy Blade" 
                required
                value={assetForm.assetName}
                onChange={(e) => setAssetForm({ ...assetForm, assetName: e.target.value })}
              />
              <div className="form-helper">Unique item name (up to 64 chars).</div>
            </div>

            <div className="form-group">
              <label className="form-label" htmlFor="input-level-require">Level Requirement</label>
              <input 
                id="input-level-require"
                type="number" 
                min="1"
                className="form-input" 
                placeholder="10" 
                value={assetForm.levelRequire}
                onChange={(e) => setAssetForm({ ...assetForm, levelRequire: e.target.value })}
              />
              <div className="form-helper">Minimum player level required to equip this item.</div>
            </div>

            <button 
              id="submit-asset-btn"
              type="submit" 
              className="btn btn-emerald" 
              style={{ width: '100%', justifyContent: 'center' }}
              disabled={assetSubmitting}
            >
              {assetSubmitting ? (
                <>
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" className="animate-spin">
                    <circle cx="12" cy="12" r="10"></circle>
                    <path d="M12 2a10 10 0 0 1 10 10"></path>
                  </svg>
                  Creating Asset...
                </>
              ) : 'Create Asset'}
            </button>
          </form>
        </div>
      )}

      {/* TAB 4: CONFIGURATION & ENDPOINTS INFO */}
      {activeTab === 'config' && (
        <div className="glass-panel form-panel" style={{ maxWidth: '800px' }}>
          <div style={{ marginBottom: '24px' }}>
            <h2 style={{ fontSize: '20px', fontWeight: 700, marginBottom: '6px' }}>Azure Functions API Configuration</h2>
            <p style={{ color: 'var(--text-muted)', fontSize: '13px' }}>
              Switch seamlessly between your local development environment and your Azure Cloud Function URL.
            </p>
          </div>

          <form onSubmit={handleSaveApiUrl} style={{ marginBottom: '32px' }}>
            <div className="form-group">
              <label className="form-label" htmlFor="input-api-endpoint">Backend Base URL</label>
              <div style={{ display: 'flex', gap: '10px' }}>
                <input 
                  id="input-api-endpoint"
                  type="url" 
                  className="form-input" 
                  style={{ fontFamily: 'var(--font-mono)' }}
                  value={apiBaseUrl}
                  onChange={(e) => setApiBaseUrl(e.target.value)}
                  placeholder="https://<your-app>.azurewebsites.net or http://localhost:7071"
                  required
                />
                <button type="submit" className="btn btn-primary">
                  Save & Connect
                </button>
              </div>
              <div className="form-helper">
                Default local: <code>http://localhost:7071</code> | Production: <code>https://&lt;function-name&gt;.azurewebsites.net</code>
              </div>
            </div>
          </form>

          <h3 style={{ fontSize: '16px', fontWeight: 700, marginBottom: '14px', borderTop: '1px solid var(--border-color)', paddingTop: '20px' }}>
            Implemented REST API Endpoints
          </h3>
          <div style={{ display: 'grid', gap: '12px' }}>
            <div style={{ background: 'var(--bg-surface-elevated)', padding: '14px', borderRadius: '8px', border: '1px solid var(--border-color)' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '6px' }}>
                <span className="badge" style={{ background: '#0284c7', color: '#fff' }}>GET</span>
                <code style={{ fontSize: '14px', color: '#38bdf8' }}>/api/getassetsbyplayer</code>
              </div>
              <p style={{ fontSize: '13px', color: '#94a3b8' }}>
                Performs SQL 3-table INNER JOIN (<code>Player</code>, <code>PlayerAsset</code>, <code>Asset</code>) and returns list formatted as <code>No, PlayerName, Level, Age, AssetName</code>.
              </p>
            </div>

            <div style={{ background: 'var(--bg-surface-elevated)', padding: '14px', borderRadius: '8px', border: '1px solid var(--border-color)' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '6px' }}>
                <span className="badge" style={{ background: '#16a34a', color: '#fff' }}>POST</span>
                <code style={{ fontSize: '14px', color: '#4ade80' }}>/api/registerplayer</code>
              </div>
              <p style={{ fontSize: '13px', color: '#94a3b8' }}>
                Registers new Player entity into SQL Server. Auto-generates GUID <code>PlayerId</code> if not supplied in body.
              </p>
            </div>

            <div style={{ background: 'var(--bg-surface-elevated)', padding: '14px', borderRadius: '8px', border: '1px solid var(--border-color)' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '6px' }}>
                <span className="badge" style={{ background: '#16a34a', color: '#fff' }}>POST</span>
                <code style={{ fontSize: '14px', color: '#4ade80' }}>/api/createasset</code>
              </div>
              <p style={{ fontSize: '13px', color: '#94a3b8' }}>
                Inserts a new item/asset into database with level requirements and auto-generated GUID <code>AssetId</code>.
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Footer */}
      <footer style={{ marginTop: '50px', textAlign: 'center', fontSize: '13px', color: '#64748b', borderTop: '1px solid #1e293b', paddingTop: '20px' }}>
        BattleGame Azure Solutions • .NET 8 Isolated Worker & ReactJS Architecture • Class T2508M
      </footer>
    </div>
  );
}
