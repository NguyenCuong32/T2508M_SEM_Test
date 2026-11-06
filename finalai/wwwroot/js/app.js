document.addEventListener('DOMContentLoaded', () => {
  // Initialize default report
  loadReport();

  // Event Listeners for Report Filter
  document.getElementById('btnFilter').addEventListener('click', () => {
    const from = document.getElementById('fromDate').value;
    const to = document.getElementById('toDate').value;
    loadReport(from, to);
  });

  document.getElementById('btnResetFilter').addEventListener('click', () => {
    document.getElementById('fromDate').value = '';
    document.getElementById('toDate').value = '';
    loadReport();
  });

  document.getElementById('btnRefreshReport').addEventListener('click', () => {
    const from = document.getElementById('fromDate').value;
    const to = document.getElementById('toDate').value;
    loadReport(from, to);
  });

  // Ask Hotel Submit
  document.getElementById('btnAskSubmit').addEventListener('click', handleAskHotel);
  document.getElementById('askQuestionInput').addEventListener('keydown', (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleAskHotel();
    }
  });

  // Parse Booking Submit
  document.getElementById('btnParseSubmit').addEventListener('click', handleParseBooking);

  // Assistant Chat Submit
  document.getElementById('chatForm').addEventListener('submit', (e) => {
    e.preventDefault();
    handleAssistantChat();
  });

  document.getElementById('btnClearChat').addEventListener('click', () => {
    chatHistory = [];
    document.getElementById('chatContainer').innerHTML = `
      <div class="chat-bubble chat-bubble-ai">
        Lịch sử trò chuyện đã được làm mới. Quý khách cần hỗ trợ thêm thông tin gì về khách sạn Sunrise Hotel?
      </div>
    `;
    document.getElementById('functionLogsContainer').innerHTML = `
      <div class="text-muted small fst-italic p-2">Chưa có lệnh gọi hàm nào được thực thi.</div>
    `;
  });
});

let chatHistory = [];

// ==========================================
// 1. REPORT API & FRONTEND (REQ 4 & 5)
// ==========================================
async function loadReport(from = '', to = '') {
  const tbody = document.getElementById('reportTableBody');
  tbody.innerHTML = `
    <tr>
      <td colspan="9" class="text-center py-4 text-muted">
        <div class="spinner-border text-primary spinner-border-sm me-2" role="status"></div>
        Đang tải dữ liệu báo cáo...
      </td>
    </tr>
  `;

  let url = '/api/reports/bookings';
  const params = [];
  if (from) params.push(`from=${encodeURIComponent(from)}`);
  if (to) params.push(`to=${encodeURIComponent(to)}`);
  if (params.length > 0) url += `?${params.join('&')}`;

  try {
    const res = await fetch(url);
    if (!res.ok) {
      const err = await res.json();
      throw new Error(err.error || `HTTP ${res.status}`);
    }

    const data = await res.json();
    renderReportTable(data);
    updateReportStats(data);
  } catch (error) {
    tbody.innerHTML = `
      <tr>
        <td colspan="9" class="text-center py-4 text-danger">
          <i class="bi bi-exclamation-triangle-fill me-2"></i> Lỗi tải báo cáo: ${error.message}
        </td>
      </tr>
    `;
  }
}

