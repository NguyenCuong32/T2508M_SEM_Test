document.addEventListener("DOMContentLoaded", function () {
    initTabs();
    initSidebar();
    initQuickChips();
    loadReport();
    loadRooms();
    loadAllBookings();
    loadChatHistory();
    checkApiStatus();

    document.getElementById("btnFilter").addEventListener("click", function () {
        var from = document.getElementById("filterFrom").value;
        var to = document.getElementById("filterTo").value;
        loadReport(from, to);
    });

    document.getElementById("btnResetFilter").addEventListener("click", function () {
        document.getElementById("filterFrom").value = "";
        document.getElementById("filterTo").value = "";
        loadReport();
    });

    document.getElementById("btnAsk").addEventListener("click", handleAskHotel);
    document.getElementById("askInput").addEventListener("keypress", function (e) {
        if (e.key === "Enter") handleAskHotel();
    });

    document.getElementById("btnParse").addEventListener("click", handleParseBooking);

    document.getElementById("btnSendChat").addEventListener("click", handleSendChat);
    document.getElementById("chatInput").addEventListener("keypress", function (e) {
        if (e.key === "Enter") handleSendChat();
    });
    var btnClearChat = document.getElementById("btnClearChat");
    if (btnClearChat) {
        btnClearChat.addEventListener("click", handleClearChat);
    }

    document.getElementById("btnOpenConfig").addEventListener("click", openConfigModal);
    var sidebarConfigBtn = document.getElementById("btnOpenConfigSidebar");
    if (sidebarConfigBtn) {
        sidebarConfigBtn.addEventListener("click", function () {
            closeSidebar();
            openConfigModal();
        });
    }

    document.getElementById("btnCloseModal").addEventListener("click", closeConfigModal);
    document.getElementById("btnCancelModal").addEventListener("click", closeConfigModal);
    document.getElementById("btnSaveKey").addEventListener("click", handleSaveApiKey);
});

function initTabs() {
    var desktopBtns = document.querySelectorAll(".tab-btn");
    var sidebarBtns = document.querySelectorAll(".sidebar-nav-item");

    function switchTab(targetId) {
        desktopBtns.forEach(function (b) {
            if (b.getAttribute("data-tab") === targetId) {
                b.classList.add("active");
            } else {
                b.classList.remove("active");
            }
        });

        sidebarBtns.forEach(function (sb) {
            if (sb.getAttribute("data-tab") === targetId) {
                sb.classList.add("active");
            } else {
                sb.classList.remove("active");
            }
        });

        document.querySelectorAll(".tab-content").forEach(function (c) {
            if (c.id === targetId) {
                c.classList.add("active");
            } else {
                c.classList.remove("active");
            }
        });
    }

    desktopBtns.forEach(function (btn) {
        btn.addEventListener("click", function () {
            var targetId = btn.getAttribute("data-tab");
            switchTab(targetId);
        });
    });

    sidebarBtns.forEach(function (btn) {
        btn.addEventListener("click", function () {
            var targetId = btn.getAttribute("data-tab");
            switchTab(targetId);
            closeSidebar();
        });
    });
}

function initSidebar() {
    var toggleBtn = document.getElementById("btnToggleSidebar");
    var closeBtn = document.getElementById("btnCloseSidebar");
    var overlay = document.getElementById("sidebarOverlay");

    if (toggleBtn) {
        toggleBtn.addEventListener("click", function () {
            openSidebar();
        });
    }

    if (closeBtn) {
        closeBtn.addEventListener("click", function () {
            closeSidebar();
        });
    }

    if (overlay) {
        overlay.addEventListener("click", function () {
            closeSidebar();
        });
    }
}

function openSidebar() {
    var drawer = document.getElementById("sidebarDrawer");
    var overlay = document.getElementById("sidebarOverlay");
    if (drawer) drawer.classList.add("active");
    if (overlay) overlay.classList.add("active");
}

function closeSidebar() {
    var drawer = document.getElementById("sidebarDrawer");
    var overlay = document.getElementById("sidebarOverlay");
    if (drawer) drawer.classList.remove("active");
    if (overlay) overlay.classList.remove("active");
}

function openConfigModal() {
    document.getElementById("configModal").classList.add("show");
}

function closeConfigModal() {
    document.getElementById("configModal").classList.remove("show");
}

