# DỰ ÁN SUNRISE HOTEL - GENERATIVE AI FOR .NET DEVELOPERS (SET02)

Giải pháp trọn bộ theo chuẩn Senior .NET Developer kết hợp **ASP.NET Core Web API**, **Entity Framework Core (SQL Server Code First)** và **Google Gemini REST API (gemini-2.5-flash)**.

---

## 1. CẤU TRÚC THƯ MỤC DỰ ÁN
Dự án được thiết kế theo cấu trúc phân tầng chuẩn chỉnh:

```
baithi/
│
├── Controllers/
│   ├── AiController.cs             # Yêu cầu 1: POST /api/ai/ask (AI Lễ tân)
│   ├── BookingsController.cs       # Yêu cầu 2: POST /api/bookings/parse (Bóc tách JSON Mode)
│   ├── AssistantController.cs      # Yêu cầu 3: POST /api/assistant/chat (Function Calling)
│   ├── ReportsController.cs        # Yêu cầu 4: GET /api/reports/bookings (Báo cáo đặt phòng)
│   └── AuthController.cs           # Cấp mã JWT Token mẫu cho Swagger UI Authorize
│
├── Models/
│   ├── Room.cs                     # Entity Room (RoomId, RoomNumber, RoomType, PricePerNight, MaxGuests)
│   ├── Booking.cs                  # Entity Booking (BookingId, RoomId, GuestName, Phone, CheckIn, CheckOut, TotalPrice, Status)
│   └── DTOs.cs                     # Toàn bộ Data Transfer Objects cho Request & Response
│
├── Repositories/
│   ├── SunriseDbContext.cs         # EF Core DbContext & cấu hình quan hệ, kiểu dữ liệu SQL
│   ├── IBookingRepository.cs       # Interface trừu tượng hóa các tác vụ nghiệp vụ & truy vấn DB
│   └── BookingRepository.cs        # Triển khai truy vấn LINQ, kiểm tra phòng trống & Seed Data tự động
│
├── Services/
│   ├── IGeminiService.cs           # Interface giao tiếp AI Gemini
│   └── GeminiService.cs            # Gọi HttpClient trực tiếp gemini-2.5-flash / gemini-3.5-flash-lite, JSON Schema & Function Calling
│
├── wwwroot/
│   ├── index.html                  # Trang chủ: Báo cáo đặt phòng sang trọng (Yêu cầu 4, 5)
│   ├── ask.html                    # Trang riêng: AI Lễ tân khách sạn (Yêu cầu 1)
│   ├── parse.html                  # Trang riêng: Bóc tách đặt phòng tự động JSON Mode (Yêu cầu 2)
│   ├── assistant.html              # Trang riêng: Trợ lý AI thực thi Function Calling (Yêu cầu 3)
│   └── css/
│       └── hotel.css               # Hệ thống thiết kế chuẩn 5 sao, độ cong khung đồng nhất 8px
│
├── database.sql                    # Script DDL tạo Database SUNRISEHOTEL, bảng và Seed Data
├── SunriseHotel_Postman_Collection.json # File Postman import test toàn bộ 5 API (kèm Auth JWT)
├── SunriseHotel.http               # File test HTTP requests tiện lợi kèm JWT Bearer
├── appsettings.json                # Cấu hình chuỗi kết nối SQL Server & JWT Secret Key
└── Program.cs                      # Cấu hình DI, Middleware, JWT Authentication, Swagger UI (Bearer) và Seed DB
```

---

## 2. BẢO MẬT GEMINI API KEY (BẮT BUỘC)

Tuyệt đối **KHÔNG hardcode API Key** trong mã nguồn hoặc tệp `appsettings.json`.

Hệ thống hỗ trợ đọc API Key theo thứ tự ưu tiên:
1. **User Secrets**: `dotnet user-secrets` (Khuyên dùng khi lập trình và chấm bài trên máy cá nhân).
2. **Biến môi trường (Environment Variable)**: `GEMINI_API_KEY` hoặc `Gemini:ApiKey`.
3. **IConfiguration**: Tự động bind từ User Secrets / Environment Variables vào `IConfiguration["Gemini:ApiKey"]` hoặc `IConfiguration["GEMINI_API_KEY"]`.

### Lệnh cấu hình API Key trước khi chạy:

#### Cách 1: Sử dụng .NET User Secrets (Khuyên dùng)
Mở Terminal tại thư mục gốc của dự án (`d:\baithi\baithi\baithi`) và chạy:
```powershell
dotnet user-secrets set "Gemini:ApiKey" "YOUR_ACTUAL_GEMINI_API_KEY"
```

#### Cách 2: Sử dụng Biến môi trường (PowerShell)
```powershell
$env:GEMINI_API_KEY="YOUR_ACTUAL_GEMINI_API_KEY"
```

#### Cách 3: Sử dụng Biến môi trường hệ thống (Command Prompt / Windows CMD)
```cmd
setx GEMINI_API_KEY "YOUR_ACTUAL_GEMINI_API_KEY"
```

