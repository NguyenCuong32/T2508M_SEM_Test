# Sunrise Hotel - Generative AI for .NET Developers (SET02)

Dự án bài thi thực hành: **Generative AI for .NET Developers with Google AI – SET02**  
Tên dự án: **SunriseHotel**  
Lớp: **T2508M** | Học kỳ: **Học kỳ 2**

---

## 1. Project Overview (Tổng quan dự án)
**SunriseHotel** là hệ thống quản lý khách sạn thông minh tích hợp trí tuệ nhân tạo **Google Gemini AI** trên nền tảng **ASP.NET Core Web API** và **MySQL**, kết hợp giao diện Web **React**. Dự án giải quyết các bài toán hiện đại:
- Hỏi đáp thông tin khách sạn tự động qua System Instruction nghiêm ngặt.
- Trích xuất thông tin đặt phòng tự nhiên sang định dạng chuẩn JSON Mode và tự động xử lý nghiệp vụ, kiểm tra phòng trống và lưu vào database.
- Trợ lý đặt phòng thông minh sử dụng **Gemini Function Calling (Tools)** tương tác 2 chiều với dữ liệu thực tế trong MySQL (tra cứu phòng, tìm kiếm đơn đặt phòng theo số điện thoại, hủy phòng).
- Báo cáo thống kê doanh thu và lượt đặt phòng theo khoảng thời gian.
- Giao diện Dashboard React trực quan, thân thiện, có khả năng tương tác trực tiếp.

---

## 2. Technologies (Công nghệ sử dụng)
- **Backend**: .NET 9 (tương thích hoàn toàn .NET 8), C#, ASP.NET Core Web API.
- **ORM / Database**: Entity Framework Core 9.0, Pomelo.EntityFrameworkCore.MySql, MySQL 8.x / MariaDB.
- **AI Integration**: Google Gemini 2.5 Flash API (`gemini-2.5-flash`), Gemini REST API v1beta (System Instruction, JSON Mode, Function Calling Tools).
- **Frontend**: React 18, React DOM, Babel, Bootstrap 5.3, Bootstrap Icons.
- **Kiến trúc**: Clean Code, Dependency Injection, Service Layer, Async/Await, DTO pattern.

---

## 3. Database Setup (Cấu hình Cơ sở dữ liệu)

### Tên Database
`SUNRISEHOTEL`

### Bảng `Room`
| Cột | Kiểu dữ liệu | Ràng buộc |
|---|---|---|
| `RoomId` | INT | Primary Key, Auto Increment |
| `RoomNumber` | VARCHAR(10) | Not Null |
| `RoomType` | VARCHAR(32) | Not Null (Single, Double, Suite) |
| `PricePerNight` | DECIMAL(18,2) | Not Null |
| `MaxGuests` | INT | Not Null |

### Bảng `Booking`
| Cột | Kiểu dữ liệu | Ràng buộc |
|---|---|---|
| `BookingId` | INT | Primary Key, Auto Increment |
| `RoomId` | INT | Foreign Key references `Room(RoomId)` |
| `GuestName` | VARCHAR(128) | Not Null |
| `Phone` | VARCHAR(20) | Not Null |
| `CheckIn` | DATE | Not Null |
| `CheckOut` | DATE | Not Null |
| `TotalPrice` | DECIMAL(18,2) | Not Null |
| `Status` | VARCHAR(16) | Not Null (`Confirmed`, `Cancelled`) |

