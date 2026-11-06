# SUNRISE HOTEL - AI BOOKING ASSISTANT
## Đề thi: Generative AI for .NET Developers with Google AI (SET02)

Hệ thống trợ lý đặt phòng khách sạn thông minh tích hợp Google Gemini AI và ASP.NET Core 8 Web API, sử dụng cơ sở dữ liệu MySQL trên Laragon (hỗ trợ quản lý qua phpMyAdmin).

---

## 1. Cấu Trúc Dự Án

```
T2508M_SEM_Test/
├── database.sql                             # Script khởi tạo cơ sở dữ liệu MySQL (Laragon / phpMyAdmin)
├── SunriseHotel_API.postman_collection.json # Bộ test Postman đầy đủ cho các API
├── SunriseHotel.sln                         # Solution Visual Studio
├── README.md                                # Tài liệu hướng dẫn, prompt, function declarations
└── SunriseHotel/
    ├── Controllers/
    │   ├── AiController.cs                  # Yêu cầu 1: POST /api/ai/ask (askhotel)
    │   ├── BookingsController.cs            # Yêu cầu 2: POST /api/bookings/parse (parsebooking)
    │   ├── AssistantController.cs           # Yêu cầu 3: POST /api/assistant/chat (bookingassistant)
    │   ├── ReportsController.cs             # Yêu cầu 4: GET /api/reports/bookings (getbookingreport)
    │   ├── RoomsController.cs               # GET /api/rooms
    │   └── ConfigController.cs              # Cấu hình API Key động từ giao diện
    ├── Services/
    │   ├── IGeminiService.cs & GeminiService.cs       # Tích hợp Gemini REST API, Function Calling, JSON Mode, xử lý 429
    │   ├── IBookingService.cs & BookingService.cs     # Nghiệp vụ đặt phòng, kiểm tra phòng trống, báo cáo
    │   └── IRoomService.cs & RoomService.cs           # Truy vấn dữ liệu danh mục phòng
    ├── Models/
    │   ├── Room.cs                          # Thực thể Phòng (RoomId, RoomNumber, RoomType, PricePerNight, MaxGuests)
    │   ├── Booking.cs                       # Thực thể Đặt phòng (BookingId, RoomId, GuestName, Phone, CheckIn, CheckOut, TotalPrice, Status)
    │   └── DTOs/                            # Data Transfer Objects
    ├── Data/
    │   ├── SunriseDbContext.cs              # EF Core DbContext kết nối MySQL (Pomelo)
    │   └── DbInitializer.cs                 # Tự động nạp dữ liệu mẫu ban đầu nếu bảng trống
    ├── wwwroot/                             # Yêu cầu 5: Giao diện Web (Tiếng Việt, border-radius 4px, nền sáng trắng, không gradient)
    │   ├── index.html
    │   ├── css/style.css
    │   └── js/app.js
    ├── appsettings.json
    └── Program.cs
```

---

## 2. Hướng Dẫn Cài Đặt Cơ Sở Dữ Liệu MySQL (Laragon / phpMyAdmin)

1. Mở phần mềm **Laragon**, nhấn **Start All** để khởi động Apache và MySQL.
2. Mở trình duyệt truy cập phpMyAdmin tại: `http://localhost/phpmyadmin`.
3. Nhập cơ sở dữ liệu:
   - Cách 1 (Qua phpMyAdmin): Nhấn vào tab **Import** (Nhập), chọn file `database.sql` tại thư mục gốc của dự án và nhấn **Go** (Thực hiện).
   - Cách 2 (Dòng lệnh MySQL):
     ```bash
     mysql -u root -p < database.sql
     ```
4. Cơ sở dữ liệu `SUNRISEHOTEL` sẽ được tạo bao gồm 2 bảng:
   - `Room`: 5 phòng khởi tạo (101 Single 600k, 102 Single 600k, 201 Double 950k, 202 Double 950k, 301 Suite 2.2tr).
   - `Booking`: Chứa các bản ghi đặt phòng mẫu ban đầu theo đề bài.

Chuỗi kết nối trong `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "server=localhost;port=3306;database=SUNRISEHOTEL;user=root;password=;CharSet=utf8mb4;"
}
```

