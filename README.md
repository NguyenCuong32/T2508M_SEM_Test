# Sunrise Hotel - AI Booking Assistant (.NET 8+ & Google Gemini AI)
> **Môn thi:** Generative AI for .NET Developers with Google AI – SET02  
> **Thời gian:** 60 phút | **Điểm số tối đa:** 20/20  
> **Nhánh thực hiện:** `GANA_LeTrungKien`  

---

## 📋 Mục lục
1. [Giới thiệu dự án](#1-giới-thiệu-dự-án)
2. [Bảo vệ Gemini API Key (Gemini API Key Protection)](#2-bảo-vệ-gemini-api-key)
3. [Danh sách Prompts và Function Declarations (Yêu cầu 1, 2, 3)](#3-danh-sách-prompts-và-function-declarations)
   - [Câu 1: API askhotel (Lễ tân AI)](#31-câu-1-api-askhotel-post-apiaask)
   - [Câu 2: API parsebooking (Trích xuất JSON có cấu trúc)](#32-câu-2-api-parsebooking-post-apibookingsparse)
   - [Câu 3: API bookingassistant (Function Calling)](#33-câu-3-api-bookingassistant-post-apiassistantchat)
4. [Báo cáo đặt phòng (Câu 4) & Website Giao diện (Câu 5)](#4-báo-cáo-đặt-phòng-câu-4--website-giao-diện-câu-5)
5. [Cấu trúc Cơ sở dữ liệu (Database SUNRISEHOTEL)](#5-cấu-trúc-cơ-sở-dữ-liệu)
6. [Hướng dẫn cài đặt & Chạy ứng dụng](#6-hướng-dẫn-cài-đặt--chạy-ứng-dụng)
7. [Kiểm thử với Postman Collection](#7-kiểm-thử-với-postman-collection)

---

## 1. Giới thiệu dự án

Dự án xây dựng hệ thống **Trợ lý AI Đặt phòng Khách sạn Sunrise Hotel** (25 Trần Phú, Nha Trang) bằng **ASP.NET Core Web API** kết hợp **Google Gemini AI** (Model: `gemini-2.5-flash`), đáp ứng trọn vẹn 6 yêu cầu và các tiêu chí thưởng (bonus) trong đề thi:
- **Hỏi đáp thông tin khách sạn (`/api/ai/ask`):** Đóng vai lễ tân lịch sự, từ chối câu hỏi ngoài lề.
- **Trích xuất đặt phòng tự nhiên (`/api/bookings/parse`):** Gemini trả về JSON có cấu trúc (`response_schema`), C# validate ngày tháng/loại phòng, tìm phòng trống, tính tổng tiền và tạo booking với trạng thái `Confirmed`.
- **Trợ lý thông minh với Function Calling (`/api/assistant/chat`):** Tự động điều phối và gọi 3 hàm C# (`CheckAvailability`, `GetBookingsByPhone`, `CancelBooking`) trong vòng lặp đa lượt.
- **Báo cáo đặt phòng (`/api/reports/bookings`):** Trả về danh sách đơn phòng đã xác nhận kèm bộ lọc khoảng thời gian (`from`, `to`).
- **Giao diện Web trực quan (Front-End):** Website hiển thị bảng báo cáo đặt phòng, bộ lọc ngày, thống kê doanh thu và tích hợp các bảng điều khiển kiểm thử trực tiếp cho cả 3 chức năng AI.

---

## 2. Bảo vệ Gemini API Key

Theo quy định bắt buộc của đề thi (Note 3 & Marking Schema: 2 điểm): **Khóa bí mật API tuyệt đối KHÔNG được hard-code trong mã nguồn**.

Dự án áp dụng 2 cơ chế chuẩn doanh nghiệp của .NET để bảo vệ API Key:

### Cách 1: Sử dụng .NET User Secrets (Khuyến nghị trong môi trường phát triển)
Secret được lưu trữ bên ngoài thư mục dự án của mã nguồn (trong thư mục cấu hình cá nhân của người dùng trên hệ điều hành), không bao giờ bị đưa lên Git:
```bash
cd SunriseHotel.Api
dotnet user-secrets set "Gemini:ApiKey" "YOUR_ACTUAL_GEMINI_API_KEY"
```

### Cách 2: Sử dụng Biến môi trường (Environment Variable)
Trên Windows (PowerShell):
```powershell
$env:GEMINI_API_KEY="YOUR_ACTUAL_GEMINI_API_KEY"
```
Trên Linux/macOS:
```bash
export GEMINI_API_KEY="YOUR_ACTUAL_GEMINI_API_KEY"
```

### Cách thức nạp trong mã nguồn (`GeminiService.cs`):
Mã nguồn đọc key linh hoạt qua thứ tự ưu tiên:
```csharp
_apiKey = _configuration["Gemini:ApiKey"]
          ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY")
          ?? Environment.GetEnvironmentVariable("Gemini__ApiKey")
          ?? string.Empty;
```
Tập tin `appsettings.json` trong repository chỉ để trống: `"ApiKey": ""` nhằm đảm bảo an toàn tuyệt đối khi nộp bài.

---

## 3. Danh sách Prompts và Function Declarations

### 3.1. Câu 1: API `askhotel` (`POST /api/ai/ask`)
* **Endpoint:** `POST /api/ai/ask`
* **Request Body:**
  ```json
  {
    "question": "Giờ check-in và check-out của khách sạn là mấy giờ?"
  }
  ```
* **System Instruction:**
  ```text
  Bạn là lễ tân lịch sự, chu đáo và nhiệt tình tại Sunrise Hotel (địa chỉ: 25 Trần Phú, Nha Trang).
  Thông tin khách sạn:
  - Giờ check-in: từ 14:00.
  - Giờ check-out: trước 12:00.
  - Bữa sáng miễn phí: từ 6:30 đến 9:30.
  - Đưa đón sân bay miễn phí cho hạng phòng Suite.
  - Hồ bơi: ở tầng 5.
  - Các loại phòng:
    + Single (Phòng đơn): 600.000 VND / đêm, tối đa 1 khách.
    + Double (Phòng đôi): 950.000 VND / đêm, tối đa 2 khách.
    + Suite (Phòng cao cấp): 2.200.000 VND / đêm, tối đa 4 khách.

  QUY TẮC BẮT BUỘC:
  1. Luôn giữ thái độ lễ phép, lịch sự của một lễ tân khách sạn chuyên nghiệp.
  2. Chỉ trả lời các câu hỏi liên quan đến Sunrise Hotel và dịch vụ của khách sạn.
  3. Lịch sự từ chối trả lời nếu câu hỏi không liên quan đến Sunrise Hotel (ví dụ hỏi về toán, code, lịch sử thế giới, bóng đá,...).
  ```

---

### 3.2. Câu 2: API `parsebooking` (`POST /api/bookings/parse`)
* **Endpoint:** `POST /api/bookings/parse`
* **Request Body:**
  ```json
  {
    "text": "Tôi là Nguyễn Văn An, sđt 0901234567, muốn đặt 1 phòng đôi từ 20/11/2026 đến 22/11/2026 cho 2 người"
  }
  ```
* **System Instruction:**
  ```text
  Bạn là trợ lý AI chuyên trích xuất thông tin đặt phòng tại Sunrise Hotel.
  Nhiệm vụ: Phân tích văn bản yêu cầu đặt phòng của khách hàng và trích xuất thành JSON.
  Quy tắc trích xuất:
  - guestName: Họ tên khách hàng.
  - phone: Số điện thoại liên hệ.
  - roomType: Chuẩn hóa chính xác về một trong 3 loại: 'Single', 'Double', 'Suite'.
  - checkIn: Ngày nhận phòng theo định dạng 'YYYY-MM-DD'.
  - checkOut: Ngày trả phòng theo định dạng 'YYYY-MM-DD'.
  - guests: Số lượng khách (số nguyên).
  Năm tham chiếu hiện tại là năm 2026. Nếu khách nhập ngày tháng không kèm năm, mặc định lấy năm 2026.
  Chỉ trả về JSON thuần túy theo schema yêu cầu.
  ```
* **Gemini JSON Schema (`generationConfig.response_schema`):**
  ```json
  {
    "type": "OBJECT",
    "properties": {
      "guestName": { "type": "STRING" },
      "phone": { "type": "STRING" },
      "roomType": { "type": "STRING" },
      "checkIn": { "type": "STRING" },
      "checkOut": { "type": "STRING" },
      "guests": { "type": "INTEGER" }
    },
    "required": ["guestName", "phone", "roomType", "checkIn", "checkOut", "guests"]
  }
  ```
* **Logic xử lý C# sau khi trích xuất:**
  1. Kiểm tra tính hợp lệ của ngày tháng: `checkOut > checkIn`.
  2. Chuẩn hóa & kiểm tra loại phòng thuộc danh sách: `Single`, `Double`, `Suite`.
  3. Tìm kiếm phòng trống thuộc loại phòng đó không bị trùng lặp với đơn đặt phòng nào khác đang có trạng thái `Confirmed` trong khoảng `[checkIn, checkOut]`.
  4. Tính toán `TotalPrice = số đêm * Giá phòng`.
  5. Thêm mới bản ghi vào bảng `Booking` với `Status = "Confirmed"`.

---

### 3.3. Câu 3: API `bookingassistant` (`POST /api/assistant/chat`)
* **Endpoint:** `POST /api/assistant/chat`
* **Request Body:**
  ```json
  {
    "message": "Còn phòng Suite nào trống từ 24/12/2026 đến 26/12/2026 không?"
  }
  ```
* **System Instruction:**
  ```text
  Bạn là trợ lý AI thông minh hỗ trợ khách hàng tại Sunrise Hotel.
  Bạn có quyền sử dụng các function sau để phục vụ khách:
  - CheckAvailability(roomType, checkIn, checkOut): Kiểm tra phòng trống và giá phòng trong khoảng thời gian.
  - GetBookingsByPhone(phone): Tra cứu danh sách các đơn đặt phòng của khách theo số điện thoại.
  - CancelBooking(bookingId): Hủy đơn đặt phòng theo mã bookingId.

  Năm tham chiếu hiện tại là năm 2026. Nếu khách hỏi ngày không kèm năm, mặc định là năm 2026.
  Khi khách yêu cầu kiểm tra phòng, tra cứu hoặc hủy phòng, hãy gọi function thích hợp.
  Sau khi nhận kết quả từ function, hãy trả lời khách hàng một cách lịch sự, rõ ràng và đầy đủ bằng tiếng Việt.
  ```
* **Khai báo Function Declarations (Tools):**
  ```json
  [
    {
      "name": "CheckAvailability",
      "description": "Return available rooms and price for given room type and date range",
      "parameters": {
        "type": "OBJECT",
        "properties": {
          "roomType": { "type": "STRING", "description": "Room type: Single, Double, or Suite (optional)" },
          "checkIn": { "type": "STRING", "description": "Check-in date (YYYY-MM-DD)" },
          "checkOut": { "type": "STRING", "description": "Check-out date (YYYY-MM-DD)" }
        },
        "required": ["checkIn", "checkOut"]
      }
    },
    {
      "name": "GetBookingsByPhone",
      "description": "Return the bookings of a guest by phone number",
      "parameters": {
        "type": "OBJECT",
        "properties": {
          "phone": { "type": "STRING", "description": "Guest phone number" }
        },
        "required": ["phone"]
      }
    },
    {
      "name": "CancelBooking",
      "description": "Change booking status to Cancelled",
      "parameters": {
        "type": "OBJECT",
        "properties": {
          "bookingId": { "type": "INTEGER", "description": "The ID of the booking to cancel" }
        },
        "required": ["bookingId"]
      }
    }
  ]
  ```

---

## 4. Báo cáo đặt phòng (Câu 4) & Website Giao diện (Câu 5)

### 4.1. API `getbookingreport` (`GET /api/reports/bookings`)
* **Endpoint:** `GET /api/reports/bookings?from=2026-11-20&to=2026-11-25`
* **Response format:**
  ```json
  [
    {
      "no": 1,
      "roomNumber": "201",
      "roomType": "Double",
      "guestName": "Nguyễn Văn An",
      "checkIn": "20/11/2026",
      "checkOut": "22/11/2026",
      "nights": 2,
      "totalPrice": 1900000.00
    },
    {
      "no": 2,
      "roomNumber": "101",
      "roomType": "Single",
      "guestName": "Trần Thị Bình",
      "checkIn": "21/11/2026",
      "checkOut": "24/11/2026",
      "nights": 3,
      "totalPrice": 1800000.00
    },
    {
      "no": 3,
      "roomNumber": "301",
      "roomType": "Suite",
      "guestName": "Lê Hoàng Nam",
      "checkIn": "25/11/2026",
      "checkOut": "26/11/2026",
      "nights": 1,
      "totalPrice": 2200000.00
    }
  ]
  ```

### 4.2. Giao diện Website (`wwwroot/index.html`)
Ứng dụng tích hợp sẵn giao diện Web tương tác trực tiếp:
- **Tab 1: Báo cáo đặt phòng:** Hiển thị bảng số liệu chuẩn hóa, hỗ trợ bộ chọn ngày `Từ ngày` - `Đến ngày`, thống kê tổng đơn, tổng số đêm và tổng doanh thu.
- **Tab 2: Hỏi đáp lễ tân AI:** Tương tác với lễ tân ảo với câu hỏi tự do hoặc nút bấm mẫu.
- **Tab 3: Trích xuất & Đặt phòng:** Nhập văn bản đặt phòng tự nhiên, hiển thị trực tiếp JSON trích xuất từ Gemini và thẻ thông tin đơn đặt phòng được tạo trong C#.
- **Tab 4: Trợ lý AI (Function Calling):** Chatbot tương tác tự động kích hoạt C# Function Calling và hiển thị nhãn các hàm C# đã thực thi ngầm.

---

## 5. Cấu trúc Cơ sở dữ liệu

Database name: `SUNRISEHOTEL`

Tập tin SQL hoàn chỉnh nằm tại thư mục gốc: **`database.sql`**.

### Bảng Room:
| Cột | Kiểu dữ liệu | Mô tả |
| :--- | :--- | :--- |
| `RoomId` | `int` (PK) | Mã phòng (1 đến 5) |
| `RoomNumber` | `nvarchar(10)` | Số phòng (101, 102, 201, 202, 301) |
| `RoomType` | `nvarchar(32)` | Single / Double / Suite |
| `PricePerNight`| `decimal(18,2)` | Giá mỗi đêm (VND) |
| `MaxGuests` | `int` | Số khách tối đa |

### Bảng Booking:
| Cột | Kiểu dữ liệu | Mô tả |
| :--- | :--- | :--- |
| `BookingId` | `int` (PK, identity) | Mã định danh đơn đặt phòng |
| `RoomId` | `int` (FK -> Room) | Mã phòng đã đặt |
| `GuestName` | `nvarchar(128)` | Họ tên khách hàng |
| `Phone` | `nvarchar(20)` | Số điện thoại khách hàng |
| `CheckIn` | `date` | Ngày nhận phòng |
| `CheckOut` | `date` | Ngày trả phòng |
| `TotalPrice` | `decimal(18,2)` | Tổng tiền (`Số đêm * PricePerNight`) |
| `Status` | `nvarchar(16)` | `Confirmed` / `Cancelled` |

*Lưu ý:* Hệ thống hỗ trợ cả SQL Server và tự động khởi tạo dữ liệu mẫu nếu chạy trên SQLite (`sunrisehotel.db`), giúp người chấm bài có thể chạy ngay lập tức mà không cần cấu hình SQL Server phức tạp.

---

## 6. Hướng dẫn cài đặt & Chạy ứng dụng

### Bước 1: Khởi tạo Database (Nếu dùng SQL Server)
Mở SQL Server Management Studio (SSMS) hoặc `sqlcmd` và chạy file `database.sql`:
```powershell
sqlcmd -S localhost -i database.sql
```

### Bước 2: Thiết lập Gemini API Key
```powershell
cd SunriseHotel.Api
dotnet user-secrets set "Gemini:ApiKey" "YOUR_ACTUAL_API_KEY"
```

### Bước 3: Chạy ứng dụng
Tại thư mục gốc:
```powershell
dotnet run --project SunriseHotel.Api
```

### Bước 4: Trải nghiệm
- **Website ứng dụng:** Mở trình duyệt tại `http://localhost:5000` (hoặc cổng hiển thị trên console).
- **Swagger UI:** Mở `http://localhost:5000/swagger`.

---

## 7. Kiểm thử với Postman Collection

Tập tin Postman đã được chuẩn bị sẵn tại: **`SunriseHotel_API.postman_collection.json`**.
1. Mở Postman &rarr; chọn **Import** &rarr; chọn file `SunriseHotel_API.postman_collection.json`.
2. Biến môi trường `baseUrl` mặc định là `http://localhost:5000`.
3. Có sẵn các requests mẫu cho toàn bộ 4 câu hỏi trong đề thi.
