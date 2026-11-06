# Sunrise Hotel - AI Booking Assistant & Management System
**Generative AI for .NET Developers with Google AI - SET02**

---

## 1. Tổng quan Dự án (Project Overview)
Dự án được xây dựng bằng **.NET 10 (ASP.NET Core Web API)** tích hợp mô hình ngôn ngữ lớn **Google Gemini (`gemini-2.5-flash`)** và **Entity Framework Core (SQL Server)** nhằm tự động hóa quy trình chăm sóc khách hàng, trích xuất đơn đặt phòng và hỗ trợ nghiệp vụ khách sạn thông minh.

* **Database:** `SUNRISEHOTEL` (SQL Server / MSSQLLocalDB)
* **Frontend:** Single-page dashboard HTML5 / Bootstrap 5 / JavaScript tại `wwwroot/index.html` (truy cập tại `http://localhost:<port>/`)
* **Swagger UI:** `http://localhost:<port>/swagger`

---

## 2. Bảo mật API Key (API Key Security)
Tuân thủ nghiêm ngặt quy định không để lộ / không hardcode API Key trong mã nguồn:
1. **Lưu trữ bảo mật trong môi trường phát triển (Local Development):**
   * Sử dụng công cụ **.NET User Secrets**:
     ```bash
     cd SunriseHotel.Api
     dotnet user-secrets init
     dotnet user-secrets set "Gemini:ApiKey" "YOUR_ACTUAL_GEMINI_API_KEY"
     ```
2. **Hỗ trợ biến môi trường (Environment Variable):**
   * Ứng dụng tự động fallback đọc biến môi trường `GEMINI_API_KEY` nếu User Secrets không được cấu hình:
     ```csharp
     _apiKey = _config["Gemini:ApiKey"] 
               ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY") 
               ?? string.Empty;
     ```
3. **An toàn khi nộp bài / commit Git:**
   * Trong file `appsettings.json`, giá trị `Gemini:ApiKey` luôn được để rỗng `""`.
   * Thư mục `bin/`, `obj/` và các file cấu hình nhạy cảm được loại bỏ trước khi nộp.

---

## 3. Danh sách Prompt đã sử dụng (Prompts Used)

### 🔹 Câu 1: API `askhotel` (`POST /api/ai/ask`)
* **System Instruction:**
```text
You are a polite receptionist at Sunrise Hotel.
Hotel Information:
- Address: 25 Tran Phu, Nha Trang.
- Check-in time: from 14:00.
- Check-out time: before 12:00.
- Free breakfast: 6:30 - 9:30.
- Swimming pool: 5th floor.
- Free airport pick-up for Suite rooms.
Rules:
1. Act as a polite, helpful receptionist.
2. ONLY answer questions related to Sunrise Hotel.
3. If a question is not related to Sunrise Hotel, politely refuse to answer.
```

### 🔹 Câu 2: API `parsebooking` (`POST /api/bookings/parse`)
* **Prompt thiết kế Structured JSON Output:**
```text
Extract booking details from the following guest request into a clean JSON object.
Rules:
- guestName: Full name of the guest.
- phone: Phone number.
- roomType: Standardize to one of exactly: 'Single', 'Double', or 'Suite' ('phòng đơn' -> 'Single', 'phòng đôi' -> 'Double', 'phòng suite' -> 'Suite').
- checkIn: Check-in date formatted as YYYY-MM-DD.
- checkOut: Check-out date formatted as YYYY-MM-DD.
- guests: Number of guests as an integer.

Expected JSON format:
{
  "guestName": "string",
  "phone": "string",
  "roomType": "Single | Double | Suite",
  "checkIn": "YYYY-MM-DD",
  "checkOut": "YYYY-MM-DD",
  "guests": 2
}

Input: "{text}"
```
* **Cấu hình JSON Mode:** `GenerationConfig.ResponseMimeType = "application/json"`.

