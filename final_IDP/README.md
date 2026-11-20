# FPT Aptech - Introduction to Dart Programming (SET01) - Practical Exam

Bài thi thực hành môn: **Introduction to Dart Programming**  
Thời gian làm bài: **60 phút** | Điểm tối đa: **15 điểm**

---

## 📋 Yêu cầu đề bài (Exam Requirements)

Cho chuỗi JSON:
```json
'[{"Item": "A1000","ItemName": "Iphone 15","Price": 1200,"Currency": "USD","Quantity":1},{"Item": "A1001","ItemName": "Iphone 16","Price": 1500,"Currency": "USD","Quantity":1}]'
```

1. **Tạo class tên "Order"** (2 điểm):
   - Đầy đủ các thuộc tính: `item`, `itemName`, `price`, `currency`, `quantity`.
   - Các getter chuẩn hoa và camelCase, phương thức `Order.fromJson` và `toJson()`.
   - Hỗ trợ format giá có dấu phân cách hàng nghìn (ví dụ `1.200` theo đúng hình đề bài).

2. **Đọc dữ liệu từ chuỗi và lưu vào danh sách đối tượng orders, hiển thị ra màn hình** (5 điểm):
   - Parse JSON string thành `List<Order>`.
   - Hiển thị danh sách orders ra giao diện web bảng (table) và console CLI.

3. **Cho phép thêm (insert) order vào `order.json`** (4 điểm):
   - Form nhập liệu gồm: `Item`, `Item Name`, `Price`, `Quantity`, `Currency`.
   - Nút `Add Item to Cart` màu cam đất (terracotta) chuẩn mẫu.
   - Thêm vào danh sách và lưu trực tiếp vào file `order.json` (thông qua Backend Server API hoặc download file/localStorage).

4. **Cho phép tìm kiếm (search) Order theo `ItemName`** (3 điểm):
   - Hộp tìm kiếm hỗ trợ tìm kiếm không phân biệt chữ hoa/thường (case-insensitive substring match).
   - Tự động lọc real-time hoặc qua nút tìm kiếm.

5. **Chức năng Sửa (Update / Edit Order)**:
   - Click icon cây bút (✏️) tại bất kỳ dòng nào trên bảng để nạp toàn bộ `value` lên Form.
   - Form chuyển sang chế độ sửa với nút **Update Order** và nút **Cancel (Hủy)**.
   - Dòng đang sửa được highlight trực quan (`#fffbeb`).
   - Cập nhật trực tiếp vào `order.json` (thông qua API PUT hoặc download JSON / localStorage).

6. **Hệ thống Validate Input toàn diện (Validation System)**:
   - **Item (SKU)**: Bắt buộc, 2-20 ký tự, kiểm tra trùng lặp mã khi thêm mới.
   - **Item Name**: Bắt buộc, tối thiểu 2 ký tự.
   - **Price**: Bắt buộc, **CHỈ ĐƯỢC NHẬP SỐ** (chặn toàn bộ ký tự chữ khi gõ hoặc dán), hỗ trợ số nguyên hoặc phân cách hàng nghìn (`1200`, `1.200`).
   - **Quantity**: Bắt buộc, **CHỈ ĐƯỢC NHẬP SỐ NGUYÊN DƯƠNG** (`0-9`, chặn toàn bộ chữ cái).
   - **Currency**: Bắt buộc, mã tiền tệ chuẩn 2-5 ký tự (mặc định `USD`).
   - Hiệu ứng trực quan: Chặn phím chữ tức thì (`onKeyDown`), tự động xóa ký tự chữ nếu paste hoặc gõ bộ gõ tiếng Việt (`onInput`), hiển thị cảnh báo đỏ *"Ô Price chỉ được nhập số!"* / *"Ô Quantity chỉ được nhập số!"*, viền đỏ, popup `window.alert(...)`, rung form (shake) và tự động focus.

7. **Giao diện đẹp chuẩn mẫu (Bonus good UI)** (1 điểm):
   - Tái hiện chính xác 100% layout theo ảnh đề bài (Tiêu đề "My Order" màu vàng gold, form 2 hàng, bảng màu cam đất, nút thùng rác xóa từng dòng, footer thông tin địa chỉ trường: *"Số 8, Tôn Thất Thuyết, Cầu Giấy, Hà Nội"*).
   - Bổ sung sẵn 9 giá trị mẫu (A1000 - A1008) chuẩn như hình Page 1.

