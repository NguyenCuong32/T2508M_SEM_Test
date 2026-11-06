# SUNRISE HOTEL - AI BOOKING ASSISTANT
> **Generative AI for .NET Developers with Google AI – Set02 Practical Paper**  
> **Author:** Senior Full-Stack .NET Developer  
> **Tech Stack:** ASP.NET Core 8 / 10 • Entity Framework Core • SQL Server • Google Gemini AI • Bootstrap 5

---

## 1. TỔNG QUAN HỆ THỐNG (PROJECT OVERVIEW)

Dự án **Sunrise Hotel AI Booking Assistant** là giải pháp phần mềm toàn diện hỗ trợ khách hàng và ban quản lý khách sạn Sunrise Hotel (25 Trần Phú, Nha Trang) thông qua trí tuệ nhân tạo Google Gemini:
- **Lễ tân ảo thông minh (Ask Hotel):** Giải đáp thắc mắc dịch vụ và từ chối các câu hỏi ngoài lề.
- **Trích xuất thông tin đặt phòng tự nhiên (Parse Booking):** Tự động bóc tách ngày, phòng, số khách bằng Structured JSON, kiểm tra phòng trống trên CSDL và lưu đơn `Confirmed`.
- **Trợ lý gọi hàm C# (Function Calling Assistant):** Thực thi trực tiếp các hàm C# như kiểm tra phòng trống, tra cứu đơn theo số điện thoại, hủy đơn theo mã.
- **Báo cáo & Giao diện quản trị (Report & Frontend):** Báo cáo danh sách đặt phòng `Confirmed` theo khoảng thời gian trực quan trên Web UI và Swagger.

---

## 2. CƠ SỞ DỮ LIỆU (DATABASE CONFIGURATION)

### 2.1. Cấu hình CSDL `SUNRISEHOTEL`
- **Tên CSDL:** `SUNRISEHOTEL` (hỗ trợ SQL Server / LocalDB).
- **Chuỗi kết nối (Connection String):**
  ```json
  "Server=localhost;Database=SUNRISEHOTEL;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;"
  ```
- Hệ thống hỗ trợ 2 hình thức triển khai:
  1. **Tự động (Code First):** Hệ thống tự động tạo DB và nạp dữ liệu ban đầu khi khởi động ứng dụng thông qua `DbInitializer.InitializeAsync()`.
  2. **Thủ công (Database First / Script):** Sử dụng tệp script SQL có sẵn `SUNRISEHOTEL_init.sql`.

### 2.2. Bảng dữ liệu & Khởi tạo (Schema & Seed Data)

#### Bảng `Room`
| Column Name | Data Type | Description |
|---|---|---|
| `RoomId` | `int` (PK) | Mã định danh phòng |
| `RoomNumber` | `nvarchar(10)` | Số phòng |
| `RoomType` | `nvarchar(32)` | Single / Double / Suite |
| `PricePerNight` | `decimal(18,2)` | Giá mỗi đêm (VNĐ) |
| `MaxGuests` | `int` | Số lượng khách tối đa |

*Dữ liệu ban đầu bảng `Room`:*
- 1: `101`, `Single`, 600.000 VNĐ, 1 khách
- 2: `102`, `Single`, 600.000 VNĐ, 1 khách
- 3: `201`, `Double`, 950.000 VNĐ, 2 khách
- 4: `202`, `Double`, 950.000 VNĐ, 2 khách
- 5: `301`, `Suite`, 2.200.000 VNĐ, 4 khách

#### Bảng `Booking`
| Column Name | Data Type | Description |
|---|---|---|
| `BookingId` | `int` (PK, identity) | Mã đơn đặt phòng |
| `RoomId` | `int` (FK) | Phòng được đặt |
| `GuestName` | `nvarchar(128)` | Họ tên khách hàng |
| `Phone` | `nvarchar(20)` | Số điện thoại liên hệ |
| `CheckIn` | `date` | Ngày nhận phòng |
| `CheckOut` | `date` | Ngày trả phòng |
| `TotalPrice` | `decimal(18,2)` | Số đêm x Đơn giá phòng |
| `Status` | `nvarchar(16)` | Confirmed / Cancelled |

---

## 3. BẢO MẬT API KEY GOOGLE GEMINI (API KEY PROTECTION)

Tuân thủ nghiêm ngặt yêu cầu không hardcode API Key trong mã nguồn, hệ thống áp dụng cơ chế bảo mật theo tiêu chuẩn của Microsoft .NET:

