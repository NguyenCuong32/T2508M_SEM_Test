# Introduction to Dart Programming - Exam Paper (SET01)

Bài thi môn **Introduction to Dart Programming** (Thời gian: 60 phút | Điểm: 15)  
Sinh viên thực hiện: **Lê Trung Kiên**  
Lớp: **T2508M** (Học kỳ 2 - FPT Aptech)

---

## 1. Cấu trúc bài nộp và thang điểm (Marking Schema)

| Câu hỏi (Yêu cầu) | Điểm | Vị trí cài đặt mã nguồn | Mô tả chi tiết |
| :--- | :---: | :--- | :--- |
| **Create class Order** | **2** | `lib/order.dart` | Khai báo lớp `Order` với đầy đủ các thuộc tính: `item`, `itemName`, `price`, `currency`, `quantity`, constructor, `fromJson`, `toJson`, `formattedPrice` (e.g. `1.200`). |
| **Read string and save to objects, display list of orders to screen** | **5** | `lib/order.dart`<br>`bin/main.dart`<br>`web/index.html` | Hàm `parseOrdersFromString(jsonString)` giải mã chuỗi JSON ban đầu thành danh sách `List<Order>`. Hiển thị ra màn hình Console dạng bảng và hiển thị trực quan trên giao diện Website. |
| **Allows inserting an order into order.json** | **4** | `lib/order.dart`<br>`bin/main.dart`<br>`bin/server.dart`<br>`web/app.js` | Hỗ trợ thêm đơn hàng mới và lưu trực tiếp vào tệp `order.json` (thông qua hàm Dart và REST API `POST /api/orders` trên Web). |
| **Allows search Order by ItemName** | **3** | `lib/order.dart`<br>`bin/main.dart`<br>`web/app.js` | Hàm `searchOrdersByItemName(orders, query)` tìm kiếm không phân biệt hoa thường theo tên sản phẩm, cập nhật bảng kết quả tức thì trên Website. |
| **Bonus good UI & Tính năng nâng cao** | **1** | `web/style.css`<br>`web/index.html`<br>`web/app.js` | Giao diện chuẩn xác 100% theo hình chụp đề thi: tiêu đề vàng gold "My Order", form nhập 2 dòng, nút "Add Item to Cart" màu cam đất (#c25c38), bảng dữ liệu với icon thùng rác xóa đơn hàng, footer "Số 8, Tôn Thất Thuyết, Cầu giấy, Hà Nội".<br>✨ **Tính năng mở rộng:**<br>- **Sửa dữ liệu đơn hàng (Edit):** Bấm biểu tượng bút chì ✏️ trên từng dòng để sửa trực tiếp và lưu vào `order.json`.<br>- **Bộ lọc đa năng (Filters):** Lọc theo tiền tệ (USD, VND, EUR), lọc theo mức giá (< 1.300, 1.300 - 1.600, > 1.600).<br>- **Sắp xếp (Sorting):** Mặc định, mới nhất, tên A-Z / Z-A, giá tăng / giảm dần.<br>- **Chặn nhập chữ:** Ô Price và Quantity chỉ cho phép nhập số. |
| **Tổng điểm** | **15/15** | | |

---

## 2. Cấu trúc thư mục dự án

```text
T2508M_SEM_Test/
├── bin/
│   ├── main.dart             # Chương trình Dart Console chạy kiểm thử đầy đủ các yêu cầu đề thi
│   └── server.dart           # Web Server viết bằng Dart (dart:io) phục vụ Web & REST API order.json
├── lib/
│   └── order.dart            # Định nghĩa lớp Order, parsing JSON, tìm kiếm, lưu tệp order.json
├── test/
│   └── order_test.dart       # Bộ kiểm thử Unit Test cho lớp Order và các chức năng
├── web/
│   ├── index.html            # Giao diện Website My Order theo ảnh đề thi
│   ├── style.css             # CSS tùy chỉnh màu sắc chuẩn mẫu (terracotta #c25c38, gold #d49a3d)
│   ├── app.js                # Logic tương tác Web, gọi API lưu order.json, lọc tìm kiếm theo ItemName
│   └── main.dart             # Mã nguồn Dart Web client
├── order.json                # Tệp dữ liệu JSON lưu trữ các đơn hàng
├── pubspec.yaml              # Cấu hình dự án Dart SDK
└── README.md                 # Tài liệu hướng dẫn chi tiết
```

---

## 3. Hướng dẫn chạy chương trình

### Cách 1: Chạy chương trình Dart Console (Kiểm tra chức năng logic)
Mở terminal tại thư mục dự án và chạy:
```powershell
dart run bin/main.dart
```
Chương trình sẽ tự động thực hiện:
1. Đọc và parse chuỗi JSON ban đầu thành `List<Order>`.
2. Hiển thị bảng đơn hàng ra màn hình terminal.
3. Chèn một đơn hàng mới vào `order.json`.
4. Đọc lại dữ liệu từ `order.json` và hiển thị danh sách cập nhật.
5. Thực hiện tìm kiếm đơn hàng theo `ItemName` ("Iphone 15", "16") và in kết quả.

### Cách 2: Chạy Web Server Dart và xem Website giao diện
Chạy máy chủ Dart:
```powershell
dart run bin/server.dart
```
Sau đó mở trình duyệt web và truy cập:
👉 **http://localhost:8080**

Trên giao diện web:
- Nhập thông tin đơn hàng và bấm **"Add Item to Cart"** để thêm đơn hàng vào `order.json`.
- Gõ vào ô tìm kiếm **"Search Order by ItemName"** để lọc kết quả tức thì.
- Bấm vào biểu tượng **thùng rác** trên từng dòng để xóa đơn hàng khỏi `order.json`.
- Bấm **"order.json"** để tải về tệp JSON hiện tại.

### Cách 3: Mở trực tiếp file `web/index.html` (Không cần server)
Người chấm có thể nhấp đúp trực tiếp vào tệp `web/index.html` trên trình duyệt để trải nghiệm toàn bộ giao diện và chức năng (dữ liệu được tự động đồng bộ qua LocalStorage và cho phép tải xuống `order.json`).

### Cách 4: Chạy kiểm thử Unit Test
```powershell
dart run test/order_test.dart
```

---

## 4. Dữ liệu JSON mẫu từ đề thi
```json
[
  {
    "Item": "A1000",
    "ItemName": "Iphone 15",
    "Price": 1200,
    "Currency": "USD",
    "Quantity": 1
  },
  {
    "Item": "A1001",
    "ItemName": "Iphone 16",
    "Price": 1500,
    "Currency": "USD",
    "Quantity": 1
  }
]
```
