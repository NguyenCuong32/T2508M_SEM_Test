# BATTLEGAME

Bài thực hành Azure Functions quản lý người chơi và tài sản trong game. Repository gồm backend .NET isolated, SQL Server schema/seed, frontend React và tài liệu triển khai Azure.

## Chức năng đã triển khai

- `POST /api/registerplayer`: đăng ký người chơi, kiểm tra dữ liệu và chống trùng tên/email.
- `POST /api/createasset`: tạo tài sản mới, kiểm tra tên và level yêu cầu.
- `GET /api/getassetsbyplayer`: báo cáo toàn bộ tài sản của người chơi.
- `GET /api/getassetsbyplayer?playerId=<guid>`: lọc báo cáo theo người chơi.
- Giao diện React responsive, có tìm kiếm, refresh, loading, empty state và error state.
- Migration Code First và `database/init.sql` có sẵn ba dòng dữ liệu theo đề bài.

## Cấu trúc

```text
backend/BattleGame.Functions/  Azure Functions .NET isolated + EF Core
database/init.sql              SQL Server schema và demo data
frontend/                      React + Vite report website
docs/DEPLOYMENT.md             Hướng dẫn triển khai lên Azure
```

## Yêu cầu cục bộ

- .NET 8 SDK.
- Azure Functions Core Tools 4.x.
- SQL Server LocalDB, SQL Server hoặc Azure SQL.
- Node.js 20 trở lên.

## Khởi tạo database

Chọn một trong hai cách.

### Cách 1: Code First

```powershell
cd backend/BattleGame.Functions
Copy-Item local.settings.example.json local.settings.json
dotnet tool restore
dotnet ef database update
```

### Cách 2: SQL script

Mở `database/init.sql` bằng SQL Server Management Studio hoặc Azure Data Studio và chạy toàn bộ script.

`Age` trong sơ đồ gốc là `nvarchar(10)`. Dự án chủ động sử dụng `int` để validation, sắp xếp và truy vấn đúng kiểu dữ liệu.

## Chạy backend

Kiểm tra connection string trong `local.settings.json`, sau đó:

```powershell
cd backend/BattleGame.Functions
func start
```

API chạy mặc định tại `http://localhost:7071/api`. Có thể thử request trong `BattleGame.Functions.http` bằng REST Client hoặc IDE hỗ trợ HTTP files.

## Chạy frontend

```powershell
cd frontend
npm install
npm run dev
```

Mở `http://localhost:5173`. Vite proxy các request `/api` sang Function Host tại port `7071`.

## Build kiểm tra

```powershell
dotnet build backend/BattleGame.Functions/BattleGame.Functions.csproj
npm --prefix frontend run build
```

## Cấu hình production

- Backend cần app setting `BATTLEGAME_CONNECTION_STRING`.
- Frontend cần `VITE_API_BASE_URL=https://<function-app>.azurewebsites.net` tại thời điểm build.
- Function App phải cho phép CORS từ domain frontend.
- Không commit `local.settings.json`, `.env` hoặc connection string thật.

Xem hướng dẫn chi tiết tại [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md).