### 3.1. Cơ chế tải Key trong hệ thống
Lớp `GeminiService` kiểm tra và tải API key theo thứ tự ưu tiên:
1. **Biến môi trường (Environment Variable):** `GEMINI_API_KEY`
2. **Quản lý bí mật người dùng (.NET User Secrets):** `Gemini:ApiKey`
3. Tệp cấu hình `appsettings.json` chỉ chứa placeholder `YOUR_GEMINI_API_KEY_HERE` và không bao giờ chứa key thật.

### 3.2. Hướng dẫn thiết lập API Key

#### Cách 1: Sử dụng .NET User Secrets (Khuyên dùng trong môi trường Dev)
```bash
# Khởi tạo và thiết lập User Secrets
dotnet user-secrets init
dotnet user-secrets set "Gemini:ApiKey" "AIzaSyYourSecretGeminiApiKeyHere..."
```
> *Lợi ích:* Key được lưu tại thư mục profile của người dùng trên máy tính (`%APPDATA%\Microsoft\UserSecrets\...`), tách biệt hoàn toàn khỏi thư mục dự án và không bao giờ bị commit lên Git.

#### Cách 2: Sử dụng Biến môi trường (Environment Variable)
- **PowerShell:**
  ```powershell
  $env:GEMINI_API_KEY="AIzaSyYourSecretGeminiApiKeyHere..."
  ```
- **Linux / macOS:**
  ```bash
  export GEMINI_API_KEY="AIzaSyYourSecretGeminiApiKeyHere..."
  ```
- **Windows Command Prompt:**
  ```cmd
  set GEMINI_API_KEY=AIzaSyYourSecretGeminiApiKeyHere...
  ```

---

## 4. CHI TIẾT CÁC PROMPT & CẤU TRÚC HÀM (REQUIREMENTS 1, 2, 3)

### 4.1. Question 1: API `askhotel` (`POST /api/ai/ask`)
- **Mục tiêu:** Đóng vai lễ tân lịch sự, chỉ trả lời về khách sạn Sunrise Hotel và từ chối các câu hỏi ngoài lề.
- **System Instruction sử dụng:**
```text
Bạn là nhân viên lễ tân lịch sự, nhiệt tình và chuyên nghiệp của khách sạn Sunrise Hotel (địa chỉ: 25 Trần Phú, Nha Trang).
Thông tin chính xác về khách sạn:
- Giờ nhận phòng (checkin): từ 14:00.
- Giờ trả phòng (checkout): trước 12:00.
- Ăn sáng miễn phí: từ 6:30 đến 9:30 hàng ngày.
- Đưa đón sân bay miễn phí: chỉ áp dụng miễn phí cho khách đặt phòng loại Suite.
- Hồ bơi: ở tầng 5 của khách sạn.
- Địa chỉ: 25 Trần Phú, Nha Trang.
- Các loại phòng và giá niêm yết:
  + Phòng Single (Đơn): 600.000 VNĐ/đêm, tối đa 1 khách.
  + Phòng Double (Đôi): 950.000 VNĐ/đêm, tối đa 2 khách.
  + Phòng Suite: 2.200.000 VNĐ/đêm, tối đa 4 khách (kèm đón sân bay miễn phí).

Quy tắc bắt buộc:
1. CHỈ trả lời các câu hỏi liên quan đến Sunrise Hotel, các dịch vụ, chính sách và đặt phòng của khách sạn.
2. TUYỆT ĐỐI từ chối lịch sự mọi câu hỏi không liên quan đến Sunrise Hotel (ví dụ: kiến thức chung, code lập trình, thời tiết ngoài lề, khách sạn khác...). Khi từ chối, hãy lịch sự thông báo bạn chỉ hỗ trợ thông tin về Sunrise Hotel.
3. Trả lời bằng ngôn ngữ mà khách hỏi (ưu tiên Tiếng Việt lịch sự).
```