---

## 3. Cách Bảo Vệ Và Cấu Hình Gemini API Key

Theo quy định đề thi, Gemini API Key tuyệt đối **không được hard-code** trong mã nguồn. Dự án hỗ trợ các cơ chế cấu hình an toàn:

### Cách 1: Sử dụng File `.env` (Khuyên dùng)
Dự án có sẵn file mẫu `.env.example`. Bạn có thể sao chép thành file `.env` tại thư mục gốc:
```env
GEMINI_API_KEY=AIzaSyYourActualGeminiApiKeyHere
```
File `.env` đã được đưa vào `.gitignore` để bảo mật tuyệt đối, không bị lộ khi commit mã nguồn.

### Cách 2: Nhập trực tiếp trên Giao Diện Web (Tự Động Lưu Vào `.env`)
Khi mở trang web tại `http://localhost:5000`, nhấn nút **Cài đặt API Key** ở góc trên bên phải, dán API Key vào và nhấn **Lưu API Key**. Hệ thống sẽ tự động ghi khóa này vào file `.env` để bảo lưu vĩnh viễn qua các lần chạy.

### Cách 3: Thiết lập qua Biến Môi Trường (Environment Variable)
Mở PowerShell và gán biến môi trường trước khi khởi chạy:
```powershell
$env:GEMINI_API_KEY="AIzaSyYourActualGeminiApiKeyHere"
```

### Cách 4: Sử dụng .NET User Secrets
```bash
dotnet user-secrets init
dotnet user-secrets set "Gemini:ApiKey" "AIzaSyYourActualGeminiApiKeyHere"
```

---

## 4. Danh Sách Prompts & Function Declarations (Yêu Cầu 1, 2, 3)

### Yêu Cầu 1: API `askhotel` (POST `/api/ai/ask`)
- **Mục đích**: Đóng vai lễ tân Sunrise Hotel, trả lời câu hỏi của khách hàng dựa trên thông tin chính xác của khách sạn và từ chối các câu hỏi không liên quan.
- **System Instruction**:
```text
Bạn là nhân viên lễ tân lịch sự, chu đáo và chuyên nghiệp của khách sạn Sunrise Hotel (địa chỉ: 25 Trần Phú, Hà Nội).
Thông tin chi tiết về khách sạn Sunrise Hotel:
- Địa chỉ: 25 Trần Phú, Hà Nội.
- Giờ nhận phòng (check-in): từ 14:00.
- Giờ trả phòng (check-out): trước 12:00.
- Bữa sáng: miễn phí từ 6:30 đến 9:30 hàng ngày.
- Tiện ích: Hồ bơi tại tầng 5.
- Dịch vụ đón tiễn sân bay: miễn phí đón tiễn sân bay Nội Bài dành riêng cho khách đặt phòng hạng Suite.
- Danh sách loại phòng và giá niêm yết:
  + Phòng Single (Đơn): 600.000 VND / đêm, sức chứa tối đa 1 khách (Phòng 101, 102).
  + Phòng Double (Đôi): 950.000 VND / đêm, sức chứa tối đa 2 khách (Phòng 201, 202).
  + Phòng Suite (Cao cấp): 2.200.000 VND / đêm, sức chứa tối đa 4 khách (Phòng 301, miễn phí đón sân bay).

QUY TẮC BẮT BUỘC:
- Chỉ trả lời các câu hỏi và thắc mắc liên quan trực tiếp đến thông tin và dịch vụ của Sunrise Hotel.
- Nếu khách hỏi về bất kỳ chủ đề nào không liên quan đến Sunrise Hotel (thời tiết bên ngoài, chính trị, địa điểm khác, kiến thức chung...), bạn phải lịch sự từ chối và giải thích rằng bạn là lễ tân của Sunrise Hotel nên chỉ hỗ trợ các thông tin về khách sạn.
- Sử dụng tiếng Việt chuẩn mực, lịch sự, hiếu khách, xưng hô 'Quý khách' và 'em' hoặc 'Sunrise Hotel'.
```

