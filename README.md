# BATTLEGAME - Developing Microsoft Azure Solutions
> **Dự án thực hành môn học:** Developing Microsoft Azure Solutions  
> **Lớp:** T2508M - Học kỳ 2  
> **Kiến trúc:** .NET 8 Isolated Azure Functions + SQL Server / Azure SQL + React Single-Page Application  

---

## 📁 Cấu trúc Thư mục Dự án

```
T2508M_SEM_Test/
├── database/
│   └── schema.sql              # Script SQL tạo CSDL BATTLEGAME, 3 bảng và dữ liệu mẫu
├── backend/
│   ├── BattleGame.Api.csproj   # File cấu hình .NET 8 Isolated Worker
│   ├── Program.cs              # Khởi tạo DI, Entity Framework Core DbContext
│   ├── host.json               # Cấu hình runtime Functions
│   ├── local.settings.json     # Connection string local & cấu hình CORS (*)
│   ├── Data/
│   │   └── BattleGameDbContext.cs
│   ├── Models/
│   │   ├── Player.cs           # Entity Player
│   │   ├── Asset.cs            # Entity Asset
│   │   └── PlayerAsset.cs      # Entity PlayerAsset (Junction table)
│   ├── DTOs/
│   │   ├── PlayerAssetReportDto.cs # DTO trả về cho API getassetsbyplayer
│   │   ├── RegisterPlayerRequest.cs
│   │   ├── CreateAssetRequest.cs
│   │   └── ApiResponse.cs
│   └── Functions/
│       ├── GetAssetsByPlayerFunction.cs  # GET  /api/getassetsbyplayer (JOIN 3 bảng)
│       ├── RegisterPlayerFunction.cs     # POST /api/registerplayer
│       ├── CreateAssetFunction.cs        # POST /api/createasset
│       └── AssignAssetFunction.cs        # POST /api/assignasset
├── frontend/
│   ├── index.html              # HTML shell (Vite)
│   ├── preview.html            # File HTML độc lập, mở chạy ngay trên trình duyệt (zero-setup)
│   ├── package.json            # Cấu hình React 18 & Vite
│   ├── vite.config.js
│   └── src/
│       ├── index.css           # Design tokens, dark mode & glassmorphism
│       ├── main.jsx            # React root mount
│       └── App.jsx             # Single-page: Bảng hiển thị, Loading/Error, Forms kiểm thử
└── DEPLOY_GUIDE.md             # Hướng dẫn chi tiết từng bước deploy lên Microsoft Azure Cloud
```

---

## 🚀 Hướng Dẫn Chạy Dự Án

### 1. Khởi tạo Cơ sở Dữ liệu (SQL Server)
1. Mở **SQL Server Management Studio (SSMS)** hoặc **Azure Data Studio**.
2. Kết nối tới SQL Server cục bộ (`(localdb)\mssqllocaldb` hoặc `localhost`).
3. Mở và thực thi toàn bộ script trong file:
   [`database/schema.sql`](file:///database/schema.sql)
4. Script sẽ tạo database `BATTLEGAME`, 3 bảng `Player`, `Asset`, `PlayerAsset` và chèn dữ liệu mock chuẩn để test API.

### 2. Chạy Backend (Azure Functions .NET 8)
1. Mở terminal tại thư mục `backend`:
   ```powershell
   cd backend
   dotnet build
   ```
2. Chạy Function App bằng Azure Functions Core Tools:
   ```powershell
   func start
   ```
   *Mặc định API sẽ lắng nghe tại `http://localhost:7071`.*

### 3. Chạy Frontend (React Single-Page)
- **Cách 1 (Khuyên dùng khi có Node.js):**
  ```powershell
  cd frontend
  npm install
  npm run dev
  ```
  Truy cập: `http://localhost:5173`

- **Cách 2 (Mở xem ngay không cần cài đặt node/npm):**
  Mở trực tiếp file [`frontend/preview.html`](file:///frontend/preview.html) bằng bất kỳ trình duyệt nào (Chrome, Edge, Firefox). Giao diện có sẵn bộ chuyển đổi giữa dữ liệu mẫu (Mock) và API backend thật.

---

## 🌐 Danh Sách API Đã Triển Khai

| Method | Endpoint | Mô tả |
|---|---|---|
| **GET** | `/api/getassetsbyplayer` | Truy vấn JOIN 3 bảng (`Player`, `PlayerAsset`, `Asset`), trả về mảng JSON gồm: `No`, `PlayerName`, `Level`, `Age`, `AssetName`. |
| **POST** | `/api/registerplayer` | Nhận JSON đăng ký Player (tự sinh GUID `PlayerId` nếu không truyền). |
| **POST** | `/api/createasset` | Nhận JSON tạo mới trang bị Asset (tự sinh GUID `AssetId`). |
| **POST** | `/api/assignasset` | Gán một trang bị (Asset) cho người chơi (Player) vào bảng `PlayerAsset`. |

---

## ☁️ Hướng Dẫn Deploy Lên Azure Cloud
Xem tài liệu hướng dẫn chi tiết tại:  
👉 [`DEPLOY_GUIDE.md`](file:///DEPLOY_GUIDE.md)