function initQuickChips() {
    document.querySelectorAll(".chip").forEach(function (chip) {
        chip.addEventListener("click", function () {
            var targetInputId = chip.getAttribute("data-target");
            var text = chip.getAttribute("data-text");
            var input = document.getElementById(targetInputId);
            if (input) {
                input.value = text;
                input.focus();
            }
        });
    });
}

function showToast(icon, title) {
    if (typeof Swal !== "undefined") {
        Swal.fire({
            toast: true,
            position: "top-end",
            icon: icon,
            title: title,
            showConfirmButton: false,
            timer: 2500,
            timerProgressBar: true
        });
    } else {
        alert(title);
    }
}

function showModalAlert(icon, title, text) {
    if (typeof Swal !== "undefined") {
        Swal.fire({
            icon: icon,
            title: title,
            text: text,
            confirmButtonColor: "#1a73e8",
            confirmButtonText: "Đóng"
        });
    } else {
        alert(title + ": " + text);
    }
}

function checkApiStatus() {
    fetch("/api/config/status")
        .then(function (res) { return res.json(); })
        .then(function (data) {
            updateApiStatusElements(data.apiKeyStatus);
        })
        .catch(function () {
            updateApiStatusElements("Không kết nối");
        });
}

function updateApiStatusElements(statusText) {
    var badge = document.getElementById("apiStatusBadge");
    var textSpan = document.getElementById("apiStatusText");
    var sideBadge = document.getElementById("sidebarApiStatusBadge");
    var sideTextSpan = document.getElementById("sidebarApiStatusText");
    var modalStatus = document.getElementById("modalKeyStatus");

    var isConfigured = statusText && !statusText.includes("Chưa") && !statusText.includes("Không");
    var cls = isConfigured ? "status-badge success" : "status-badge warning";
    var txt = isConfigured ? "Gemini: " + statusText : (statusText.includes("Không") ? "Gemini: Không kết nối" : "Gemini: Chưa có Key");

    if (badge && textSpan) {
        badge.className = cls;
        textSpan.innerText = txt;
    }
    if (sideBadge && sideTextSpan) {
        sideBadge.className = cls;
        sideTextSpan.innerText = txt;
    }
    if (modalStatus) {
        modalStatus.innerText = "Trạng thái hiện tại: " + statusText;
    }
}

function handleSaveApiKey() {
    var key = document.getElementById("apiKeyInput").value.trim();
    if (!key) {
        showToast("warning", "Vui lòng nhập API Key!");
        return;
    }

    fetch("/api/config/key", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ apiKey: key })
    })
        .then(function (res) { return res.json(); })
        .then(function (data) {
            closeConfigModal();
            showToast("success", data.message || "Lưu API Key thành công!");
            checkApiStatus();
        })
        .catch(function (err) {
            showModalAlert("error", "Lỗi cấu hình", "Lỗi khi lưu API Key: " + err);
        });
}

function loadReport(from, to) {
    var url = "/api/reports/bookings";
    var params = [];
    if (from) params.push("from=" + encodeURIComponent(from));
    if (to) params.push("to=" + encodeURIComponent(to));
    if (params.length > 0) url += "?" + params.join("&");

    var tbody = document.getElementById("reportBody");
    tbody.innerHTML = '<tr><td colspan="9" style="text-align: center; padding: 20px;">Đang tải dữ liệu...</td></tr>';

    fetch(url)
        .then(function (res) { return res.json(); })
        .then(function (data) {
            tbody.innerHTML = "";
            if (!Array.isArray(data) || data.length === 0) {
                tbody.innerHTML = '<tr><td colspan="9" style="text-align: center; padding: 20px; color: #5f6368;">Không có dữ liệu đặt phòng nào phù hợp với bộ lọc.</td></tr>';
                document.getElementById("reportCount").innerText = "0";
                document.getElementById("reportTotalRevenue").innerText = "0 đ";
                return;
            }

            var totalRev = 0;
            data.forEach(function (item) {
                totalRev += item.totalPrice;
                var tr = document.createElement("tr");
                tr.innerHTML =
                    '<td><strong>' + item.no + '</strong></td>' +
                    '<td><span style="font-weight: 600; color: #1a73e8;">' + item.roomNumber + '</span></td>' +
                    '<td>' + item.roomType + '</td>' +
                    '<td><strong>' + item.guestName + '</strong></td>' +
                    '<td>' + item.checkIn + '</td>' +
                    '<td>' + item.checkOut + '</td>' +
                    '<td style="text-align: center;">' + item.nights + '</td>' +
                    '<td class="money">' + item.formattedTotalPrice + ' đ</td>' +
                    '<td><span class="tag-confirmed">' + item.status + '</span></td>';
                tbody.appendChild(tr);
            });

            document.getElementById("reportCount").innerText = data.length;
            document.getElementById("reportTotalRevenue").innerText = totalRev.toLocaleString("vi-VN") + " đ";
        })
        .catch(function (err) {
            tbody.innerHTML = '<tr><td colspan="9" style="text-align: center; padding: 20px; color: #d93025;">Lỗi khi tải dữ liệu báo cáo: ' + err + '</td></tr>';
        });
}

