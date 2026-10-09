import React, { useState, useEffect } from 'react';
import { 
  fetchPlayerAssetsReport, 
  registerPlayer, 
  createAsset, 
  assignAsset, 
  seedDatabase,
  DEFAULT_REPORT_DATA 
} from './services/api';
import { 
  Gamepad2, 
  UserPlus, 
  ShieldAlert, 
  Database, 
  RefreshCw, 
  Search, 
  CloudUpload, 
  Sparkles, 
  CheckCircle2, 
  AlertCircle,
  Link2,
  TableProperties
} from 'lucide-react';

export default function App() {
  const [activeTab, setActiveTab] = useState('report');
  const [reportData, setReportData] = useState([]);
  const [loading, setLoading] = useState(false);
  const [isLiveApi, setIsLiveApi] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [statusMessage, setStatusMessage] = useState(null);

  // Form states
  const [playerForm, setPlayerForm] = useState({
    playerName: '',
    fullName: '',
    age: '20',
    level: 1,
    email: '',
  });

  const [assetForm, setAssetForm] = useState({
    assetName: '',
    levelRequire: 1,
  });

  const [assignForm, setAssignForm] = useState({
    playerId: '',
    assetId: '',
  });

  const [submitting, setSubmitting] = useState(false);

  // Load report data on mount
  useEffect(() => {
    loadReport();
  }, []);

  async function loadReport() {
    setLoading(true);
    setStatusMessage(null);
    try {
      const res = await fetchPlayerAssetsReport();
      setReportData(res.data || DEFAULT_REPORT_DATA);
      setIsLiveApi(res.isLive);
      if (res.isLive) {
        showMessage('success', 'Đã kết nối với Azure Functions: Tải dữ liệu trực tiếp từ cơ sở dữ liệu thành công.');
      } else {
        showMessage('info', 'Azure Function đang ngoại tuyến. Đang hiển thị dữ liệu mẫu theo yêu cầu đề bài.');
      }
    } catch {
      setReportData(DEFAULT_REPORT_DATA);
      setIsLiveApi(false);
    } finally {
      setLoading(false);
    }
  }

  function showMessage(type, text) {
    setStatusMessage({ type, text });
    setTimeout(() => {
      setStatusMessage((prev) => (prev?.text === text ? null : prev));
    }, 6000);
  }

  async function handleRegisterPlayer(e) {
    e.preventDefault();
    setSubmitting(true);
    try {
      const res = await registerPlayer({
        playerName: playerForm.playerName.trim(),
        fullName: playerForm.fullName.trim(),
        age: playerForm.age.toString().trim(),
        level: parseInt(playerForm.level, 10) || 1,
        email: playerForm.email.trim(),
      });
      showMessage('success', `Đăng ký người chơi '${playerForm.playerName}' thành công! (Mã ID: ${res.data?.playerId || 'Đã tạo'})`);
      setPlayerForm({ playerName: '', fullName: '', age: '20', level: 1, email: '' });
      await loadReport();
    } catch (err) {
      showMessage('error', err.message || 'Lỗi khi đăng ký người chơi.');
    } finally {
      setSubmitting(false);
    }
  }

  async function handleCreateAsset(e) {
    e.preventDefault();
    setSubmitting(true);
    try {
      const res = await createAsset({
        assetName: assetForm.assetName.trim(),
        levelRequire: parseInt(assetForm.levelRequire, 10) || 1,
      });
      showMessage('success', `Tạo tài sản '${assetForm.assetName}' thành công! (Mã ID: ${res.data?.assetId || 'Đã tạo'})`);
      setAssetForm({ assetName: '', levelRequire: 1 });
      await loadReport();
    } catch (err) {
      showMessage('error', err.message || 'Lỗi khi tạo tài sản.');
    } finally {
      setSubmitting(false);
    }
  }

  async function handleAssignAsset(e) {
    e.preventDefault();
    setSubmitting(true);
    try {
      await assignAsset({
        playerId: assignForm.playerId.trim(),
        assetId: assignForm.assetId.trim(),
      });
      showMessage('success', 'Gán tài sản cho người chơi thành công!');
      setAssignForm({ playerId: '', assetId: '' });
      await loadReport();
    } catch (err) {
      showMessage('error', err.message || 'Lỗi khi gán tài sản.');
    } finally {
      setSubmitting(false);
    }
  }

  async function handleSeedDatabase() {
    setLoading(true);
    try {
      await seedDatabase();
      showMessage('success', 'Đã nạp dữ liệu mẫu ban đầu của bài thi vào cơ sở dữ liệu!');
      await loadReport();
    } catch (err) {
      showMessage('error', `Lỗi khi nạp dữ liệu mẫu: ${err.message}`);
    } finally {
      setLoading(false);
    }
  }

  // Filter report rows
  const filteredReport = reportData.filter((item) => {
    const q = searchQuery.toLowerCase();
    const pName = (item.playerName || item['Player name'] || '').toLowerCase();
    const aName = (item.assetName || item['Asset name'] || '').toLowerCase();
    return pName.includes(q) || aName.includes(q);
  });

  return (
    <div className="app-container">
      {/* Top Header */}
      <header className="app-header">
        <div className="brand-wrapper">
          <div className="brand-icon">
            <Gamepad2 size={28} />
          </div>
          <div>
            <h1 className="brand-title">BATTLEGAME STUDIO</h1>
            <p className="brand-subtitle">Bài thi thực hành Microsoft Azure Solutions - SET01</p>
          </div>
        </div>

        <div className="header-meta">
          <div className={`status-badge ${isLiveApi ? '' : 'offline'}`}>
            <span className="status-dot"></span>
            <span>{isLiveApi ? 'API Azure Function Trực Tuyến' : 'Chế Độ Ngoại Tuyến (Demo)'}</span>
          </div>
          <div className="student-tag">
            Thí sinh: <strong>Lê Trung Kiên</strong> (PRAM)
          </div>
        </div>
      </header>

      {/* Status Alert Banner */}
      {statusMessage && (
        <div className={`alert-box ${statusMessage.type === 'error' ? 'alert-error' : 'alert-success'}`}>
          {statusMessage.type === 'error' ? <AlertCircle size={20} /> : <CheckCircle2 size={20} />}
          <span>{statusMessage.text}</span>
        </div>
      )}

      {/* Navigation Tabs */}
      <nav className="nav-tabs" aria-label="Portal Navigation">
        <button
          className={`tab-btn ${activeTab === 'report' ? 'active' : ''}`}
          onClick={() => setActiveTab('report')}
        >
          <TableProperties size={18} />
          <span>Báo Cáo Tài Sản (Câu 3)</span>
        </button>
        <button
          className={`tab-btn ${activeTab === 'register' ? 'active' : ''}`}
          onClick={() => setActiveTab('register')}
        >
          <UserPlus size={18} />
          <span>Đăng Ký Người Chơi (Câu 1)</span>
        </button>
        <button
          className={`tab-btn ${activeTab === 'create-asset' ? 'active' : ''}`}
          onClick={() => setActiveTab('create-asset')}
        >
          <ShieldAlert size={18} />
          <span>Tạo Tài Sản Mới (Câu 2)</span>
        </button>
        <button
          className={`tab-btn ${activeTab === 'assign' ? 'active' : ''}`}
          onClick={() => setActiveTab('assign')}
        >
          <Link2 size={18} />
          <span>Gán Tài Sản Cho Người Chơi</span>
        </button>
        <button
          className={`tab-btn ${activeTab === 'cloud-deploy' ? 'active' : ''}`}
          onClick={() => setActiveTab('cloud-deploy')}
        >
          <CloudUpload size={18} />
          <span>Hướng Dẫn Deploy Cloud (Câu 5)</span>
        </button>
      </nav>

      {/* Main Content Area */}
      <main>
        {/* TAB 1: REPORT TABLE (Requirement 3 & 4) */}
        {activeTab === 'report' && (
          <section className="content-card">
            <div className="card-header-bar">
              <div>
                <h2 className="section-title">
                  <Database size={22} color="#2563eb" />
                  Báo Cáo Tài Sản Người Chơi
                </h2>
                <p className="section-desc">
                  Dữ liệu được truy vấn từ API Azure Function: <code>GET /api/getassetsbyplayer</code>
                </p>
              </div>

              <div className="toolbar">
                <div className="search-input-wrapper">
                  <Search size={16} />
                  <input
                    type="text"
                    className="search-input"
                    placeholder="Tìm theo tên người chơi hoặc tài sản..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                  />
                </div>

                <button 
                  className="btn btn-secondary" 
                  onClick={loadReport} 
                  disabled={loading}
                  title="Tải lại dữ liệu từ Azure Function"
                >
                  <RefreshCw size={16} className={loading ? 'animate-spin' : ''} />
                  <span>Làm mới</span>
                </button>

                <button 
                  className="btn btn-secondary" 
                  onClick={handleSeedDatabase} 
                  disabled={loading}
                  title="Nạp lại dữ liệu mẫu (Player 1-3, Hero 1-2)"
                >
                  <Sparkles size={16} />
                  <span>Nạp Dữ Liệu Mẫu</span>
                </button>
              </div>
            </div>

            {/* The exact Exam table layout */}
            <div className="table-wrapper">
              <table className="report-table">
                <thead>
                  <tr>
                    <th style={{ width: '80px' }}>No</th>
                    <th>Player name</th>
                    <th>Level</th>
                    <th>Age</th>
                    <th>Asset name</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredReport.length > 0 ? (
                    filteredReport.map((row, idx) => (
                      <tr key={row.playerId && row.assetId ? `${row.playerId}-${row.assetId}` : idx}>
                        <td style={{ fontWeight: 700, color: '#2563eb' }}>{row.no || idx + 1}</td>
                        <td style={{ fontWeight: 600 }}>{row.playerName || row['Player name']}</td>
                        <td>
                          <span className="badge-level">
                            Cấp {row.level ?? row['Level']}
                          </span>
                        </td>
                        <td className="badge-age">{row.age || row['Age']}</td>
                        <td>
                          <span className="badge-asset">
                            <ShieldAlert size={14} />
                            {row.assetName || row['Asset name']}
                          </span>
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan={5} style={{ textAlign: 'center', padding: '2rem', color: '#64748b' }}>
                        Không tìm thấy bản ghi nào phù hợp với từ khóa tìm kiếm.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>

            <div style={{ marginTop: '1.25rem', display: 'flex', justifyContent: 'space-between', fontSize: '0.85rem', color: '#64748b' }}>
              <span>Tổng số bản ghi: <strong>{filteredReport.length}</strong></span>
              <span>Endpoint: <code>GET /api/getassetsbyplayer</code></span>
            </div>
          </section>
        )}

        {/* TAB 2: REGISTER PLAYER (Requirement 1) */}
        {activeTab === 'register' && (
          <section className="content-card">
            <div className="card-header-bar">
              <div>
                <h2 className="section-title">
                  <UserPlus size={22} color="#16a34a" />
                  Câu 1: Đăng Ký Người Chơi Mới
                </h2>
                <p className="section-desc">
                  Thêm thông tin người chơi vào bảng <code>Player</code> thông qua API <code>POST /api/registerplayer</code>
                </p>
              </div>
            </div>

            <form onSubmit={handleRegisterPlayer}>
              <div className="form-grid">
                <div className="form-group">
                  <label className="form-label">Tên người chơi (Player Name) *</label>
                  <input
                    type="text"
                    className="form-input"
                    placeholder="Ví dụ: Player 4"
                    required
                    maxLength={64}
                    value={playerForm.playerName}
                    onChange={(e) => setPlayerForm({ ...playerForm, playerName: e.target.value })}
                  />
                  <span className="form-tip">Tên hiển thị / tài khoản duy nhất (nvarchar 64)</span>
                </div>

                <div className="form-group">
                  <label className="form-label">Họ và tên (Full Name) *</label>
                  <input
                    type="text"
                    className="form-input"
                    placeholder="Ví dụ: Lê Trung Kiên"
                    required
                    maxLength={128}
                    value={playerForm.fullName}
                    onChange={(e) => setPlayerForm({ ...playerForm, fullName: e.target.value })}
                  />
                  <span className="form-tip">Họ tên đầy đủ của người chơi (nvarchar 128)</span>
                </div>

                <div className="form-group">
                  <label className="form-label">Tuổi (Age) *</label>
                  <input
                    type="text"
                    className="form-input"
                    placeholder="Ví dụ: 21"
                    required
                    maxLength={10}
                    value={playerForm.age}
                    onChange={(e) => setPlayerForm({ ...playerForm, age: e.target.value })}
                  />
                  <span className="form-tip">Độ tuổi người chơi (nvarchar 10)</span>
                </div>

                <div className="form-group">
                  <label className="form-label">Cấp độ hiện tại (Level) *</label>
                  <input
                    type="number"
                    min={1}
                    className="form-input"
                    placeholder="1"
                    required
                    value={playerForm.level}
                    onChange={(e) => setPlayerForm({ ...playerForm, level: parseInt(e.target.value, 10) || 1 })}
                  />
                  <span className="form-tip">Cấp độ ban đầu [Level] int</span>
                </div>

                <div className="form-group" style={{ gridColumn: '1 / -1' }}>
                  <label className="form-label">Địa chỉ Email *</label>
                  <input
                    type="email"
                    className="form-input"
                    placeholder="Ví dụ: player4@battlegame.vn"
                    required
                    maxLength={64}
                    value={playerForm.email}
                    onChange={(e) => setPlayerForm({ ...playerForm, email: e.target.value })}
                  />
                  <span className="form-tip">Hộp thư điện tử liên hệ (nvarchar 64)</span>
                </div>
              </div>

              <button type="submit" className="btn btn-primary" disabled={submitting}>
                <UserPlus size={16} />
                <span>{submitting ? 'Đang đăng ký...' : 'Xác Nhận Đăng Ký (POST /registerplayer)'}</span>
              </button>
            </form>
          </section>
        )}

        {/* TAB 3: CREATE ASSET (Requirement 2) */}
        {activeTab === 'create-asset' && (
          <section className="content-card">
            <div className="card-header-bar">
              <div>
                <h2 className="section-title">
                  <ShieldAlert size={22} color="#0284c7" />
                  Câu 2: Tạo Tài Sản Game Mới
                </h2>
                <p className="section-desc">
                  Thêm mới trang bị hoặc anh hùng vào bảng <code>Asset</code> thông qua API <code>POST /api/createasset</code>
                </p>
              </div>
            </div>

            <form onSubmit={handleCreateAsset}>
              <div className="form-grid">
                <div className="form-group">
                  <label className="form-label">Tên tài sản (Asset Name) *</label>
                  <input
                    type="text"
                    className="form-input"
                    placeholder="Ví dụ: Hero 3 hoặc Kiếm Huyền Thoại"
                    required
                    maxLength={64}
                    value={assetForm.assetName}
                    onChange={(e) => setAssetForm({ ...assetForm, assetName: e.target.value })}
                  />
                  <span className="form-tip">Tên anh hùng hoặc trang bị trong game (nvarchar 64)</span>
                </div>

                <div className="form-group">
                  <label className="form-label">Cấp độ yêu cầu tối thiểu (LevelRequire) *</label>
                  <input
                    type="number"
                    min={1}
                    className="form-input"
                    placeholder="1"
                    required
                    value={assetForm.levelRequire}
                    onChange={(e) => setAssetForm({ ...assetForm, levelRequire: parseInt(e.target.value, 10) || 1 })}
                  />
                  <span className="form-tip">Cấp độ tối thiểu của người chơi để sử dụng (int)</span>
                </div>
              </div>

              <button type="submit" className="btn btn-primary" disabled={submitting}>
                <ShieldAlert size={16} />
                <span>{submitting ? 'Đang tạo...' : 'Xác Nhận Tạo Tài Sản (POST /createasset)'}</span>
              </button>
            </form>
          </section>
        )}

        {/* TAB 4: ASSIGN ASSET (Junction Link) */}
        {activeTab === 'assign' && (
          <section className="content-card">
            <div className="card-header-bar">
              <div>
                <h2 className="section-title">
                  <Link2 size={22} color="#7c3aed" />
                  Gán Tài Sản Cho Người Chơi (Bảng PlayerAsset)
                </h2>
                <p className="section-desc">
                  Liên kết người chơi với tài sản để xuất hiện trong báo cáo ở Câu 3.
                </p>
              </div>
            </div>

            <form onSubmit={handleAssignAsset}>
              <div className="form-grid">
                <div className="form-group">
                  <label className="form-label">Mã Người Chơi (Player ID - UUID) *</label>
                  <input
                    type="text"
                    className="form-input"
                    placeholder="Ví dụ: 7694ee4e-c521-4e73-badc-302be9d87868"
                    required
                    value={assignForm.playerId}
                    onChange={(e) => setAssignForm({ ...assignForm, playerId: e.target.value })}
                  />
                  <span className="form-tip">Mã Guid của người chơi trong bảng dbo.Player</span>
                </div>

                <div className="form-group">
                  <label className="form-label">Mã Tài Sản (Asset ID - UUID) *</label>
                  <input
                    type="text"
                    className="form-input"
                    placeholder="Ví dụ: 9b118b67-8268-43af-8366-060a2945a53b"
                    required
                    value={assignForm.assetId}
                    onChange={(e) => setAssignForm({ ...assignForm, assetId: e.target.value })}
                  />
                  <span className="form-tip">Mã Guid của tài sản trong bảng dbo.Asset</span>
                </div>
              </div>

              <button type="submit" className="btn btn-primary" disabled={submitting}>
                <Link2 size={16} />
                <span>{submitting ? 'Đang gán...' : 'Xác Nhận Gán Tài Sản (POST /assignasset)'}</span>
              </button>
            </form>
          </section>
        )}

        {/* TAB 5: DEPLOY TUTORIAL (Requirement 5) */}
        {activeTab === 'cloud-deploy' && (
          <section className="content-card">
            <div className="card-header-bar">
              <div>
                <h2 className="section-title">
                  <CloudUpload size={22} color="#0284c7" />
                  Câu 5: Hướng Dẫn Triển Khai Azure Function Lên Azure Cloud
                </h2>
                <p className="section-desc">
                  Tài liệu hướng dẫn triển khai hệ thống máy chủ không máy chủ (Serverless) và Azure SQL Database
                </p>
              </div>
            </div>

            <div className="guide-card">
              <div className="guide-step">
                <div className="step-num">1</div>
                <div>
                  <h3 style={{ fontSize: '1rem', fontWeight: 700 }}>Bước 1: Khởi tạo Azure SQL Database</h3>
                  <p style={{ fontSize: '0.875rem', color: '#475569', marginTop: '0.25rem' }}>
                    Tạo cơ sở dữ liệu có tên <strong>BATTLEGAME</strong> trên Azure Portal hoặc qua Azure CLI, bật cho phép các dịch vụ Azure truy cập tường lửa, sau đó chạy script khởi tạo.
                  </p>
                  <div className="code-block">
                    az sql server create -g rg-battlegame -n sql-battlegame -u battlegameadmin -p P@ssw0rd2026!<br/>
                    az sql db create -g rg-battlegame -s sql-battlegame -n BATTLEGAME --service-objective Basic
                  </div>
                </div>
              </div>

              <div className="guide-step">
                <div className="step-num">2</div>
                <div>
                  <h3 style={{ fontSize: '1rem', fontWeight: 700 }}>Bước 2: Tạo Function App trên Azure Cloud</h3>
                  <p style={{ fontSize: '0.875rem', color: '#475569', marginTop: '0.25rem' }}>
                    Tạo Azure Function App theo gói Consumption Plan sử dụng môi trường thực thi .NET 8 Isolated Worker cùng tài khoản Azure Storage.
                  </p>
                  <div className="code-block">
                    az functionapp create -g rg-battlegame -n func-battlegame-api \<br/>
                    &nbsp;&nbsp;--storage-account stbattlegame --consumption-plan-location southeastasia \<br/>
                    &nbsp;&nbsp;--functions-version 4 --runtime dotnet-isolated --os-type Windows
                  </div>
                </div>
              </div>

              <div className="guide-step">
                <div className="step-num">3</div>
                <div>
                  <h3 style={{ fontSize: '1rem', fontWeight: 700 }}>Bước 3: Cấu hình Connection String & Thiết lập CORS</h3>
                  <p style={{ fontSize: '0.875rem', color: '#475569', marginTop: '0.25rem' }}>
                    Thêm chuỗi kết nối SQL <code>SqlConnectionString</code> vào Application Settings và mở khóa CORS cho ứng dụng Web Frontend.
                  </p>
                  <div className="code-block">
                    az functionapp config appsettings set -g rg-battlegame -n func-battlegame-api \<br/>
                    &nbsp;&nbsp;--settings "SqlConnectionString=Server=tcp:sql-battlegame.database.windows.net,1433;Database=BATTLEGAME;User ID=battlegameadmin;Password=P@ssw0rd2026!;Encrypt=True;"<br/>
                    az functionapp cors add -g rg-battlegame -n func-battlegame-api --allowed-origins "*"
                  </div>
                </div>
              </div>

              <div className="guide-step" style={{ marginBottom: 0 }}>
                <div className="step-num">4</div>
                <div>
                  <h3 style={{ fontSize: '1rem', fontWeight: 700 }}>Bước 4: Xuất bản mã nguồn lên Cloud</h3>
                  <p style={{ fontSize: '0.875rem', color: '#475569', marginTop: '0.25rem' }}>
                    Triển khai dự án lên Function App thông qua Azure Functions Core Tools hoặc Visual Studio Code.
                  </p>
                  <div className="code-block">
                    cd backend/BattleGame.Functions<br/>
                    func azure functionapp publish func-battlegame-api
                  </div>
                </div>
              </div>
            </div>

            <p style={{ fontSize: '0.875rem', color: '#2563eb', textAlign: 'center', marginTop: '1.25rem', fontWeight: 600 }}>
              Tài liệu hướng dẫn chi tiết từng bước được lưu tại: <code>docs/DEPLOYMENT.md</code>
            </p>
          </section>
        )}
      </main>

      {/* Footer */}
      <footer className="app-footer">
        <p>Giải pháp Điện toán Đám mây BATTLEGAME • Đề thi thực hành Microsoft Azure SET01 • Xây dựng bằng .NET 8 Isolated & React</p>
        <p style={{ marginTop: '0.35rem', color: '#64748b' }}>Thí sinh: Lê Trung Kiên • Lớp: T2508M • Nhánh Git: PRAM_LeTrungKien</p>
      </footer>
    </div>
  );
}
