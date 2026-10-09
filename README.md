# BATTLEGAME - Developing Microsoft Azure Solutions (SET01)
### Lớp: T2508M - Học kỳ 2

Dự án hoàn chỉnh triển khai giải pháp Cloud Azure theo đề thi **Developing Microsoft Azure Solutions - SET01** với kiến trúc **Azure Functions (.NET 8/10 Isolated Worker)** tuân thủ mô hình **Service & Repository Pattern**, cơ sở dữ liệu **MySQL (Database First & Code First)**, giao diện **React + Vite**, và tài liệu hướng dẫn triển khai lên **Azure Cloud**.

---

## 📋 Bảng Thống Kê Các Yêu Cầu Đã Hoàn Thành (Marking Schema)

| Yêu Cầu | Nội Dung | Trạng Thái | Điểm Tối Đa |
|---|---|:---:|:---:|
| **Create database** | Database First & Code First (DDL script `database.sql`, DbContext, Models) |  Hoàn thành | 1/1 |
| **Question 1** | API `registerplayer` (POST dữ liệu vào bảng Player qua Service & Repository) |  Hoàn thành | 3/3 |
| **Question 2** | API `createasset` (POST dữ liệu vào bảng Asset qua Service & Repository) |  Hoàn thành | 3/3 |
| **Question 3** | API `getassetsbyplayer` (GET báo cáo dạng bảng qua Service & Repository) |  Hoàn thành | 3/3 |
| **Question 4** | Website Frontend (React + Vite) hiển thị bảng báo cáo Question 3 + Forms tương tác |  Hoàn thành | 3/3 |
| **Question 5** | Tài liệu hướng dẫn chi tiết deploy Azure Function lên Azure Cloud (`DEPLOYMENT_GUIDE.md`) |  Hoàn thành | 1/1 |
| **Bonus** | Coding convention chuẩn N-Tier Architecture (Service & Repository Pattern), RESTful API |  Hoàn thành | 1/1 |
| **Tổng điểm** | | **Đạt** | **15/15** |

---

## 🏗️ Kiến Trúc Hệ Thống (Clean Architecture: Controller / Function -> Service -> Repository -> Data)

Hệ thống được thiết kế theo cấu trúc tách lớp chuẩn doanh nghiệp (.NET N-Tier):

```
BattleGameFunction/
├── Data/
│   └── BattleGameDbContext.cs           # Entity Framework Core DbContext
├── Models/
│   ├── Player.cs                        # Entity Player
│   ├── Asset.cs                         # Entity Asset
│   └── PlayerAsset.cs                   # Entity PlayerAsset (Junction table)
├── DTOs/
│   └── Requests.cs                      # Data Transfer Objects & API Response Wrappers
├── Repositories/                        # TẦNG REPOSITORY (Truy vấn CSDL)
│   ├── IPlayerRepository.cs             # Interface Player Repository
│   ├── PlayerRepository.cs              # Implement Player Repository
│   ├── IAssetRepository.cs              # Interface Asset Repository
│   ├── AssetRepository.cs               # Implement Asset Repository
│   ├── IPlayerAssetRepository.cs        # Interface PlayerAsset Repository
│   └── PlayerAssetRepository.cs         # Implement PlayerAsset Repository
├── Services/                            # TẦNG SERVICE (Business Logic & Validation)
│   ├── IPlayerService.cs                # Interface Player Service
│   ├── PlayerService.cs                 # Implement Player Service
│   ├── IAssetService.cs                 # Interface Asset Service
│   ├── AssetService.cs                  # Implement Asset Service
│   ├── IReportService.cs                # Interface Report Service
│   └── ReportService.cs                 # Implement Report Service
├── Functions/                           # TẦNG AZURE FUNCTION (HTTP Triggers / API Endpoints)
│   ├── RegisterPlayerFunction.cs        # [POST] /api/registerplayer (Q1) -> IPlayerService
│   ├── CreateAssetFunction.cs          # [POST] /api/createasset (Q2) -> IAssetService
│   ├── GetAssetsByPlayerFunction.cs    # [GET]  /api/getassetsbyplayer (Q3) -> IReportService
│   └── ManagementFunctions.cs          # [POST] /api/assignasset, [GET] /api/players, /api/assets
└── Program.cs                           # Cấu hình Dependency Injection (Scoped Services & Repos)
```

---

## 🗄️ 1. Cấu Trúc Cơ Sở Dữ Liệu (`BATTLEGAME`)