function handleAskHotel() {
    var input = document.getElementById("askInput");
    var q = input.value.trim();
    if (!q) {
        showToast("warning", "Vui lòng nhập câu hỏi!");
        return;
    }

    var resultBox = document.getElementById("askResult");
    var btn = document.getElementById("btnAsk");
    btn.disabled = true;
    btn.innerText = "Đang xử lý...";
    resultBox.innerText = "Lễ tân Sunrise Hotel đang trả lời, vui lòng chờ trong giây lát...";

    fetch("/api/ai/ask", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ question: q })
    })
        .then(function (res) {
            return res.text().then(function (text) {
                var data = {};
                try { data = JSON.parse(text); } catch (e) { data = { message: text }; }
                return { status: res.status, ok: res.ok, data: data };
            });
        })
        .then(function (res) {
            btn.disabled = false;
            btn.innerText = "Gửi câu hỏi";
            if (!res.ok) {
                resultBox.innerText = "Thông báo: " + (res.data.message || "Đã có lỗi xảy ra");
                showToast("error", res.data.message || "Có lỗi xảy ra");
                return;
            }
            resultBox.innerText = res.data.answer;
            showToast("success", "Đã nhận câu trả lời!");
        })
        .catch(function (err) {
            btn.disabled = false;
            btn.innerText = "Gửi câu hỏi";
            resultBox.innerText = "Lỗi kết nối máy chủ: " + err;
            showModalAlert("error", "Lỗi kết nối", err.toString());
        });
}

function handleParseBooking() {
    var input = document.getElementById("parseInput");
    var text = input.value.trim();
    if (!text) {
        showToast("warning", "Vui lòng nhập câu đặt phòng tự nhiên!");
        return;
    }

    var btn = document.getElementById("btnParse");
    var resultCard = document.getElementById("parseResultCard");
    var jsonView = document.getElementById("parseJsonView");
    var bookingMsg = document.getElementById("parseBookingMsg");

    btn.disabled = true;
    btn.innerText = "Gemini đang phân tích & tạo booking...";
    resultCard.style.display = "block";
    jsonView.innerText = "Đang trích xuất JSON...";
    bookingMsg.innerText = "Đang kiểm tra phòng trống và lưu CSDL...";

    fetch("/api/bookings/parse", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ text: text })
    })
        .then(function (res) {
            return res.text().then(function (text) {
                var data = {};
                try { data = JSON.parse(text); } catch (e) { data = { message: text }; }
                return { status: res.status, ok: res.ok, data: data };
            });
        })
        .then(function (res) {
            btn.disabled = false;
            btn.innerText = "Phân tích & Tự động Đặt phòng";
            if (!res.ok) {
                jsonView.innerText = "{}";
                bookingMsg.innerHTML = '<span style="color: #c5221f;"><strong>Thất bại:</strong> ' + (res.data.message || "Không thể đặt phòng.") + '</span>';
                showModalAlert("error", "Đặt phòng không thành công", res.data.message || "Không thể đặt phòng.");
                return;
            }

            jsonView.innerText = JSON.stringify(res.data.extracted, null, 2);
            var b = res.data.booking;
            bookingMsg.innerHTML =
                '<div style="color: #137333; font-weight: 700; font-size: 15px; margin-bottom: 8px;">' + b.message + '</div>' +
                '<div style="font-size: 13px; line-height: 1.6; color: #3c4043;">' +
                'Khách hàng: <strong>' + b.guestName + '</strong> | SĐT: <strong>' + b.phone + '</strong><br>' +
                'Phòng: <strong>' + b.roomNumber + ' (' + b.roomType + ')</strong><br>' +
                'Thời gian: <strong>' + b.checkIn + '</strong> đến <strong>' + b.checkOut + '</strong> (' + b.nights + ' đêm)<br>' +
                'Tổng tiền: <strong style="color: #1a73e8; font-size: 15px;">' + b.formattedTotalPrice + '</strong>' +
                '</div>';

            showToast("success", "Đặt phòng thành công!");
            loadReport();
            loadAllBookings();
        })
        .catch(function (err) {
            btn.disabled = false;
            btn.innerText = "Phân tích & Tự động Đặt phòng";
            bookingMsg.innerText = "Lỗi kết nối máy chủ: " + err;
            showModalAlert("error", "Lỗi kết nối", err.toString());
        });
}