---

## 3. CƠ SỞ DỮ LIỆU & SEED DATA (SUNRISEHOTEL)

### Tự động khởi tạo (Code First):
Khi ứng dụng khởi động (`dotnet run`), `Program.cs` sẽ tự động gọi:
```csharp
await bookingRepo.EnsureDatabaseCreatedAndSeededAsync();
```
Hệ thống sẽ tự động tạo Database `SUNRISEHOTEL` (nếu chưa có) và nạp sẵn 5 phòng + 3 đơn đặt phòng mẫu.

### File kịch bản SQL thủ công (`database.sql`):
Nếu cần chạy trực tiếp trong SQL Server Management Studio (SSMS) hoặc Azure Data Studio, file `database.sql` đã được tạo sẵn tại thư mục gốc:
- Bảng `Room`: 5 phòng (101, 102, 201, 202, 301).
- Bảng `Booking`: 3 đơn đặt phòng mẫu trạng thái `Confirmed` (RoomId = 3, 1, 5).

Chuỗi kết nối mặc định trong `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=SUNRISEHOTEL;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;"
}
```

---

## 4. CHI TIẾT SYSTEM INSTRUCTIONS, PROMPTS & SCHEMAS

### Yêu cầu 1: API askhotel (`POST /api/ai/ask`)
- **System Instruction**:
```
Bạn là nhân viên lễ tân lịch sự, chu đáo và chuyên nghiệp của khách sạn Sunrise Hotel.
Thông tin khách sạn Sunrise Hotel:
- Địa chỉ: 25 Trần Phú, Nha Trang.
- Giờ nhận phòng (check-in): từ 14:00.
- Giờ trả phòng (check-out): trước 12:00.
- Ăn sáng miễn phí: từ 6:30 đến 9:30 mỗi ngày.
- Miễn phí đón sân bay cho khách đặt hạng phòng Suite.
- Hồ bơi ngoài trời nằm ở tầng 5.
- Bảng giá các hạng phòng:
  + Phòng Single (Đơn): 600.000 VND / đêm (Tối đa 1 khách).
  + Phòng Double (Đôi): 950.000 VND / đêm (Tối đa 2 khách).
  + Phòng Suite (VIP): 2.200.000 VND / đêm (Tối đa 4 khách).

Quy tắc ứng xử:
1. Luôn chào đón khách niềm nở, lịch sự, đóng vai lễ tân khách sạn Sunrise Hotel.
2. CHỈ trả lời các câu hỏi liên quan đến Sunrise Hotel, phòng ốc, tiện ích và dịch vụ của khách sạn.
3. Nếu khách hỏi về các vấn đề ngoài khách sạn (như toán học, thời sự, lập trình, xã hội, chủ đề khác), hãy từ chối một cách khéo léo, lịch sự và hướng dẫn khách quay lại các thông tin của Sunrise Hotel.
```

---

### Yêu cầu 2: API parsebooking (`POST /api/bookings/parse`)
- **Prompt trích xuất**:
```
Bạn là trợ lý trích xuất dữ liệu đặt phòng tự động cho khách sạn Sunrise Hotel.
Nhiệm vụ: Trích xuất các trường thông tin đặt phòng từ văn bản của khách hàng thành JSON.

Quy tắc bắt buộc:
1. Quy đổi loại phòng về đúng 1 trong 3 giá trị:
   - 'phòng đơn' hoặc tương tự -> 'Single'
   - 'phòng đôi' hoặc tương tự -> 'Double'
   - 'phòng Suite' hoặc 'phòng VIP' hoặc tương tự -> 'Suite'
2. Định dạng ngày nhận phòng (checkIn) và ngày trả phòng (checkOut) BẮT BUỘC là 'yyyy-MM-dd'.
3. guestName: Họ tên khách hàng.
4. phone: Số điện thoại của khách hàng.
5. guests: Số lượng khách (kiểu số nguyên).

Văn bản từ khách hàng:
"""{text}"""
```

- **JSON Mode responseSchema**:
```json
{
  "type": "OBJECT",
  "properties": {
    "guestName": { "type": "STRING", "description": "Họ tên khách" },
    "phone": { "type": "STRING", "description": "Số điện thoại khách" },
    "roomType": { "type": "STRING", "enum": ["Single", "Double", "Suite"], "description": "Loại phòng chuẩn: Single, Double, hoặc Suite" },
    "checkIn": { "type": "STRING", "description": "Ngày nhận phòng định dạng yyyy-MM-dd" },
    "checkOut": { "type": "STRING", "description": "Ngày trả phòng định dạng yyyy-MM-dd" },
    "guests": { "type": "INTEGER", "description": "Số khách tối đa" }
  },
  "required": ["guestName", "phone", "roomType", "checkIn", "checkOut", "guests"]
}
```

- **Logic kiểm tra lịch trùng**:
```csharp
existing.CheckIn < req.CheckOut && req.CheckIn < existing.CheckOut
```