function renderReportTable(items) {
  const tbody = document.getElementById('reportTableBody');
  if (!items || items.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="9" class="text-center py-4 text-muted">
          <i class="bi bi-inbox fs-4 d-block mb-1"></i>
          Không tìm thấy đơn đặt phòng nào trong khoảng thời gian này.
        </td>
      </tr>
    `;
    return;
  }

  tbody.innerHTML = items.map(item => `
    <tr>
      <td class="text-center fw-bold text-secondary">${item.no}</td>
      <td class="fw-semibold text-primary">#${item.roomNumber}</td>
      <td><span class="badge bg-light text-dark border">${item.roomType}</span></td>
      <td class="fw-medium">${escapeHtml(item.guestName)}</td>
      <td><i class="bi bi-calendar-event text-muted me-1"></i>${item.checkIn}</td>
      <td><i class="bi bi-calendar-check text-muted me-1"></i>${item.checkOut}</td>
      <td class="text-center fw-semibold">${item.nights}</td>
      <td class="text-end fw-bold text-dark">${formatVND(item.totalPrice)}</td>
      <td class="text-center">
        <span class="${item.status === 'Confirmed' ? 'badge-confirmed' : 'badge-cancelled'}">
          ${item.status}
        </span>
      </td>
    </tr>
  `).join('');
}

function updateReportStats(items) {
  let totalBookings = 0;
  let totalNights = 0;
  let totalRevenue = 0;

  if (items && items.length > 0) {
    items.forEach(i => {
      totalBookings++;
      totalNights += (i.nights || 0);
      totalRevenue += (i.totalPrice || 0);
    });
  }

  document.getElementById('stat-total-bookings').textContent = totalBookings;
  document.getElementById('stat-total-nights').textContent = totalNights;
  document.getElementById('stat-total-revenue').textContent = formatVND(totalRevenue);
}

// ==========================================
// 2. ASK HOTEL (REQ 1)
// ==========================================
function setAskQuestion(q) {
  document.getElementById('askQuestionInput').value = q;
  document.getElementById('askQuestionInput').focus();
}

async function handleAskHotel() {
  const input = document.getElementById('askQuestionInput');
  const question = input.value.trim();
  const box = document.getElementById('askResponseBox');
  const btn = document.getElementById('btnAskSubmit');

  if (!question) {
    alert('Vui lòng nhập câu hỏi cho lễ tân.');
    return;
  }

  btn.disabled = true;
  btn.innerHTML = `<span class="spinner-border spinner-border-sm me-1"></span> Đang gửi câu hỏi...`;
  box.innerHTML = `<div class="text-muted"><span class="spinner-border spinner-border-sm text-primary me-2"></span>Lễ tân Sunrise Hotel đang soạn câu trả lời...</div>`;

  try {
    const res = await fetch('/api/ai/ask', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ question })
    });

    const data = await res.json();
    if (!res.ok) {
      throw new Error(data.error || `HTTP ${res.status}`);
    }

    box.innerHTML = `
      <div class="fw-semibold text-primary mb-2">
        <i class="bi bi-person-check-fill me-1"></i> Lễ tân Sunrise Hotel:
      </div>
      <div class="lh-base" style="white-space: pre-wrap;">${escapeHtml(data.answer)}</div>
    `;
  } catch (error) {
    box.innerHTML = `
      <div class="text-danger">
        <i class="bi bi-exclamation-circle me-1"></i> Lỗi: ${escapeHtml(error.message)}
      </div>
    `;
  } finally {
    btn.disabled = false;
    btn.innerHTML = `<i class="bi bi-send-fill me-1"></i> Gửi câu hỏi đến Lễ tân`;
  }
}

// ==========================================
// 3. PARSE BOOKING (REQ 2)
// ==========================================
function setParseText(t) {
  document.getElementById('parseTextInput').value = t;
  document.getElementById('parseTextInput').focus();
}

async function handleParseBooking() {
  const input = document.getElementById('parseTextInput');
  const text = input.value.trim();
  const jsonViewer = document.getElementById('jsonExtractedViewer');
  const resultCard = document.getElementById('bookingResultCard');
  const btn = document.getElementById('btnParseSubmit');

  if (!text) {
    alert('Vui lòng nhập câu yêu cầu đặt phòng.');
    return;
  }

  btn.disabled = true;
  btn.innerHTML = `<span class="spinner-border spinner-border-sm me-1"></span> Gemini đang phân tích và đặt phòng...`;
  jsonViewer.textContent = '// Đang gửi yêu cầu đến Gemini AI...';
  resultCard.innerHTML = `
    <div class="text-muted"><span class="spinner-border spinner-border-sm text-primary me-2"></span>Đang kiểm tra phòng trống và lưu đơn vào database...</div>
  `;

  try {
    const res = await fetch('/api/bookings/parse', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ text })
    });

    const data = await res.json();

    if (data.extractedData) {
      jsonViewer.textContent = JSON.stringify(data.extractedData, null, 2);
    }

    if (!res.ok || !data.success) {
      resultCard.innerHTML = `
        <div class="alert alert-danger mb-0">
          <i class="bi bi-x-circle-fill me-2"></i> ${escapeHtml(data.message || data.error || 'Đặt phòng không thành công')}
        </div>
      `;
      return;
    }

    resultCard.innerHTML = `
      <div class="alert alert-success mb-3">
        <i class="bi bi-check-circle-fill me-2"></i> ${escapeHtml(data.message)}
      </div>
      <table class="table table-sm table-bordered">
        <tr><th>Mã đặt phòng:</th><td class="fw-bold text-primary">#${data.bookingId}</td></tr>
        <tr><th>Số phòng:</th><td class="fw-bold">${data.roomNumber} (${data.roomType})</td></tr>
        <tr><th>Khách hàng:</th><td>${escapeHtml(data.guestName)}</td></tr>
        <tr><th>Điện thoại:</th><td>${escapeHtml(data.phone)}</td></tr>
        <tr><th>Thời gian:</th><td>${data.checkIn} ➔ ${data.checkOut} (${data.nights} đêm)</td></tr>
        <tr><th>Giá phòng/đêm:</th><td>${formatVND(data.pricePerNight)}</td></tr>
        <tr><th>Tổng thanh toán:</th><td class="fw-bold text-success fs-6">${formatVND(data.totalPrice)}</td></tr>
        <tr><th>Trạng thái:</th><td><span class="badge-confirmed">${data.status}</span></td></tr>
      </table>
    `;

    // Refresh report table so new booking is visible
    loadReport();
  } catch (error) {
    resultCard.innerHTML = `
      <div class="alert alert-danger mb-0">
        <i class="bi bi-exclamation-triangle-fill me-2"></i> Lỗi: ${escapeHtml(error.message)}
      </div>
    `;
    jsonViewer.textContent = `// Lỗi: ${error.message}`;
  } finally {
    btn.disabled = false;
    btn.innerHTML = `<i class="bi bi-lightning-charge-fill me-1"></i> Xử lý & Tạo Đặt Phòng`;
  }
}