function handleSendChat() {
    var input = document.getElementById("chatInput");
    var msg = input.value.trim();
    if (!msg) return;

    var historyPayload = [];
    try {
        var rawHistory = localStorage.getItem("sunrise_hotel_chat_history");
        if (rawHistory) {
            var parsedHistory = JSON.parse(rawHistory);
            if (Array.isArray(parsedHistory)) {
                historyPayload = parsedHistory
                    .filter(function (m) { return m && m.text; })
                    .map(function (m) {
                        return {
                            role: m.role === "assistant" ? "model" : "user",
                            text: m.text
                        };
                    })
                    .slice(-10);
            }
        }
    } catch (e) { }

    appendChatMessage("user", msg);
    input.value = "";

    var btn = document.getElementById("btnSendChat");
    btn.disabled = true;

    fetch("/api/assistant/chat", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ message: msg, history: historyPayload })
    })
        .then(function (res) {
            return res.text().then(function (text) {
                var data = {};
                try { data = JSON.parse(text); } catch (e) { data = { message: text }; }
                return { status: res.status, ok: res.ok, data: data };
            });
        })
        .then(function (res) {
            btn.disabled = false;
            if (!res.ok) {
                var errMsg = (res.data && res.data.message) ? res.data.message : "Đã có lỗi xảy ra khi xử lý tin nhắn.";
                appendChatMessage("assistant", errMsg);
                showToast("error", "Lỗi xử lý yêu cầu");
                return;
            }

            appendChatMessage("assistant", res.data.response, res.data.toolCalled);

            if (res.data.toolCalled === "CancelBooking" || res.data.toolCalled === "CheckAvailability") {
                loadReport();
                loadAllBookings();
            }
        })
        .catch(function (err) {
            btn.disabled = false;
            appendChatMessage("assistant", "Không thể kết nối đến máy chủ. Vui lòng kiểm tra lại dịch vụ backend.");
            showToast("error", "Lỗi kết nối máy chủ");
        });
}

var CHAT_STORAGE_KEY = "sunrise_hotel_chat_history";
var DEFAULT_GREETING = "Xin kính chào Quý khách! Em là trợ lý ảo của Sunrise Hotel Hà Nội. Em có thể hỗ trợ Quý khách kiểm tra phòng trống, tra cứu đặt phòng qua số điện thoại hoặc hỗ trợ hủy đặt phòng. Em có thể giúp gì cho Quý khách hôm nay ạ?";

function loadChatHistory() {
    var container = document.getElementById("chatMessages");
    if (!container) return;
    var raw = localStorage.getItem(CHAT_STORAGE_KEY);
    if (!raw) {
        container.innerHTML = '<div class="chat-bubble assistant">' + escapeHtml(DEFAULT_GREETING) + '</div>';
        return;
    }
    try {
        var list = JSON.parse(raw);
        if (Array.isArray(list) && list.length > 0) {
            container.innerHTML = "";
            list.forEach(function (m) {
                renderChatMessageElement(container, m.role, m.text, m.toolCalled);
            });
            container.scrollTop = container.scrollHeight;
        } else {
            container.innerHTML = '<div class="chat-bubble assistant">' + escapeHtml(DEFAULT_GREETING) + '</div>';
        }
    } catch (e) {
        container.innerHTML = '<div class="chat-bubble assistant">' + escapeHtml(DEFAULT_GREETING) + '</div>';
    }
}

function saveChatMessageToStorage(role, text, toolCalled) {
    try {
        var raw = localStorage.getItem(CHAT_STORAGE_KEY);
        var list = raw ? JSON.parse(raw) : [];
        if (!Array.isArray(list)) list = [];
        if (list.length === 0) {
            list.push({ role: "assistant", text: DEFAULT_GREETING, toolCalled: null });
        }
        list.push({ role: role, text: text, toolCalled: toolCalled || null });
        localStorage.setItem(CHAT_STORAGE_KEY, JSON.stringify(list));
    } catch (e) { }
}

