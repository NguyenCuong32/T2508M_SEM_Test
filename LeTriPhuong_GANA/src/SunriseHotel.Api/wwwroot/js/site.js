// Shared helpers for the Sunrise Hotel UI.
window.hotel = (() => {
    /** POST JSON and return the parsed body; throws an Error with a friendly message on failure. */
    async function postJson(url, body) {
        let res;
        try {
            res = await fetch(url, {
                method: "POST",
                headers: { "Content-Type": "application/json", "Accept": "application/json" },
                body: JSON.stringify(body)
            });
        } catch {
            throw new Error("Không kết nối được tới máy chủ. Vui lòng kiểm tra mạng và thử lại.");
        }

        const data = await res.json().catch(() => null);
        if (res.ok) return data;

        throw new Error(describeProblem(res.status, data));
    }

    /** Turn an RFC 7807 ProblemDetails response into a readable Vietnamese message. */
    function describeProblem(status, p) {
        if (status === 429) {
            const wait = p?.retryAfterSeconds ? ` Vui lòng thử lại sau ${duration(p.retryAfterSeconds)}.` : " Vui lòng thử lại sau ít phút.";
            return "Trợ lý AI đang quá tải (hết hạn mức Gemini)." + wait;
        }
        const parts = [];
        if (p?.detail) parts.push(p.detail);
        if (Array.isArray(p?.errors)) parts.push(...p.errors);
        else if (p?.errors && typeof p.errors === "object") parts.push(...Object.values(p.errors).flat());
        const detail = [...new Set(parts)].join(" ");
        const prefix = {
            400: "Yêu cầu chưa hợp lệ",
            404: "Không tìm thấy",
            409: "Không còn phòng phù hợp",
            502: "AI trả về dữ liệu không hợp lệ",
            503: "Dịch vụ AI tạm thời không khả dụng"
        }[status] ?? (p?.title || "Đã xảy ra lỗi");
        return detail ? `${prefix}: ${detail}` : `${prefix}.`;
    }

    /** 19152 -> "5 giờ 20 phút", 45 -> "45 giây" */
    function duration(sec) {
        if (sec < 60) return `${sec} giây`;
        const h = Math.floor(sec / 3600), m = Math.ceil((sec % 3600) / 60);
        return [h && `${h} giờ`, m && `${m} phút`].filter(Boolean).join(" ");
    }

    const money = n => new Intl.NumberFormat("vi-VN").format(n) + " ₫";

    const date = s => {
        if (!s) return "";
        const [y, m, d] = String(s).split("-");
        return d ? `${d}/${m}/${y}` : s;
    };

    const esc = s => String(s ?? "").replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));

    return { postJson, money, date, esc };
})();