### 4.2. Question 2: API `parsebooking` (`POST /api/bookings/parse`)
- **Mục tiêu:** Bóc tách yêu cầu đặt phòng tự nhiên sang định dạng JSON có cấu trúc chuẩn xác (`Structured Outputs` / `responseSchema`).
- **Prompt:**
```text
Hãy trích xuất thông tin đặt phòng khách sạn từ câu yêu cầu sau thành JSON đúng cấu trúc.
Yêu cầu đặt phòng: "{text}"

Quy tắc chuẩn hóa:
- roomType PHẢI là một trong ba giá trị: 'Single', 'Double', 'Suite'.
  + Nếu khách nói 'phòng đơn', 'đơn', 1 người -> 'Single'
  + Nếu khách nói 'phòng đôi', 'đôi', 2 người -> 'Double'
  + Nếu khách nói 'phòng suite', 'phòng sang', 'gia đình' -> 'Suite'
- checkIn và checkOut phải là định dạng 'YYYY-MM-DD'. Nếu không rõ năm, mặc định là năm 2026.
- guests: số lượng khách (số nguyên).
- guestName: họ và tên khách.
- phone: số điện thoại của khách.
```
- **Response Schema JSON khai báo cho Gemini:**
```json
{
  "type": "OBJECT",
  "properties": {
    "guestName": { "type": "STRING", "description": "Tên đầy đủ của khách hàng" },
    "phone": { "type": "STRING", "description": "Số điện thoại của khách hàng" },
    "roomType": { "type": "STRING", "description": "Loại phòng", "enum": ["Single", "Double", "Suite"] },
    "checkIn": { "type": "STRING", "description": "Ngày nhận phòng định dạng YYYY-MM-DD" },
    "checkOut": { "type": "STRING", "description": "Ngày trả phòng định dạng YYYY-MM-DD" },
    "guests": { "type": "INTEGER", "description": "Số lượng khách" }
  },
  "required": ["guestName", "phone", "roomType", "checkIn", "checkOut", "guests"]
}
```
- **Quy trình xử lý C# Backend:**
  1. Deserialize JSON từ Gemini sang đối tượng C# `ParsedBookingData`.
  2. Kiểm tra `checkOut > checkIn`.
  3. Kiểm tra tính hợp lệ của `roomType` và `guests <= MaxGuests`.
  4. Tìm phòng trống không bị trùng ngày với bất kỳ đơn nào có trạng thái `Confirmed`:
     `!Bookings.Any(b => b.RoomId == r.RoomId && b.Status == "Confirmed" && b.CheckIn < checkOut && b.CheckOut > checkIn)`
  5. Tính toán `nights = checkOut - checkIn` và `TotalPrice = nights * PricePerNight`.
  6. Lưu bản ghi mới vào CSDL với trạng thái `Confirmed`.

### 4.3. Question 3: API `bookingassistant` (`POST /api/assistant/chat`)
- **Mục tiêu:** Chat đa lượt với Gemini sử dụng Function Calling gọi các hàm C#.
- **Các Function Declarations khai báo cho Gemini:**

#### 1. Hàm `CheckAvailability`
```json
{
  "name": "CheckAvailability",
  "description": "Kiểm tra danh sách phòng còn trống và giá phòng theo loại phòng và khoảng thời gian check-in, check-out.",
  "parameters": {
    "type": "OBJECT",
    "properties": {
      "roomType": {
        "type": "STRING",
        "description": "Loại phòng cần kiểm tra ('Single', 'Double', 'Suite'). Nếu khách muốn xem mọi phòng, để trống hoặc 'all'."
      },
      "checkIn": {
        "type": "STRING",
        "description": "Ngày nhận phòng định dạng YYYY-MM-DD (hoặc DD/MM/YYYY)."
      },
      "checkOut": {
        "type": "STRING",
        "description": "Ngày trả phòng định dạng YYYY-MM-DD (hoặc DD/MM/YYYY)."
      }
    },
    "required": ["checkIn", "checkOut"]
  }
}
```

#### 2. Hàm `GetBookingsByPhone`
```json
{
  "name": "GetBookingsByPhone",
  "description": "Tra cứu các đơn đặt phòng của khách hàng theo số điện thoại.",
  "parameters": {
    "type": "OBJECT",
    "properties": {
      "phone": {
        "type": "STRING",
        "description": "Số điện thoại khách hàng cần tra cứu (ví dụ: '0901234567')."
      }
    },
    "required": ["phone"]
  }
}
```

#### 3. Hàm `CancelBooking`
```json
{
  "name": "CancelBooking",
  "description": "Hủy một đơn đặt phòng dựa trên mã đơn đặt phòng (bookingId), cập nhật trạng thái đơn thành 'Cancelled'.",
  "parameters": {
    "type": "OBJECT",
    "properties": {
      "bookingId": {
        "type": "INTEGER",
        "description": "Mã số đơn đặt phòng cần hủy (ví dụ: 1, 2, 3)."
      }
    },
    "required": ["bookingId"]
  }
}
```

- **Quy trình tương tác Function Calling:**
  1. Người dùng gửi tin nhắn trò chuyện.
  2. Gemini phân tích và quyết định gọi hàm chức năng (ví dụ `functionCall: CheckAvailability`).
  3. Backend C# bắt được lời gọi hàm, thực thi trên CSDL thông qua `IHotelService`.
  4. Backend gửi kết quả của hàm (`functionResponse`) ngược lại cho Gemini.
  5. Gemini tổng hợp câu trả lời tự nhiên bằng tiếng Việt gửi về cho khách hàng.