### Script khởi tạo Database & Seed Data (MySQL)
Dự án đã có sẵn file script [`database_setup.sql`](file:///Users/hcc/Desktop/T2508M_SEM_Test/T2508M_SEM_Test/database_setup.sql). Bạn có thể chạy trực tiếp trong MySQL Workbench / DBeaver:

```sql
CREATE DATABASE IF NOT EXISTS `SUNRISEHOTEL` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `SUNRISEHOTEL`;

CREATE TABLE IF NOT EXISTS `Room` (
    `RoomId` INT NOT NULL AUTO_INCREMENT,
    `RoomNumber` VARCHAR(10) NOT NULL,
    `RoomType` VARCHAR(32) NOT NULL,
    `PricePerNight` DECIMAL(18,2) NOT NULL,
    `MaxGuests` INT NOT NULL,
    PRIMARY KEY (`RoomId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `Booking` (
    `BookingId` INT NOT NULL AUTO_INCREMENT,
    `RoomId` INT NOT NULL,
    `GuestName` VARCHAR(128) NOT NULL,
    `Phone` VARCHAR(20) NOT NULL,
    `CheckIn` DATE NOT NULL,
    `CheckOut` DATE NOT NULL,
    `TotalPrice` DECIMAL(18,2) NOT NULL,
    `Status` VARCHAR(16) NOT NULL,
    PRIMARY KEY (`BookingId`),
    CONSTRAINT `FK_Booking_Room_RoomId` FOREIGN KEY (`RoomId`) REFERENCES `Room` (`RoomId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO `Room` (`RoomId`, `RoomNumber`, `RoomType`, `PricePerNight`, `MaxGuests`) VALUES
(1, '101', 'Single', 600000.00, 1),
(2, '102', 'Single', 600000.00, 1),
(3, '201', 'Double', 950000.00, 2),
(4, '202', 'Double', 950000.00, 2),
(5, '301', 'Suite', 2200000.00, 4)
ON DUPLICATE KEY UPDATE `RoomNumber` = VALUES(`RoomNumber`);
```

---

## 4. API Endpoints (Danh sách Endpoints)

| HTTP Method | Route | Mô tả chức năng | Requirement |
|---|---|---|---|
| `POST` | `/api/ai/ask` | Hỏi đáp thông tin khách sạn với lễ tân AI | Req 1 |
| `POST` | `/api/bookings/parse` | Đặt phòng bằng ngôn ngữ tự nhiên, trích xuất JSON & lưu DB | Req 2 |
| `POST` | `/api/assistant/chat` | Trợ lý đặt phòng thông minh với Gemini Function Calling | Req 3 |
| `GET` | `/api/reports/bookings?from=...&to=...` | Báo cáo danh sách đặt phòng và doanh thu | Req 4 |

---

## 5. Prompts sử dụng trong Requirement 1 (Ask Hotel API)

### System Instruction:
```text
You are a polite, professional receptionist at Sunrise Hotel.
Only answer questions related to Sunrise Hotel based strictly on the following hotel information:
- Hotel Name: Sunrise Hotel
- Address: 25 Tran Phu, Nha Trang
- Check-in time: from 14:00 (2:00 PM)
- Check-out time: before 12:00 (12:00 PM)
- Complimentary/Free breakfast: served from 06:30 to 09:30
- Free airport pick-up: exclusively provided for Suite rooms
- Swimming pool: located on the 5th floor
- Room Types & Pricing:
  * Single: 600,000 VND / night, max 1 guest
  * Double: 950,000 VND / night, max 2 guests
  * Suite: 2,200,000 VND / night, max 4 guests

Guidelines:
1. Always maintain a courteous, welcoming, and helpful receptionist tone.
2. Answer in the same language as the customer's question (e.g. Vietnamese or English).
3. Strictly decline questions that are not related to Sunrise Hotel, its services, or hospitality inquiries, politely explaining that you can only assist with Sunrise Hotel inquiries.
```

---

## 6. JSON Schema và Prompt trong Requirement 2 (Parse Booking API)

### Gemini Generation Config:
```json
{
  "response_mime_type": "application/json"
}
```

### Extraction System Instruction:
```text
You are a hotel booking data extraction system.
Extract booking details from natural language text into a JSON object matching this exact schema:
{
  "guestName": "Full name of the guest",
  "phone": "Phone number",
  "roomType": "Single" | "Double" | "Suite",
  "checkIn": "YYYY-MM-DD",
  "checkOut": "YYYY-MM-DD",
  "guests": integer
}

Rules:
1. roomType must strictly be normalized to one of: "Single", "Double", "Suite".
2. Convert all dates to standard "YYYY-MM-DD" format.
3. Return ONLY valid JSON.
```

### 12 Tiêu chí nghiệp vụ được kiểm tra nghiêm ngặt tại C# Backend:
1. Nhận natural language input.
2. Gửi request đến Gemini trích xuất dữ liệu.
3. Nhận JSON từ Gemini.
4. Deserialize an toàn sang C# object `ParsedBookingData`.
5. Validate đầy đủ dữ liệu (họ tên, SĐT, ngày tháng).
6. Validate `CheckOut > CheckIn`.
7. Validate `roomType` hợp lệ (`Single`, `Double`, `Suite`).
8. Validate số khách không vượt `MaxGuests` (`Single` <= 1, `Double` <= 2, `Suite` <= 4).
9. Truy vấn kiểm tra phòng còn trống trong toàn bộ thời gian:
   `existing.CheckIn < newCheckOut && existing.CheckOut > newCheckIn` với booking `Status == Confirmed`.
10. Tính `TotalPrice = (CheckOut - CheckIn) * PricePerNight`.
11. Gán phòng trống tìm được và lưu booking vào MySQL với `Status = "Confirmed"`.
12. Trả về `ParseBookingResponse` hoàn chỉnh cho client.

---

## 7. Function Declarations trong Requirement 3 (Booking Assistant API)

Khai báo đúng 3 functions trong `tools`:

### Function 1: `CheckAvailability`
- **Description**: `Return available rooms and price.`
- **Parameters**: `roomType` (string), `checkIn` (string), `checkOut` (string). All required.

### Function 2: `GetBookingsByPhone`
- **Description**: `Return the bookings of a guest.`
- **Parameters**: `phone` (string). Required.

### Function 3: `CancelBooking`
- **Description**: `Change booking status to Cancelled.`
- **Parameters**: `bookingId` (integer). Required.

---

## 8. Function Calling Workflow (Quy trình 10 bước)

```
[ Người dùng gửi Message ]
            │
            ▼ (1)
[ Backend C# gửi Message + Function Declarations đến Gemini ]
            │
            ▼ (2)
[ Gemini phân tích và trả về yêu cầu functionCall(name, args) ]
            │
            ▼ (3)
[ Backend C# phân tích function name & parameters ]
            │
            ▼ (4)
[ C# thực thi logic nghiệp vụ thực tế với MySQL Database ]
            │
            ▼ (5)
[ Nhận kết quả từ MySQL, đóng gói thành Function Response ]
            │
            ▼ (6)
[ Gửi Turn 2: functionResponse trở lại Gemini ]
            │
            ▼ (7)
[ Gemini nhận dữ liệu thực tế và tổng hợp câu trả lời tự nhiên ]
            │
            ▼ (8)
[ Trả lời phản hồi cuối cùng cho Người dùng ]
```

---

## 9. Hướng dẫn cấu hình Gemini API Key

1. Đăng nhập vào [Google AI Studio](https://aistudio.google.com/).
2. Nhấn **Get API Key** và tạo một API Key mới.
3. Không chia sẻ hoặc push API Key này lên Git repository công khai.

---

## 10. Bảo mật API Key (Security Best Practices)

Tuyệt đối **không hard-code API Key** vào source code hoặc commit file cấu hình chứa key thật lên Git.

### Cách 1: Sử dụng .NET User Secrets (Khuyến nghị cho môi trường Development)
Trong thư mục project, chạy lệnh:
```bash
dotnet user-secrets init
dotnet user-secrets set "Gemini:ApiKey" "AIzaSyYourSecretKeyHere..."
```

### Cách 2: Sử dụng Environment Variable
Thiết lập biến môi trường hệ thống:
- **macOS / Linux**:
  ```bash
  export GEMINI_API_KEY="AIzaSyYourSecretKeyHere..."
  ```
- **Windows (PowerShell)**:
  ```powershell
  $env:GEMINI_API_KEY="AIzaSyYourSecretKeyHere..."
  ```

Hệ thống đã được lập trình sẵn để tự động đọc key từ:
1. `IConfiguration["Gemini:ApiKey"]` (User Secrets hoặc appsettings).
2. Biến môi trường `GEMINI_API_KEY`.

---

## 11. Hướng dẫn chạy project (Run Application)

### Bước 1: Cấu hình Connection String
Mở file `appsettings.json` và điều chỉnh thông tin đăng nhập MySQL của máy bạn:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=SUNRISEHOTEL;User=root;Password=your_password;"
}
```

### Bước 2: Chạy script database
Chạy script `database_setup.sql` trong MySQL để tạo database và 5 phòng ban đầu.

### Bước 3: Khởi chạy Web API & React Frontend
```bash
dotnet restore
dotnet run
```
Mở trình duyệt truy cập:
- Giao diện Dashboard React: **`http://localhost:5000/`** (hoặc port được hiển thị trên console).
- OpenAPI / Swagger: **`http://localhost:5000/openapi/v1.json`**.

---

## 12. Hướng dẫn kiểm thử (Testing Guide)

Dự án có sẵn file [`SunriseHotel.http`](file:///Users/hcc/Desktop/T2508M_SEM_Test/T2508M_SEM_Test/SunriseHotel.http) để test nhanh trong VS Code (Extension REST Client) hoặc Visual Studio.

### Test cURL mẫu:

#### 1. Ask Hotel API:
```bash
curl -X POST http://localhost:5000/api/ai/ask \
  -H "Content-Type: application/json" \
  -d '{"question": "Does the hotel have a swimming pool?"}'
```

#### 2. Parse Booking API:
```bash
curl -X POST http://localhost:5000/api/bookings/parse \
  -H "Content-Type: application/json" \
  -d '{"text": "Tôi là Nguyễn Văn An, sđt 0901234567, muốn đặt 1 phòng đôi từ 20/11/2026 đến 22/11/2026 cho 2 người"}'
```

#### 3. Assistant Chat (Function Calling):
```bash
curl -X POST http://localhost:5000/api/assistant/chat \
  -H "Content-Type: application/json" \
  -d '{"message": "Còn phòng Suite nào trống từ 24/12 đến 26/12 không?"}'
```

#### 4. Booking Report API:
```bash
curl -X GET "http://localhost:5000/api/reports/bookings?from=2026-01-01&to=2026-12-31"
```
