# Sunrise Hotel — AI Booking Assistant

Ứng dụng ASP.NET Core (.NET 10), MySQL 8.4, Gemini REST API, sử dụng controller, service và repository pattern. Controllers xử lý HTTP; services xử lý nghiệp vụ và Gemini; các repository Booking, Room, Report truy cập MySQL thông qua interface tương ứng.

## Chạy bằng Docker

Cần Docker Desktop đang chạy. Tại thư mục dự án, tạo `.env` từ `.env.example` nếu chưa có, điền `GEMINI_API_KEY` hợp lệ, rồi chạy:

```sh
docker compose up -d --build
```

Mở http://localhost:8080 để xem báo cáo. MySQL lắng nghe tại `localhost:3307`, database `SUNRISEHOTEL`, user `root`, mật khẩu trong `.env`. Script `database/init.sql` tự tạo bảng và 5 phòng khi volume được khởi tạo lần đầu. Dữ liệu lưu trong Docker volume `hotel_data`; muốn tạo lại từ đầu có thể chạy `docker compose down -v` (lệnh này xóa toàn bộ dữ liệu), sau đó chạy lại `up`. Đổi script sau khi volume đã tồn tại sẽ không tự chạy lại.

## API và kiểm thử

Import `SunriseHotel.postman_collection.json` vào Postman; mặc định base URL là `http://localhost:8080`.

- `POST /api/ai/ask`: `{ "question": "Khách sạn có hồ bơi không?" }`.
- `POST /api/bookings/parse`: `{ "text": "Tôi là Nguyễn Văn An, sđt 0901234567, muốn đặt 1 phòng đôi từ 20/11/2026 đến 22/11/2026 cho 2 người" }`. Thành công trả về `parsed` và `booking`; hết phòng trả 409. Yêu cầu này thực sự tạo booking.
- `POST /api/assistant/chat`: `{ "message": "Còn phòng Suite nào trống từ 2026-12-24 đến 2026-12-26 không?" }`. Lệnh hủy qua trợ lý thực sự cập nhật booking.
- `GET /api/reports/bookings?from=2026-11-01&to=2026-11-30`: các booking Confirmed có ngày nhận phòng trong khoảng bao gồm cả hai đầu. Giao diện dùng cùng API.

Khoảng đặt phòng được xem là `[checkIn, checkOut)`, do đó khách mới có thể nhận phòng cùng ngày khách trước trả phòng. Tạo booking khóa các phòng phù hợp trong transaction trước khi kiểm tra trùng lịch.

## Prompt và function declarations

Mã nguồn đầy đủ ở `Services/GeminiService.cs`:

1. **Ask hotel**: lễ tân lịch sự, chỉ trả lời về Sunrise Hotel, từ chối chủ đề khác; cung cấp địa chỉ 25 Trần Phú, Nha Trang, giờ nhận/trả phòng 14:00/12:00, bữa sáng 6:30–9:30, đón sân bay miễn phí cho Suite và hồ bơi tầng 5; không bịa thông tin.
2. **Parse booking**: trích xuất thông tin theo schema JSON, ánh xạ phòng đơn/đôi/suite sang Single/Double/Suite, ngày YYYY-MM-DD; không suy đoán dữ liệu thiếu. Gemini JSON mode dùng `responseMimeType: application/json` và schema gồm `guestName`, `phone`, `roomType`, `checkIn`, `checkOut`, `guests`; ứng dụng deserialize và xác thực trước khi lưu.
3. **Booking assistant**: dùng thông tin và quy tắc lễ tân trên, yêu cầu hỏi lại nếu thiếu thông tin và chỉ báo thành công khi công cụ xác nhận. Khai báo `CheckAvailability(roomType: string, checkIn: string, checkOut: string)`, `GetBookingsByPhone(phone: string)`, `CancelBooking(bookingId: integer)`. Khi Gemini yêu cầu, C# thực thi qua repository và gửi `functionResponse` để Gemini tạo câu trả lời.

## Bảo vệ khóa

Khóa chỉ được truyền phía máy chủ qua biến môi trường `GEMINI_API_KEY`, không nằm trong source hoặc trình duyệt; `.env` được `.gitignore` và `.dockerignore` loại trừ. Không commit hoặc nộp `.env`; đề yêu cầu xóa khóa trước khi nộp. Nếu khóa đã bị lộ, hãy thu hồi và tạo khóa mới. `GEMINI_MODEL` có thể đổi sang Flash model tương thích. Chức năng Gemini cần API key Google AI Studio hợp lệ và dịch vụ Gemini có thể truy cập; lỗi quota 429 được trả rõ ràng.

Đây là bài tập: các API tra cứu/hủy theo số điện thoại hoặc mã chưa có xác thực người dùng. Cần bổ sung xác thực và phân quyền trước khi dùng với dữ liệu khách thật.

## Giao diện React

Mã nguồn giao diện nằm trong `client/`: `src/pages` chứa từng chức năng, `src/components` chứa các thành phần dùng chung và `src/services` gọi API. Docker tự build React bằng Vite và phục vụ bản build từ ứng dụng ASP.NET tại cổng 8080.

Để phát triển giao diện riêng, khởi động backend và MySQL bằng Docker, sau đó chạy `cd client`, `npm ci`, `npm run dev` và mở http://localhost:5173. Vite chuyển các yêu cầu `/api` tới backend tại cổng 8080. Sau khi sửa mã, chạy `docker compose up -d --build` để cập nhật bản Docker.