function renderChatMessageElement(container, role, text, toolCalled) {
    var bubble = document.createElement("div");
    bubble.className = "chat-bubble " + role;
    var contentHtml = "";
    if (toolCalled) {
        contentHtml += '<div style="font-size: 11px; color: #1a73e8; margin-bottom: 4px; font-weight: 600;">[Hàm C#: ' + escapeHtml(toolCalled) + ']</div>';
    }
    contentHtml += '<div>' + escapeHtml(text).replace(/\n/g, "<br>") + '</div>';
    bubble.innerHTML = contentHtml;
    container.appendChild(bubble);
}

function appendChatMessage(role, text, toolCalled, saveToStorage) {
    var container = document.getElementById("chatMessages");
    if (!container) return;
    renderChatMessageElement(container, role, text, toolCalled);
    container.scrollTop = container.scrollHeight;
    if (saveToStorage !== false) {
        saveChatMessageToStorage(role, text, toolCalled);
    }
}

function handleClearChat() {
    if (typeof Swal !== "undefined") {
        Swal.fire({
            title: "Xóa lịch sử trò chuyện?",
            text: "Toàn bộ tin nhắn trước đó sẽ bị xóa khỏi trình duyệt.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#d33",
            cancelButtonColor: "#5f6368",
            confirmButtonText: "Đồng ý xóa",
            cancelButtonText: "Hủy"
        }).then(function (result) {
            if (result.isConfirmed) {
                localStorage.removeItem(CHAT_STORAGE_KEY);
                loadChatHistory();
                showToast("success", "Đã xóa lịch sử trò chuyện.");
            }
        });
    } else {
        if (confirm("Bạn có chắc chắn muốn xóa lịch sử trò chuyện?")) {
            localStorage.removeItem(CHAT_STORAGE_KEY);
            loadChatHistory();
        }
    }
}

function loadRooms() {
    fetch("/api/rooms")
        .then(function (res) { return res.json(); })
        .then(function (rooms) {
            var grid = document.getElementById("roomsGrid");
            grid.innerHTML = "";
            rooms.forEach(function (r) {
                var card = document.createElement("div");
                card.className = "room-card";
                card.innerHTML =
                    '<div class="room-number">Phòng ' + r.roomNumber + '</div>' +
                    '<div class="room-type">' + r.roomType + '</div>' +
                    '<div class="room-price">' + r.pricePerNight.toLocaleString("vi-VN") + ' đ / đêm</div>' +
                    '<div class="room-info">Sức chứa: Tối đa ' + r.maxGuests + ' người' +
                    (r.roomType === "Suite" ? '<br><span style="color: #0f9d58; font-weight: 600;">Miễn phí đón sân bay Nội Bài</span>' : '') +
                    '</div>';
                grid.appendChild(card);
            });
        })
        .catch(function () { });
}

function loadAllBookings() {
    fetch("/api/bookings")
        .then(function (res) { return res.json(); })
        .then(function (bookings) {
            var tbody = document.getElementById("allBookingsBody");
            if (!tbody) return;
            tbody.innerHTML = "";
            if (!Array.isArray(bookings) || bookings.length === 0) {
                tbody.innerHTML = '<tr><td colspan="7" style="text-align: center; padding: 12px; color: #5f6368;">Chưa có booking nào.</td></tr>';
                return;
            }

            bookings.forEach(function (b) {
                var tr = document.createElement("tr");
                var tagClass = b.status === "Confirmed" ? "tag-confirmed" : "tag-cancelled";
                tr.innerHTML =
                    '<td>#' + b.bookingId + '</td>' +
                    '<td><strong>' + (b.room ? b.room.roomNumber : b.roomId) + '</strong> (' + (b.room ? b.room.roomType : "") + ')</td>' +
                    '<td><strong>' + b.guestName + '</strong></td>' +
                    '<td>' + b.phone + '</td>' +
                    '<td>' + b.checkIn + ' đến ' + b.checkOut + '</td>' +
                    '<td class="money">' + b.totalPrice.toLocaleString("vi-VN") + ' đ</td>' +
                    '<td><span class="' + tagClass + '">' + b.status + '</span></td>';
                tbody.appendChild(tr);
            });
        })
        .catch(function () { });
}

function escapeHtml(str) {
    if (!str) return "";
    return str
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}
