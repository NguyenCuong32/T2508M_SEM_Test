document.addEventListener("DOMContentLoaded", () => {
    // ----------------------------------------------------
    // TAB 1: BOOKING REPORT (Questions 4 & 5)
    // ----------------------------------------------------
    const reportFilterForm = document.getElementById("reportFilterForm");
    const filterFrom = document.getElementById("filterFrom");
    const filterTo = document.getElementById("filterTo");
    const btnResetFilter = document.getElementById("btnResetFilter");
    const reportTableBody = document.getElementById("reportTableBody");
    const statTotalBookings = document.getElementById("statTotalBookings");
    const statTotalNights = document.getElementById("statTotalNights");
    const statTotalRevenue = document.getElementById("statTotalRevenue");

    async function loadBookingReport(from = "", to = "") {
        reportTableBody.innerHTML = `
            <tr>
                <td colspan="8" class="text-center py-4 text-muted">
                    <div class="spinner-border spinner-border-sm text-primary me-2"></div>
                    Đang tải dữ liệu...
                </td>
            </tr>`;

        try {
            const params = new URLSearchParams();
            if (from) params.append("from", from);
            if (to) params.append("to", to);

            const url = `/api/reports/bookings${params.toString() ? "?" + params.toString() : ""}`;
            const res = await fetch(url);
            if (!res.ok) {
                const err = await res.json();
                throw new Error(err.message || "Lỗi tải báo cáo.");
            }

            const data = await res.json();
            renderReportTable(data);
        } catch (err) {
            reportTableBody.innerHTML = `
                <tr>
                    <td colspan="8" class="text-center text-danger py-4">
                        <i class="bi bi-exclamation-triangle"></i> ${err.message}
                    </td>
                </tr>`;
        }
    }

    function renderReportTable(items) {
        if (!items || items.length === 0) {
            reportTableBody.innerHTML = `
                <tr>
                    <td colspan="8" class="text-center text-muted py-4">
                        Không có dữ liệu đặt phòng nào phù hợp trong khoảng thời gian này.
                    </td>
                </tr>`;
            statTotalBookings.textContent = "0";
            statTotalNights.textContent = "0";
            statTotalRevenue.textContent = "0 VND";
            return;
        }

        let totalNights = 0;
        let totalRevenue = 0;

        const rowsHtml = items.map((item, idx) => {
            totalNights += item.nights;
            totalRevenue += item.totalPrice;

            return `
                <tr class="text-center">
                    <td class="fw-bold">${item.no || idx + 1}</td>
                    <td><span class="badge bg-secondary fs-6">${escapeHtml(item.roomNumber)}</span></td>
                    <td><span class="badge ${getRoomTypeBadgeClass(item.roomType)}">${escapeHtml(item.roomType)}</span></td>
                    <td class="text-start fw-semibold text-dark">${escapeHtml(item.guestName)}</td>
                    <td>${escapeHtml(item.checkIn)}</td>
                    <td>${escapeHtml(item.checkOut)}</td>
                    <td class="fw-bold">${item.nights}</td>
                    <td class="text-end fw-bold text-success">${formatCurrency(item.totalPrice)}</td>
                </tr>`;
        }).join("");

        reportTableBody.innerHTML = rowsHtml;
        statTotalBookings.textContent = items.length;
        statTotalNights.textContent = totalNights;
        statTotalRevenue.textContent = formatCurrency(totalRevenue);
    }

    function getRoomTypeBadgeClass(type) {
        if (!type) return "bg-secondary";
        const t = type.toLowerCase();
        if (t === "single") return "bg-info text-dark";
        if (t === "double") return "bg-primary";
        if (t === "suite") return "bg-warning text-dark";
        return "bg-secondary";
    }

    reportFilterForm.addEventListener("submit", (e) => {
        e.preventDefault();
        loadBookingReport(filterFrom.value, filterTo.value);
    });

    btnResetFilter.addEventListener("click", () => {
        filterFrom.value = "";
        filterTo.value = "";
        loadBookingReport();
    });

    // Initial load
    loadBookingReport();

    // ----------------------------------------------------
    // TAB 2: ASK HOTEL (Question 1)
    // ----------------------------------------------------
    const askForm = document.getElementById("askForm");
    const askQuestionInput = document.getElementById("askQuestionInput");
    const btnSendAsk = document.getElementById("btnSendAsk");
    const askResultArea = document.getElementById("askResultArea");
    const askAnswerContent = document.getElementById("askAnswerContent");

    document.querySelectorAll(".sample-ask-btn").forEach(btn => {
        btn.addEventListener("click", () => {
            askQuestionInput.value = btn.getAttribute("data-q");
            askForm.dispatchEvent(new Event("submit"));
        });
    });

    askForm.addEventListener("submit", async (e) => {
        e.preventDefault();
        const question = askQuestionInput.value.trim();
        if (!question) return;

        btnSendAsk.disabled = true;
        btnSendAsk.innerHTML = `<span class="spinner-border spinner-border-sm"></span> Đang trả lời...`;
        askResultArea.classList.remove("d-none");
        askAnswerContent.innerHTML = `<div class="text-muted"><span class="spinner-border spinner-border-sm me-2"></span>Lễ tân đang suy nghĩ câu trả lời...</div>`;

        try {
            const res = await fetch("/api/ai/ask", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ question })
            });

            const data = await res.json();
            if (!res.ok) {
                throw new Error(data.message || "Lỗi gọi API hỏi đáp lễ tân.");
            }

            askAnswerContent.innerHTML = `<div style="white-space: pre-wrap;">${escapeHtml(data.answer)}</div>`;
        } catch (err) {
            askAnswerContent.innerHTML = `<div class="text-danger"><i class="bi bi-exclamation-triangle"></i> ${err.message}</div>`;
        } finally {
            btnSendAsk.disabled = false;
            btnSendAsk.innerHTML = `<i class="bi bi-send-fill"></i> Gửi hỏi`;
        }
    });

    // ----------------------------------------------------
    // TAB 3: PARSE BOOKING (Question 2)
    // ----------------------------------------------------
    const parseForm = document.getElementById("parseForm");
    const parseTextInput = document.getElementById("parseTextInput");
    const btnSubmitParse = document.getElementById("btnSubmitParse");
    const parseResultArea = document.getElementById("parseResultArea");
    const parseJsonViewer = document.getElementById("parseJsonViewer");
    const bookingCardBody = document.getElementById("bookingCardBody");
    const bookingCardTitle = document.getElementById("bookingCardTitle");

    document.querySelectorAll(".sample-parse-btn").forEach(btn => {
        btn.addEventListener("click", () => {
            parseTextInput.value = btn.getAttribute("data-text");
            parseForm.dispatchEvent(new Event("submit"));
        });
    });

    parseForm.addEventListener("submit", async (e) => {
        e.preventDefault();
        const text = parseTextInput.value.trim();
        if (!text) return;

        btnSubmitParse.disabled = true;
        btnSubmitParse.innerHTML = `<span class="spinner-border spinner-border-sm"></span> Đang xử lý...`;
        parseResultArea.classList.remove("d-none");
        parseJsonViewer.textContent = "Đang trích xuất JSON bằng Gemini AI...";
        bookingCardBody.innerHTML = `<div class="text-muted"><span class="spinner-border spinner-border-sm me-2"></span>Đang xử lý tạo đơn trong C#...</div>`;

        try {
            const res = await fetch("/api/bookings/parse", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ text })
            });

            const data = await res.json();

            if (data.parsedData) {
                parseJsonViewer.textContent = JSON.stringify(data.parsedData, null, 2);
            } else {
                parseJsonViewer.textContent = "Không trích xuất được dữ liệu JSON.";
            }

            if (!res.ok || !data.success) {
                bookingCardTitle.innerHTML = `<i class="bi bi-x-circle-fill text-danger"></i> Đặt Phòng Thất Bại`;
                bookingCardBody.innerHTML = `
                    <div class="alert alert-danger mb-0">
                        <strong>Lỗi:</strong> ${escapeHtml(data.message || "Không thể đặt phòng.")}
                    </div>`;
            } else {
                bookingCardTitle.innerHTML = `<i class="bi bi-check-circle-fill text-success"></i> Đặt Phòng Thành Công (Confirmed)`;
                const b = data.booking;
                bookingCardBody.innerHTML = `
                    <div class="alert alert-success py-2 mb-3">
                        <i class="bi bi-check-lg"></i> ${escapeHtml(data.message)}
                    </div>
                    <ul class="list-group list-group-flush">
                        <li class="list-group-item d-flex justify-content-between"><span>Mã Booking:</span> <strong>#${b.bookingId}</strong></li>
                        <li class="list-group-item d-flex justify-content-between"><span>Khách hàng:</span> <strong>${escapeHtml(b.guestName)}</strong></li>
                        <li class="list-group-item d-flex justify-content-between"><span>Số điện thoại:</span> <strong>${escapeHtml(b.phone)}</strong></li>
                        <li class="list-group-item d-flex justify-content-between"><span>Phòng xếp:</span> <strong>Phòng ${escapeHtml(b.roomNumber)} (${escapeHtml(b.roomType)})</strong></li>
                        <li class="list-group-item d-flex justify-content-between"><span>Thời gian:</span> <strong>${escapeHtml(b.checkIn)} &rarr; ${escapeHtml(b.checkOut)} (${b.nights} đêm)</strong></li>
                        <li class="list-group-item d-flex justify-content-between text-success fs-5"><span>Tổng tiền:</span> <strong>${formatCurrency(b.totalPrice)}</strong></li>
                        <li class="list-group-item d-flex justify-content-between"><span>Trạng thái:</span> <span class="badge bg-success">${escapeHtml(b.status)}</span></li>
                    </ul>`;

                // Reload report table so new booking shows up automatically!
                loadBookingReport(filterFrom.value, filterTo.value);
            }
        } catch (err) {
            parseJsonViewer.textContent = "Lỗi kết nối hoặc xử lý.";
            bookingCardTitle.innerHTML = `<i class="bi bi-x-circle-fill text-danger"></i> Lỗi`;
            bookingCardBody.innerHTML = `<div class="alert alert-danger mb-0">${err.message}</div>`;
        } finally {
            btnSubmitParse.disabled = false;
            btnSubmitParse.innerHTML = `<i class="bi bi-magic"></i> Phân tích và tạo Booking tự động`;
        }
    });

    // ----------------------------------------------------
    // TAB 4: ASSISTANT CHAT (Question 3)
    // ----------------------------------------------------
    const chatForm = document.getElementById("chatForm");
    const chatInput = document.getElementById("chatInput");
    const btnSendChat = document.getElementById("btnSendChat");
    const chatMessages = document.getElementById("chatMessages");

    document.querySelectorAll(".sample-chat-btn").forEach(btn => {
        btn.addEventListener("click", () => {
            chatInput.value = btn.getAttribute("data-msg");
            chatForm.dispatchEvent(new Event("submit"));
        });
    });

    chatForm.addEventListener("submit", async (e) => {
        e.preventDefault();
        const message = chatInput.value.trim();
        if (!message) return;

        // Append user bubble
        appendChatBubble("user", message);
        chatInput.value = "";

        // Append typing indicator
        const loadingId = "loading-" + Date.now();
        const loadingBubble = document.createElement("div");
        loadingBubble.id = loadingId;
        loadingBubble.className = "chat-msg ai-msg";
        loadingBubble.innerHTML = `
            <div class="msg-bubble text-muted">
                <span class="spinner-border spinner-border-sm me-2"></span>Trợ lý AI đang xử lý...
            </div>`;
        chatMessages.appendChild(loadingBubble);
        chatMessages.scrollTop = chatMessages.scrollHeight;

        btnSendChat.disabled = true;

        try {
            const res = await fetch("/api/assistant/chat", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ message })
            });

            const data = await res.json();
            const elem = document.getElementById(loadingId);
            if (elem) elem.remove();

            if (!res.ok) {
                throw new Error(data.message || "Lỗi gọi API trợ lý.");
            }

            appendChatBubble("ai", data.reply, data.executedTools);

            // If a booking was cancelled, refresh report table
            if (data.executedTools && data.executedTools.some(t => t.functionName === "CancelBooking")) {
                loadBookingReport(filterFrom.value, filterTo.value);
            }
        } catch (err) {
            const elem = document.getElementById(loadingId);
            if (elem) elem.remove();
            appendChatBubble("ai", "Xin lỗi, đã xảy ra lỗi: " + err.message);
        } finally {
            btnSendChat.disabled = false;
        }
    });

    function appendChatBubble(role, text, tools = null) {
        const msgDiv = document.createElement("div");
        msgDiv.className = `chat-msg ${role}-msg`;

        let toolsHtml = "";
        if (tools && tools.length > 0) {
            toolsHtml = tools.map(t => `
                <div class="tool-badge">
                    <i class="bi bi-gear-fill me-1"></i> Function Called: <strong>${escapeHtml(t.functionName)}</strong>
                </div>
            `).join("");
        }

        msgDiv.innerHTML = `
            <div class="msg-bubble">
                ${toolsHtml ? `<div class="mb-1">${toolsHtml}</div>` : ""}
                <div style="white-space: pre-wrap;">${escapeHtml(text)}</div>
            </div>`;

        chatMessages.appendChild(msgDiv);
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    // Helpers
    function formatCurrency(amount) {
        if (amount == null) return "0 VND";
        return new Intl.NumberFormat("vi-VN").format(amount) + " VND";
    }

    function escapeHtml(str) {
        if (str == null) return "";
        return String(str)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }
});