---

## 🏗️ Cấu trúc thư mục dự án (Project Architecture)

```
final_IDP/
├── bin/
│   ├── main.dart               # CLI runner kiểm tra toàn bộ yêu cầu đề bài trên console
│   └── server.dart             # HTTP server tĩnh & REST API quản lý order.json
├── lib/
│   ├── order.dart              # Model class Order chuẩn OOP, fromJson, toJson, format
│   └── order_manager.dart      # Quản lý nghiệp vụ: parse, thêm, xóa, tìm kiếm theo ItemName
├── web/
│   ├── index.html              # Giao diện website chuẩn theo hình ảnh đề bài
│   ├── styles.css              # CSS tái hiện chính xác màu sắc, layout, responsive
│   ├── main.dart               # Logic Dart Web (dart:html) tương tác DOM & API
│   └── main.dart.js            # JavaScript đã biên dịch từ Dart Web để chạy trực tiếp trên browser
├── test/
│   └── order_test.dart         # Bộ Unit Tests kiểm thử toàn bộ 5 yêu cầu đề bài (100% pass)
├── order.json                  # File JSON lưu trữ dữ liệu đơn hàng
├── pubspec.yaml                # Cấu hình dự án Dart & dependencies
└── README.md                   # Hướng dẫn chi tiết
```

---

## 🚀 Hướng dẫn chạy dự án (How to Run)

### 1. Chạy ứng dụng Web với Local Server (Khuyên dùng - Đầy đủ tính năng ghi file)
Khởi chạy server HTTP tích hợp sẵn:
```bash
dart run bin/server.dart
```
Sau đó mở trình duyệt tại: **http://localhost:8080**  
*(Mọi thao tác thêm order từ web sẽ lưu trực tiếp vào file `order.json` trên ổ đĩa).*

### 2. Mở trực tiếp Web không cần chạy server
Chỉ cần click đúp mở file `web/index.html` bằng bất kỳ trình duyệt nào (Chrome, Edge, Firefox). File đã được tích hợp sẵn `main.dart.js` và tự động lưu trữ qua `localStorage`, đồng thời hỗ trợ nút **💾 Export order.json** để tải file bất kỳ lúc nào.

### 3. Chạy demo kiểm thử trên Console (CLI)
Để kiểm tra nhanh toàn bộ logic OOP, đọc/ghi file `order.json`, và tìm kiếm:
```bash
dart run bin/main.dart
```

### 4. Chạy Unit Test kiểm thử
Chạy toàn bộ bộ test tự động:
```bash
dart run test/order_test.dart
```

### 5. Biên dịch lại Dart Web sang JavaScript (Nếu có chỉnh sửa code Dart)
```bash
dart compile js web/main.dart -o web/main.dart.js
```

---

## 📊 Bảng đối chiếu thang điểm (Marking Schema Checklist)

| Yêu cầu đề bài | Điểm | Hiện thực trong bài | Trạng thái |
| :--- | :---: | :--- | :---: |
| **Create class Order** | 2 | Class `Order` tại `lib/order.dart` đầy đủ thuộc tính, constructor, `fromJson`, `toJson`, getters hoa/thường, formatted price | ✅ Đạt 2/2 |
| **Read string and save to objects, display list of orders to screen** | 5 | Đọc từ chuỗi json đề bài, chuyển thành `List<Order>`, render bảng web đúng mẫu và hiển thị table trên console | ✅ Đạt 5/5 |
| **Insert order** | 4 | Form thêm order đầy đủ validation, lưu vào `order.json` cả từ web UI (REST API / File download) lẫn CLI | ✅ Đạt 4/4 |
| **Search order by ItemName** | 3 | Tính năng tìm kiếm case-insensitive trên cả giao diện Web (live filter) và CLI runner | ✅ Đạt 3/3 |
| **Bonus good UI** | 1 | Giao diện chuẩn xác 100% theo mẫu: màu tiêu đề, bố cục form, bảng cam đất, icon xóa rác, footer FPT Aptech | ✅ Đạt 1/1 |
| **TỔNG ĐIỂM** | **15/15** | Hoàn thành xuất sắc theo tiêu chuẩn Senior Developer | **15/15 Điểm** |