### Yêu Cầu 2: API `parsebooking` (POST `/api/bookings/parse`)
- **Mục đích**: Nhận câu thoại tự nhiên của khách hàng, trích xuất dữ liệu có cấu trúc JSON qua JSON Mode / Response Schema, sau đó C# kiểm tra tính khả dụng của phòng trống và lưu vào CSDL với trạng thái `Confirmed`.
- **System Instruction**:
```text
Bạn là trợ lý trích xuất dữ liệu đặt phòng của Sunrise Hotel. Nhiệm vụ của bạn là trích xuất thông tin đặt phòng từ câu nói tự nhiên của khách thành đúng định dạng JSON.
Định dạng JSON yêu cầu:
{
  "guestName": "Họ và tên khách",
  "phone": "Số điện thoại",
  "roomType": "Single" hoặc "Double" hoặc "Suite",
  "checkIn": "YYYY-MM-DD",
  "checkOut": "YYYY-MM-DD",
  "guests": số lượng khách (số nguyên)
}
Quy tắc:
- roomType chỉ được nhận một trong ba giá trị: 'Single', 'Double', 'Suite'.
- Nếu khách nói phòng đơn, map thành 'Single'. Phòng đôi, map thành 'Double'. Phòng VIP, gia đình, suite, map thành 'Suite'.
- checkIn và checkOut phải chuẩn YYYY-MM-DD. Nếu không nhắc đến năm, lấy năm 2026.
- guests là số nguyên dương tương ứng số người ở.
```
- **Response Schema cấu hình cho Gemini REST API**:
```json
{
  "type": "OBJECT",
  "properties": {
    "guestName": { "type": "STRING" },
    "phone": { "type": "STRING" },
    "roomType": { "type": "STRING", "enum": ["Single", "Double", "Suite"] },
    "checkIn": { "type": "STRING" },
    "checkOut": { "type": "STRING" },
    "guests": { "type": "INTEGER" }
  },
  "required": ["guestName", "phone", "roomType", "checkIn", "checkOut", "guests"]
}
```

### Yêu Cầu 3: API `bookingassistant` (POST `/api/assistant/chat`)
- **Mục đích**: Trợ lý trò chuyện tích hợp Function Calling, tự động quyết định gọi 3 hàm C# khi cần, thực thi truy vấn cơ sở dữ liệu và tổng hợp câu trả lời tự nhiên cho người dùng.
- **System Instruction**:
```text
Bạn là trợ lý ảo AI thông minh và tận tâm của Sunrise Hotel tại 25 Trần Phú, Hà Nội.
Nhiệm vụ của bạn là hỗ trợ khách hàng kiểm tra tình trạng phòng trống, tra cứu lịch sử đặt phòng bằng số điện thoại, và hỗ trợ hủy đặt phòng theo mã đặt phòng.
Bạn có quyền sử dụng 3 công cụ sau khi cần thiết:
1. CheckAvailability: Kiểm tra danh sách các phòng còn trống và giá theo loại phòng (Single, Double, Suite) và khoảng ngày check-in đến check-out (định dạng YYYY-MM-DD).
2. GetBookingsByPhone: Tra cứu danh sách booking của khách hàng theo số điện thoại.
3. CancelBooking: Hủy đặt phòng theo mã bookingId.

Quy tắc thực hiện:
- Khi khách hỏi phòng trống, hãy xác định loại phòng và ngày nhận/trả phòng rồi gọi hàm CheckAvailability. Nếu thiếu thông tin ngày, hãy hỏi khách nhẹ nhàng.
- Khi khách hỏi thông tin đặt phòng của họ, hãy gọi GetBookingsByPhone với số điện thoại khách cung cấp.
- Khi khách yêu cầu hủy booking (ví dụ: 'Hủy giúp tôi booking số 3'), hãy gọi hàm CancelBooking với bookingId tương ứng.
- Sau khi nhận được kết quả từ hàm, hãy trả lời khách hàng bằng tiếng Việt một cách tự nhiên, rõ ràng, lịch sự và chu đáo.
```
- **Khai báo 3 Function Declarations**:
```json
[
  {
    "name": "CheckAvailability",
    "description": "Return available rooms and price",
    "parameters": {
      "type": "OBJECT",
      "properties": {
        "roomType": { "type": "STRING", "description": "Type of room: Single, Double, Suite" },
        "checkIn": { "type": "STRING", "description": "Check-in date in YYYY-MM-DD format" },
        "checkOut": { "type": "STRING", "description": "Check-out date in YYYY-MM-DD format" }
      },
      "required": ["roomType", "checkIn", "checkOut"]
    }
  },
  {
    "name": "GetBookingsByPhone",
    "description": "Return the bookings of a guest",
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
        "bookingId": { "type": "INTEGER", "description": "Booking identifier" }
      },
      "required": ["bookingId"]
    }
  }
]
```