---

### Yêu cầu 3: API bookingassistant (`POST /api/assistant/chat`)
- **System Instruction**:
```
Bạn là trợ lý ảo AI thông minh và lịch thiệp của khách sạn Sunrise Hotel (25 Trần Phú, Nha Trang).
Bạn có nhiệm vụ giải đáp và thực hiện các yêu cầu đặt phòng của khách hàng thông qua việc sử dụng các công cụ (tools) được cung cấp:
1. Khi khách hỏi kiểm tra phòng trống hoặc hỏi giá phòng theo ngày, hãy gọi hàm CheckAvailability.
2. Khi khách yêu cầu tra cứu đơn đặt phòng theo số điện thoại, hãy gọi hàm GetBookingsByPhone.
3. Khi khách yêu cầu hủy đơn đặt phòng theo mã booking ID, hãy gọi hàm CancelBooking.

Hãy trả lời tự nhiên, thân thiện và đầy đủ thông tin cho khách dựa trên kết quả trả về từ công cụ.
```

- **Khai báo 3 Function Declarations**:
1. `CheckAvailability(roomType, checkIn, checkOut)`:
   - Tham số: `roomType` (STRING, Enum: Single/Double/Suite), `checkIn` (STRING, yyyy-MM-dd), `checkOut` (STRING, yyyy-MM-dd).
2. `GetBookingsByPhone(phone)`:
   - Tham số: `phone` (STRING).
3. `CancelBooking(bookingId)`:
   - Tham số: `bookingId` (INTEGER).

---

### Yêu cầu 4: API getbookingreport (`GET /api/reports/bookings?from=...&to=...`)
- Trả về danh sách đơn đặt phòng trạng thái `Confirmed`:
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
    "totalPrice": 1900000.00,
    "totalPriceFormatted": "1.900.000"
  }
]
```

---

## 5. HƯỚNG DẪN KHỞI CHẠY VÀ KIỂM THỬ

### Bước 1: Khởi động ứng dụng
Tại thư mục dự án, chạy lệnh:
```powershell
dotnet run
```
Ứng dụng sẽ lắng nghe tại: `http://localhost:5013` (hoặc cổng cấu hình trong `launchSettings.json`).

### Bước 2: Truy cập Giao diện Web (Yêu cầu 5)
Mở trình duyệt truy cập:
- 👉 **Trang chủ - Báo cáo đặt phòng**: `http://localhost:5013/` (hoặc `http://localhost:5013/index.html`)
  - Bảng báo cáo đặt phòng sang trọng theo chuẩn 5 sao với bộ lọc theo ngày, các chỉ số KPI, tự động tải dữ liệu từ CSDL.
- 👉 **Trang 1: AI Lễ Tân Khách Sạn (Yêu cầu 1: askhotel)**: `http://localhost:5013/ask.html`
  - Quầy lễ tân ảo chuyên biệt, tra cứu thông tin niêm yết của khách sạn và hỏi đáp 24/7 với Gemini 2.5 Flash.
- 👉 **Trang 2: Bóc Tách Đặt Phòng Tự Động (Yêu cầu 2: parsebooking)**: `http://localhost:5013/parse.html`
  - Giao diện bóc tách JSON Mode thông minh theo schema, kiểm tra trùng lịch phòng trống và tạo booking tức thì vào CSDL.
- 👉 **Trang 3: Trợ Lý Ảo Function Calling (Yêu cầu 3: bookingassistant)**: `http://localhost:5013/assistant.html`
  - Khung chat đàm thoại AI thông minh, điều phối và thực thi tự động 3 C# Tools vào CSDL.

### Bước 3: Truy cập Swagger API Documentation & Xác thực JWT Bearer
Mở trình duyệt truy cập:
👉 `http://localhost:5013/swagger`

Hệ thống đã tích hợp đầy đủ **JWT Bearer Authentication** vào Swagger UI:
1. Gọi API `POST /api/auth/token` trực tiếp trên Swagger UI (hoặc dùng mặc định `username: admin`, `password: 123456`) và bấm **Execute**.
2. Sao chép chuỗi mã `token` trả về trong phản hồi JSON.
3. Bấm vào nút màu xanh **Authorize** (có biểu tượng ổ khóa) ở góc trên bên phải giao diện Swagger.
4. Dán mã token vào ô (có thể dán dạng `Bearer <token>` hoặc chỉ cần dán chuỗi `<token>`) và nhấn **Authorize**.
5. Giờ đây toàn bộ các API gọi trên Swagger UI đều được đính kèm Authorization header hợp lệ!

### Bước 4: Kiểm thử bằng Postman hoặc VS Code (.http)
- **Postman**: Import file `SunriseHotel_Postman_Collection.json`.
- **VS Code / Visual Studio**: Mở file `SunriseHotel.http` và click **Send Request** (đã cấu hình sẵn biến `@authToken` tự động lấy token từ `POST /api/auth/token`).