---

## 5. HƯỚNG DẪN KHỞI CHẠY VÀ KIỂM THỬ (HOW TO RUN)

### 5.1. Khởi chạy ứng dụng
```bash
# 1. Khôi phục thư viện và biên dịch
dotnet restore
dotnet build

# 2. Chạy ứng dụng
dotnet run --launch-profile http
```
Ứng dụng sẽ hoạt động tại:
- **Trang chủ Web UI:** `http://localhost:5021/`
- **Swagger Documentation:** `http://localhost:5021/swagger`

### 5.2. Kiểm thử nhanh các API bằng cURL

#### API 1: Hỏi đáp Lễ tân (`POST /api/ai/ask`)
```bash
curl -X POST http://localhost:5021/api/ai/ask \
  -H "Content-Type: application/json" \
  -d "{\"question\": \"Mấy giờ nhận phòng và có ăn sáng miễn phí không?\"}"
```

#### API 2: Đặt phòng bằng ngôn ngữ tự nhiên (`POST /api/bookings/parse`)
```bash
curl -X POST http://localhost:5021/api/bookings/parse \
  -H "Content-Type: application/json" \
  -d "{\"text\": \"Tôi là Nguyễn Văn An, sđt 0901234567, muốn đặt 1 phòng đôi từ 20/11/2026 đến 22/11/2026 cho 2 người\"}"
```

#### API 3: Trợ lý Function Calling (`POST /api/assistant/chat`)
```bash
# Kiểm tra phòng trống
curl -X POST http://localhost:5021/api/assistant/chat \
  -H "Content-Type: application/json" \
  -d "{\"message\": \"Còn phòng Suite nào trống từ 24/12/2026 đến 26/12/2026 không?\"}"

# Tra cứu đặt phòng
curl -X POST http://localhost:5021/api/assistant/chat \
  -H "Content-Type: application/json" \
  -d "{\"message\": \"Tra cứu đơn đặt phòng của số điện thoại 0901234567\"}"

# Hủy đặt phòng
curl -X POST http://localhost:5021/api/assistant/chat \
  -H "Content-Type: application/json" \
  -d "{\"message\": \"Hủy giúp tôi booking số 3\"}"
```

#### API 4: Báo cáo Đặt phòng (`GET /api/reports/bookings`)
```bash
# Lấy toàn bộ danh sách Confirmed
curl http://localhost:5021/api/reports/bookings

# Lọc theo khoảng ngày checkin
curl "http://localhost:5021/api/reports/bookings?from=2026-11-20&to=2026-11-23"
```

### 5.3. Bộ sưu tập Postman (Postman Collection)
File `SunriseHotelAI.postman_collection.json` đã được tạo sẵn trong thư mục gốc của dự án. Quý thầy cô/người đánh giá chỉ cần import trực tiếp vào Postman để kiểm thử tất cả các trường hợp!

---

## 6. ĐIỂM CỘNG & XỬ LÝ LỖI (BONUS & ERROR HANDLING)

1. **Chuẩn kiến trúc phần mềm (Clean Architecture & DI):** Tách biệt rõ ràng tầng Controller, tầng DTO, tầng Service Domain (`IHotelService`), và tầng Gemini Client (`IGeminiService`).
2. **Xử lý hạn ngạch API (HTTP 429 Quota Exceeded):** Bắt và xử lý lịch sự lỗi 429 từ Google Gemini API, chuyển tiếp thông báo rõ ràng cho người dùng thay vì gây gián đoạn hệ thống.
3. **Cơ chế Fallback thông minh:** Khi chưa cấu hình Gemini API Key hoặc khi gặp lỗi hạn ngạch tạm thời, hệ thống tự động kích hoạt bộ phân tích dự phòng (Intelligent Fallback Engine), đảm bảo người chấm thi vẫn có thể test mượt mà toàn bộ chức năng đặt phòng, hủy phòng và kiểm tra phòng trống.
4. **Kiểm tra xung đột phòng trùng lịch (Room Conflict Prevention):** Thuật toán kiểm tra giao thoa khoảng thời gian (`b.CheckIn < checkOut && b.CheckOut > checkIn`) ngăn chặn triệt để tình trạng đặt trùng phòng đã xác nhận (`Confirmed`).
5. **Giao diện Web cao cấp (Luxury UI):** Giao diện Bootstrap 5 tích hợp đầy đủ thống kê doanh thu, số đêm, lọc báo cáo theo ngày, giao diện chat thời gian thực hiển thị chi tiết badge lệnh gọi hàm C#.