// ==========================================
// 4. ASSISTANT CHAT WITH FUNCTION CALLING (REQ 3)
// ==========================================
function sendAssistantSample(msg) {
  document.getElementById('chatMessageInput').value = msg;
  handleAssistantChat();
}

async function handleAssistantChat() {
  const input = document.getElementById('chatMessageInput');
  const message = input.value.trim();
  const container = document.getElementById('chatContainer');
  const logsContainer = document.getElementById('functionLogsContainer');
  const btn = document.getElementById('btnSendChat');

  if (!message) return;

  // Append user message
  appendUserMessage(message);
  input.value = '';

  // Append loading AI bubble
  const loadingId = 'loading-' + Date.now();
  const loadingBubble = document.createElement('div');
  loadingBubble.className = 'chat-bubble chat-bubble-ai';
  loadingBubble.id = loadingId;
  loadingBubble.innerHTML = `<span class="spinner-border spinner-border-sm text-primary me-2"></span>Trợ lý đang suy nghĩ và kiểm tra hệ thống...`;
  container.appendChild(loadingBubble);
  container.scrollTop = container.scrollHeight;

  btn.disabled = true;

  try {
    const res = await fetch('/api/assistant/chat', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        message: message,
        history: chatHistory
      })
    });

    const data = await res.json();
    if (!res.ok) {
      throw new Error(data.error || `HTTP ${res.status}`);
    }

    // Save history
    chatHistory.push({ role: 'user', content: message });
    chatHistory.push({ role: 'model', content: data.reply });

    // Render function badges if executed
    let badgesHtml = '';
    if (data.executedFunctions && data.executedFunctions.length > 0) {
      data.executedFunctions.forEach(fn => {
        badgesHtml += `
          <div>
            <span class="function-badge">
              <i class="bi bi-lightning-fill"></i> Executed: ${fn.functionName}(${JSON.stringify(fn.arguments)})
            </span>
          </div>
        `;

        // Also append to side logs
        const logItem = document.createElement('div');
        logItem.className = 'p-2 mb-2 bg-light rounded border small';
        logItem.innerHTML = `
          <div class="fw-bold text-primary">${fn.functionName}</div>
          <div class="text-muted" style="font-size: 0.75rem;">Args: ${escapeHtml(JSON.stringify(fn.arguments))}</div>
          <div class="text-success" style="font-size: 0.75rem;">Result: ${escapeHtml(JSON.stringify(fn.result))}</div>
        `;
        if (logsContainer.children[0]?.classList.contains('fst-italic')) {
          logsContainer.innerHTML = '';
        }
        logsContainer.prepend(logItem);

        // If cancel booking or booking changed, refresh report
        if (fn.functionName === 'CancelBooking') {
          loadReport();
        }
      });
    }

    const aiBubble = document.getElementById(loadingId);
    if (aiBubble) {
      aiBubble.innerHTML = `
        ${badgesHtml}
        <div style="white-space: pre-wrap;">${escapeHtml(data.reply)}</div>
      `;
    }
  } catch (error) {
    const aiBubble = document.getElementById(loadingId);
    if (aiBubble) {
      aiBubble.className = 'chat-bubble chat-bubble-ai border-danger text-danger';
      aiBubble.innerHTML = `<i class="bi bi-exclamation-triangle-fill me-1"></i> Lỗi: ${escapeHtml(error.message)}`;
    }
  } finally {
    btn.disabled = false;
    container.scrollTop = container.scrollHeight;
  }
}

function appendUserMessage(msg) {
  const container = document.getElementById('chatContainer');
  const bubble = document.createElement('div');
  bubble.className = 'chat-bubble chat-bubble-user';
  bubble.textContent = msg;
  container.appendChild(bubble);
  container.scrollTop = container.scrollHeight;
}

// Utility Helpers
function formatVND(value) {
  if (value === null || value === undefined) return '0 VND';
  return Number(value).toLocaleString('vi-VN') + ' VND';
}

function escapeHtml(text) {
  if (!text) return '';
  return String(text)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#039;');
}
