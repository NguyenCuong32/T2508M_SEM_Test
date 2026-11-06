const formatPrice = new Intl.NumberFormat("vi-VN");

for (const form of document.querySelectorAll(".ai-form")) {
    form.addEventListener("submit", async event => {
        event.preventDefault();
        const input = form.querySelector("textarea");
        const button = form.querySelector("button");
        const output = form.querySelector(".answer");
        const message = input.value.trim();
        if (!message) {
            output.classList.add("error");
            output.textContent = "Vui lòng nhập nội dung.";
            input.focus();
            return;
        }

        output.classList.remove("error");
        output.textContent = "Đang xử lý yêu cầu…";
        button.disabled = true;

        try {
            const response = await fetch(form.dataset.url, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ [form.dataset.field]: message })
            });
            const data = await response.json();
            if (!response.ok) {
                throw new Error(data.title || "Yêu cầu chưa được xử lý.");
            }

            output.textContent = data.answer ??
                ("Đã xác nhận đơn #" + data.bookingId + " · Phòng " + data.roomNumber +
                " · " + data.nights + " đêm · " + formatPrice.format(data.totalPrice) + " VND.");
            if (data.bookingId) input.value = "";
        } catch (error) {
            output.classList.add("error");
            output.textContent = error instanceof SyntaxError ? "Không nhận được phản hồi hợp lệ." : error.message;
        } finally {
            button.disabled = false;
        }
    });
}