### 🔹 Câu 3: API `bookingassistant` (`POST /api/assistant/chat`)
* **System Instruction bổ sung:**
```text
You are a polite receptionist and booking assistant at Sunrise Hotel.
Hotel Information:
- Address: 25 Tran Phu, Nha Trang.
- Check-in time: from 14:00.
- Check-out time: before 12:00.
- Free breakfast: 6:30 - 9:30.
- Swimming pool: 5th floor.
- Free airport pick-up for Suite rooms.
Rules:
1. Act as a polite, helpful receptionist.
2. ONLY answer questions related to Sunrise Hotel.
3. When guests ask about room availability, viewing their bookings, or cancelling bookings, USE the provided tools.
4. If a question is not related to Sunrise Hotel, politely refuse to answer.
```

---

## 4. Khai báo Functions cho Gemini (Function Declarations / Tools)

Các Tool được đăng ký thông qua `Mscc.GenerativeAI.Types.Tools` với cơ chế gọi tự động (`enableAutomaticFunctionCalling: true`):

| Tên Hàm | Mô tả (Description) | Tham số (Parameters) | Kết quả trả về |
|---|---|---|---|
| **`CheckAvailability`** | Return available rooms and price. | `roomType` (string: Single, Double, Suite)<br>`checkIn` (string: YYYY-MM-DD)<br>`checkOut` (string: YYYY-MM-DD) | JSON danh sách phòng còn trống thỏa mãn không trùng lịch với các booking Confirmed. |
| **`GetBookingsByPhone`** | Return the bookings of a guest. | `phone` (string: guest phone number) | JSON danh sách toàn bộ các booking của khách hàng theo số điện thoại. |
| **`CancelBooking`** | Change booking status to Cancelled. | `bookingId` (int: ID của booking cần hủy) | Thông báo xác nhận trạng thái booking đã chuyển thành `Cancelled`. |

---

## 5. Hướng dẫn Test các API (API Endpoints)

### 1. `POST /api/ai/ask`
* **Request:**
```json
{
  "question": "Khách sạn có buffet sáng miễn phí không và mấy giờ?"
}
```
* **Response:**
```json
{
  "answer": "Dạ, Sunrise Hotel có phục vụ bữa sáng miễn phí từ 6:30 đến 9:30 hàng ngày ạ."
}
```

### 2. `POST /api/bookings/parse`
* **Request:**
```json
{
  "text": "Tôi là Nguyễn Văn An, sđt 0901234567, muốn đặt 1 phòng đôi từ 20/11/2026 đến 22/11/2026 cho 2 người"
}
```
* **Response:**
```json
{
  "message": "Booking created successfully.",
  "booking": {
    "bookingId": 4,
    "roomId": 4,
    "roomNumber": "202",
    "roomType": "Double",
    "guestName": "Nguyễn Văn An",
    "phone": "0901234567",
    "checkIn": "2026-11-20",
    "checkOut": "2026-11-22",
    "nights": 2,
    "totalPrice": 1900000.0,
    "status": "Confirmed"
  },
  "parsedData": {
    "guestName": "Nguyễn Văn An",
    "phone": "0901234567",
    "roomType": "Double",
    "checkIn": "2026-11-20",
    "checkOut": "2026-11-22",
    "guests": 2
  }
}
```

### 3. `POST /api/assistant/chat`
* **Request 1:** Tra cứu phòng trống
```json
{
  "message": "Còn phòng Suite nào trống từ 24/12/2026 đến 26/12/2026 không?"
}
```
* **Request 2:** Hủy đặt phòng
```json
{
  "message": "Hủy giúp tôi booking số 3."
}
```

### 4. `GET /api/reports/bookings?from=2026-11-20&to=2026-11-30`
* Trả về danh sách đơn đặt phòng đã xác nhận (Confirmed) kèm tính số đêm và thành tiền.

---

## 6. Xử lý Lỗi & Tối ưu (Bonus Points)
* **Xử lý chuỗi JSON:** Tự động cắt bỏ các Markdown code block (```json ... ```) để phòng ngừa trường hợp model trả về kèm định dạng markdown.
* **Validation chặt chẽ:** Kiểm tra ngày `checkOut > checkIn`, chuẩn hóa định dạng chữ hoa/thường cho `roomType` (Single, Double, Suite).
* **Quản lý hạn ngạch & Timeout:** Sử dụng `try-catch` đón các lỗi `Quota / Rate Limit (429)`, `InvalidOperationException` khi thiếu key, và trả về mã lỗi HTTP chuẩn kèm thông điệp rõ ràng cho client.