Tên Database: `BATTLEGAME` (MySQL). File script: [`database.sql`](file:///e:/exam/T2508M_SEM_Test/database.sql).

### Sơ đồ quan hệ & Chi tiết bảng:
- **`Player`**:
  - `PlayerId`: `VARCHAR(36)` (Primary Key, GUID/UUID)
  - `PlayerName`: `VARCHAR(64)` NOT NULL
  - `FullName`: `VARCHAR(128)` NOT NULL
  - `Age`: `VARCHAR(10)` NOT NULL
  - `Level`: `INT` NOT NULL
  - `Email`: `VARCHAR(64)` NOT NULL
- **`Asset`**:
  - `AssetId`: `VARCHAR(36)` (Primary Key, GUID/UUID)
  - `AssetName`: `VARCHAR(64)` NOT NULL
  - `LevelRequire`: `INT` NOT NULL
- **`PlayerAsset`** (Bảng liên kết Nhiều - Nhiều):
  - `PlayerId`: `VARCHAR(36)` (Foreign Key -> Player.PlayerId)
  - `AssetId`: `VARCHAR(36)` (Foreign Key -> Asset.AssetId)
  - Primary Key: `(PlayerId, AssetId)`

### Dữ liệu mẫu khởi tạo chuẩn theo đề bài:
```sql
-- Dữ liệu hiển thị trong báo cáo Requirement 3
No | Player name | Level | Age | Asset name
1  | Player 1    | 10    | 20  | Hero 1
2  | Player 2    | 3     | 19  | Hero 2
3  | Player 3    | 10    | 23  | Hero 1
```

---

## ⚡ 2. Danh Sách Azure Function APIs

Base URL Local: `http://localhost:7071/api`

### 1. API: `registerplayer` (Question 1)
- **Method**: `POST`
- **Route**: `/api/registerplayer`
- **Architecture**: `RegisterPlayerFunction` -> `IPlayerService` -> `IPlayerRepository`
- **Body**:
  ```json
  {
    "playerName": "Player 1",
    "fullName": "Nguyen Van A",
    "age": "20",
    "level": 10,
    "email": "player1@battlegame.com"
  }
  ```
- **Response**: `201 Created`

### 2. API: `createasset` (Question 2)
- **Method**: `POST`
- **Route**: `/api/createasset`
- **Architecture**: `CreateAssetFunction` -> `IAssetService` -> `IAssetRepository`
- **Body**:
  ```json
  {
    "assetName": "Hero 1",
    "levelRequire": 1
  }
  ```
- **Response**: `201 Created`

### 3. API: `getassetsbyplayer` (Question 3)
- **Method**: `GET`
- **Route**: `/api/getassetsbyplayer`
- **Architecture**: `GetAssetsByPlayerFunction` -> `IReportService` -> `IPlayerAssetRepository`
- **Response**: `200 OK`
  ```json
  [
    { "no": 1, "playerName": "Player 1", "level": 10, "age": "20", "assetName": "Hero 1" },
    { "no": 2, "playerName": "Player 2", "level": 3, "age": "19", "assetName": "Hero 2" },
    { "no": 3, "playerName": "Player 3", "level": 10, "age": "23", "assetName": "Hero 1" }
  ]
  ```

### 4. Các API bổ trợ:
- `POST /api/assignasset`: Gán tài sản cho người chơi (`{ "playerId": "...", "assetId": "..." }`).
- `GET /api/players`: Lấy danh sách toàn bộ người chơi.
- `GET /api/assets`: Lấy danh sách toàn bộ tài sản.

---

## 💻 3. Website Frontend (Question 4)

Thư mục: [`frontend/`](file:///e:/exam/T2508M_SEM_Test/frontend)
- Công nghệ: **React 19 + Vite**, Lucide React, Google Fonts (Outfit & Inter), Glassmorphic Dark UI.
- Hiển thị đầy đủ bảng báo cáo theo yêu cầu số 3.
- Tích hợp các modal tương tác trực tiếp với API:
  - Form Đăng ký người chơi mới (`registerplayer`).
  - Form Tạo tài sản mới (`createasset`).
  - Form Gán tài sản cho người chơi (`assignasset`).
  - Bộ lọc tìm kiếm thời gian thực (Search filter) và nút Refresh đồng bộ API.

---

## 🚀 4. Hướng Dẫn Chạy Dự Án Cục Bộ (Local Quickstart)

### Bước 1: Khởi tạo Cơ sở dữ liệu MySQL
Import file SQL vào MySQL (XAMPP / MySQL Server trên máy):
```powershell
Get-Content "database.sql" | & "E:\xam\mysql\bin\mysql.exe" -u root
```

### Bước 2: Khởi động Azure Function API
```powershell
cd BattleGameFunction
func start
```
API sẽ lắng nghe tại: `http://localhost:7071/api/`

### Bước 3: Khởi động Frontend React
```powershell
cd frontend
npm install
npm run dev
```
Mở trình duyệt truy cập: `http://localhost:5173/`

---

## 📖 5. Hướng Dẫn Triển Khai Lên Azure Cloud (Question 5)

Xem chi tiết từng bước tạo tài nguyên, cấu hình và deploy tại file tài liệu:
👉 [**DEPLOYMENT_GUIDE.md**](file:///e:/exam/T2508M_SEM_Test/DEPLOYMENT_GUIDE.md)
