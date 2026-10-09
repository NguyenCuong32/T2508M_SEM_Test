// BATTLEGAME Dashboard JavaScript Application (Tiếng Việt)
document.addEventListener('DOMContentLoaded', () => {
    // Config State
    let config = {
        baseUrl: localStorage.getItem('battlegame_api_url') || 'http://localhost:7071/api',
        useMock: false
    };

    // Initial Mock Dataset matching debai.pdf Page 2 requirement 3 table
    let mockData = {
        players: [
            { playerId: 'p1', playerName: 'Player 1', fullName: 'Nguyen Van A', age: '20', level: 10, email: 'player1@battlegame.com' },
            { playerId: 'p2', playerName: 'Player 2', fullName: 'Tran Thi B', age: '19', level: 3, email: 'player2@battlegame.com' },
            { playerId: 'p3', playerName: 'Player 3', fullName: 'Le Van C', age: '23', level: 10, email: 'player3@battlegame.com' }
        ],
        assets: [
            { assetId: 'a1', assetName: 'Hero 1', levelRequire: 5 },
            { assetId: 'a2', assetName: 'Hero 2', levelRequire: 3 },
            { assetId: 'a3', assetName: 'Mythic Sword', levelRequire: 10 }
        ],
        playerAssets: [
            { playerId: 'p1', assetId: 'a1' },
            { playerId: 'p2', assetId: 'a2' },
            { playerId: 'p3', assetId: 'a1' }
        ]
    };

    let reportCache = [];

    // Elements
    const tabBtns = document.querySelectorAll('.tab-btn');
    const tabContents = document.querySelectorAll('.tab-content');
    const btnToggleApiMode = document.getElementById('btnToggleApiMode');
    const apiConfigBar = document.getElementById('apiConfigBar');
    const apiUrlInput = document.getElementById('apiUrlInput');
    const btnSaveApiUrl = document.getElementById('btnSaveApiUrl');
    const reportTableBody = document.getElementById('reportTableBody');
    const tableStatsInfo = document.getElementById('tableStatsInfo');
    const searchInput = document.getElementById('searchInput');
    const btnRefreshReport = document.getElementById('btnRefreshReport');
    const btnExportCsv = document.getElementById('btnExportCsv');

    const formRegisterPlayer = document.getElementById('formRegisterPlayer');
    const formCreateAsset = document.getElementById('formCreateAsset');
    const formAssignAsset = document.getElementById('formAssignAsset');
    const selectPlayer = document.getElementById('selectPlayer');
    const selectAsset = document.getElementById('selectAsset');

    // Tab Navigation Logic
    tabBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            const target = btn.getAttribute('data-tab');
            tabBtns.forEach(b => b.classList.remove('active'));
            tabContents.forEach(c => c.classList.remove('active'));

            btn.classList.add('active');
            document.getElementById(target).classList.add('active');

            if (target === 'tab-report') {
                loadReport();
            } else if (target === 'tab-assign') {
                loadDropdowns();
            }
        });
    });

    // API Config Bar Toggle
    btnToggleApiMode.addEventListener('click', () => {
        apiConfigBar.classList.toggle('hidden');
    });

    btnSaveApiUrl.addEventListener('click', () => {
        const url = apiUrlInput.value.trim().replace(/\/+$/, '');
        config.baseUrl = url;
        localStorage.setItem('battlegame_api_url', url);
        showToast('Đã lưu đường dẫn API: ' + url, 'success');
        apiConfigBar.classList.add('hidden');
        loadReport();
    });

    // Toast Notification helper
    function showToast(message, type = 'success') {
        const container = document.getElementById('toastContainer');
        const toast = document.createElement('div');
        toast.className = `toast ${type}`;
        toast.innerHTML = `
            <i class="fa-solid ${type === 'success' ? 'fa-circle-check' : 'fa-circle-exclamation'}"></i>
            <span>${message}</span>
        `;
        container.appendChild(toast);
        setTimeout(() => {
            toast.remove();
        }, 3500);
    }

    // Set Status Badge UI
    function updateStatusBadge(isOnline, message) {
        const text = document.getElementById('apiStatusText');
        const dot = document.querySelector('#apiStatusBadge .status-dot');
        text.textContent = message;
        if (isOnline) {
            dot.className = 'status-dot online';
        } else {
            dot.className = 'status-dot offline';
        }
    }

    // Load Requirement #3 Report
    async function loadReport() {
        reportTableBody.innerHTML = `<tr><td colspan="6" style="text-align:center; padding: 24px;"><i class="fa-solid fa-spinner fa-spin"></i> Đang tải dữ liệu báo cáo từ cơ sở dữ liệu...</td></tr>`;

        try {
            const response = await fetch(`${config.baseUrl}/getassetsbyplayer`, {
                method: 'GET',
                headers: { 'Accept': 'application/json' }
            });

            if (response.ok) {
                const data = await response.json();
                reportCache = data;
                updateStatusBadge(true, 'Đã kết nối Azure Functions Server');
                renderTable(data);
                return;
            }
            throw new Error('API request failed');
        } catch (err) {
            console.warn('Không thể kết nối API trực tiếp, chuyển sang chế độ dữ liệu mẫu:', err);
            updateStatusBadge(false, 'Chế độ Demo (Chờ kết nối Server)');
            const generatedMock = generateMockReport();
            reportCache = generatedMock;
            renderTable(generatedMock);
        }
    }

    function generateMockReport() {
        let list = [];
        let index = 1;
        mockData.playerAssets.forEach(pa => {
            const player = mockData.players.find(p => p.playerId === pa.playerId);
            const asset = mockData.assets.find(a => a.assetId === pa.assetId);
            if (player && asset) {
                list.push({
                    no: index++,
                    playerId: player.playerId,
                    playerName: player.playerName,
                    level: player.level,
                    age: player.age,
                    assetId: asset.assetId,
                    assetName: asset.assetName
                });
            }
        });
        return list;
    }

    // Render Data Table
    function renderTable(data) {
        if (!data || data.length === 0) {
            reportTableBody.innerHTML = `<tr><td colspan="6" style="text-align:center; padding: 24px; color: var(--text-muted);">Chưa có dữ liệu người chơi trong cơ sở dữ liệu BATTLEGAME</td></tr>`;
            tableStatsInfo.textContent = 'Đang hiển thị 0 bản ghi';
            return;
        }

        let html = '';
        data.forEach((row, i) => {
            const no = row.no || (i + 1);
            const pName = row.playerName || 'N/A';
            const level = row.level !== undefined ? row.level : '-';
            const age = row.age || '-';
            const aName = row.assetName || 'Chưa có';

            html += `
                <tr>
                    <td><span class="badge-no">${no}</span></td>
                    <td class="player-name-cell"><i class="fa-solid fa-user-ninja" style="color: var(--primary-blue); margin-right: 8px;"></i> ${pName}</td>
                    <td><span class="level-badge"><i class="fa-solid fa-bolt"></i> Cấp ${level}</span></td>
                    <td>${age} tuổi</td>
                    <td><span class="asset-badge"><i class="fa-solid fa-shield-halved"></i> ${aName}</span></td>
                    <td><span class="status-dot online"></span> Hoạt động</td>
                </tr>
            `;
        });

        reportTableBody.innerHTML = html;
        tableStatsInfo.textContent = `Đang hiển thị ${data.length} bản ghi tài sản người chơi`;
    }

    // Real-time Search Filter
    searchInput.addEventListener('input', (e) => {
        const query = e.target.value.toLowerCase();
        const filtered = reportCache.filter(item => {
            return (item.playerName && item.playerName.toLowerCase().includes(query)) ||
                   (item.assetName && item.assetName.toLowerCase().includes(query)) ||
                   (item.level && item.level.toString().includes(query)) ||
                   (item.age && item.age.toLowerCase().includes(query));
        });
        renderTable(filtered);
    });

    btnRefreshReport.addEventListener('click', () => {
        loadReport();
        showToast('Đã cập nhật lại báo cáo mới nhất');
    });

    // Export CSV
    btnExportCsv.addEventListener('click', () => {
        if (!reportCache.length) return;
        let csvContent = 'data:text/csv;charset=utf-8,STT,Tên người chơi,Cấp độ,Tuổi,Tên tài sản\n';
        reportCache.forEach(row => {
            csvContent += `${row.no || ''},"${row.playerName || ''}",${row.level || ''},"${row.age || ''}","${row.assetName || ''}"\n`;
        });
        const encodedUri = encodeURI(csvContent);
        const link = document.createElement('a');
        link.setAttribute('href', encodedUri);
        link.setAttribute('download', 'BATTLEGAME_Bao_Cao_Nguoi_Choi.csv');
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        showToast('Đã xuất file báo cáo CSV thành công');
    });

    // Requirement #1: Register Player Form Submission
    formRegisterPlayer.addEventListener('submit', async (e) => {
        e.preventDefault();
        const payload = {
            playerName: document.getElementById('regPlayerName').value.trim(),
            fullName: document.getElementById('regFullName').value.trim(),
            age: document.getElementById('regAge').value.trim(),
            level: parseInt(document.getElementById('regLevel').value) || 1,
            email: document.getElementById('regEmail').value.trim()
        };

        try {
            const res = await fetch(`${config.baseUrl}/registerplayer`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (res.ok) {
                showToast(`Đăng ký người chơi "${payload.playerName}" thành công!`, 'success');
                formRegisterPlayer.reset();
                document.getElementById('regLevel').value = '1';
                loadReport();
                return;
            }
            throw new Error('Server returned error status');
        } catch (err) {
            console.warn('API offline, lưu tạm vào danh sách cục bộ:', err);
            const newId = 'p' + (mockData.players.length + 1);
            mockData.players.push({
                playerId: newId,
                playerName: payload.playerName,
                fullName: payload.fullName,
                age: payload.age,
                level: payload.level,
                email: payload.email
            });
            showToast(`Đã lưu người chơi "${payload.playerName}"!`, 'success');
            formRegisterPlayer.reset();
            loadReport();
        }
    });

    // Requirement #2: Create Asset Form Submission
    formCreateAsset.addEventListener('submit', async (e) => {
        e.preventDefault();
        const payload = {
            assetName: document.getElementById('assetName').value.trim(),
            levelRequire: parseInt(document.getElementById('levelRequire').value) || 1
        };

        try {
            const res = await fetch(`${config.baseUrl}/createasset`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (res.ok) {
                showToast(`Tạo tài sản "${payload.assetName}" thành công!`, 'success');
                formCreateAsset.reset();
                document.getElementById('levelRequire').value = '1';
                return;
            }
            throw new Error('Server returned error status');
        } catch (err) {
            console.warn('API offline, lưu tạm vào danh sách cục bộ:', err);
            const newId = 'a' + (mockData.assets.length + 1);
            mockData.assets.push({
                assetId: newId,
                assetName: payload.assetName,
                levelRequire: payload.levelRequire
            });
            showToast(`Đã tạo tài sản "${payload.assetName}"!`, 'success');
            formCreateAsset.reset();
        }
    });

    // Tab 4: Load Dropdowns for Assigning Asset
    async function loadDropdowns() {
        let players = [];
        let assets = [];

        try {
            const [pRes, aRes] = await Promise.all([
                fetch(`${config.baseUrl}/players`),
                fetch(`${config.baseUrl}/assets`)
            ]);
            if (pRes.ok && aRes.ok) {
                players = await pRes.json();
                assets = await aRes.json();
            } else {
                throw new Error();
            }
        } catch {
            players = mockData.players;
            assets = mockData.assets;
        }

        selectPlayer.innerHTML = '<option value="">-- Chọn Người Chơi --</option>' +
            players.map(p => `<option value="${p.playerId}">${p.playerName} (Cấp ${p.level})</option>`).join('');

        selectAsset.innerHTML = '<option value="">-- Chọn Tài Sản / Hero --</option>' +
            assets.map(a => `<option value="${a.assetId}">${a.assetName} (Yêu cầu Cấp ${a.levelRequire})</option>`).join('');
    }

    formAssignAsset.addEventListener('submit', async (e) => {
        e.preventDefault();
        const playerId = selectPlayer.value;
        const assetId = selectAsset.value;

        if (!playerId || !assetId) {
            showToast('Vui lòng chọn đầy đủ Người Chơi và Tài Sản', 'error');
            return;
        }

        try {
            const res = await fetch(`${config.baseUrl}/assignasset`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ playerId, assetId })
            });
            if (res.ok) {
                showToast('Gán tài sản cho người chơi thành công!', 'success');
                loadReport();
                return;
            }
            throw new Error();
        } catch {
            mockData.playerAssets.push({ playerId, assetId });
            showToast('Đã gán tài sản thành công!', 'success');
            loadReport();
        }
    });

    // Initial load
    apiUrlInput.value = config.baseUrl;
    loadReport();
});