---

## 5. Báo Cáo Đặt Phòng & Giao Diện Website (Yêu Cầu 4 & 5)

- **API Báo cáo**: `GET /api/reports/bookings?from=2026-11-20&to=2026-11-30`
- **Các cột hiển thị theo chuẩn đề thi**:
  - `No` (STT)
  - `RoomNumber` (Số phòng)
  - `RoomType` (Loại phòng)
  - `GuestName` (Tên khách hàng)
  - `CheckIn` (Ngày nhận phòng: `dd/MM/yyyy`)
  - `CheckOut` (Ngày trả phòng: `dd/MM/yyyy`)
  - `Nights` (Số đêm)
  - `TotalPrice` (Tổng tiền định dạng VND: ví dụ `1.900.000`)
- **Giao diện Website**:
  - Truy cập tại: `http://localhost:5000`
  - Ngôn ngữ: 100% Tiếng Việt
  - Thiết kế: `border-radius: 4px`, nền sáng trắng (`#ffffff`, `#f8f9fa`), màu sắc cố định (`#1a73e8`, `#34a853`, `#dadce0`), không sử dụng gradient.
  - Tích hợp 5 tab tương tác: Báo cáo đặt phòng với bộ lọc từ ngày - đến ngày, Hỏi đáp khách sạn, Đặt phòng câu thoại, Trợ lý ảo AI Chatbot và Danh mục phòng CSDL.

---

## 6. Hướng Dẫn Khởi Chạy Ứng Dụng

1. Đảm bảo Laragon đang chạy MySQL (port 3306).
2. Mở terminal tại thư mục dự án và chạy:
   ```bash
   dotnet run --project .\SunriseHotel\SunriseHotel.csproj --urls "http://localhost:5000"
   ```
3. Mở trình duyệt truy cập:
   - Giao diện người dùng: `http://localhost:5000`
   - Tài liệu Swagger API: `http://localhost:5000/swagger`
4. Kiểm thử qua Postman:
   - Mở Postman -> Import file `SunriseHotel_API.postman_collection.json`.

---

## 7. Xử Lý Lỗi Và Tiêu Chuẩn Nâng Cao (Bonus Marks)

- **Cơ chế Retry khi gặp HTTP 429 Quota Exceeded**: Sử dụng hàm thử lại lũy tiến (exponential backoff) tự động đợi và gọi lại nếu vượt hạn ngạch API.
- **Tự động Fallback Model**: Hỗ trợ ưu tiên model thế hệ mới `gemini-3.5-flash-lite`, tự động fallback linh hoạt nếu cần.
- **Lưu trữ lịch sử chat cục bộ (localStorage)**: Tự động lưu toàn bộ hội thoại với Trợ lý ảo AI vào bộ nhớ trình duyệt, tải lại trang không bị mất lịch sử; hỗ trợ nút "Xóa lịch sử chat" với hộp thoại xác nhận SweetAlert2.
- **Hệ thống thông báo hiện đại**: Sử dụng SweetAlert2 và Toast thanh lịch thay thế hoàn toàn `alert()` mặc định của trình duyệt.
- **Kiểm tra phòng trống chống trùng lịch (Anti-Overbooking)**: Kiểm tra điều kiện `(CheckIn < newCheckOut && CheckOut > newCheckIn)` đối với các booking có trạng thái `Confirmed`.
- **Làm sạch định dạng JSON Markdown**: Xử lý trường hợp AI trả về markdown code blocks (```json ... ```) để parse dữ liệu chính xác 100%.
