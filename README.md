# BattleGame

Ứng dụng quản lý người chơi và tài sản game theo đề bài. Backend dùng Azure Functions .NET Isolated, EF Core và SQL Server; frontend dùng React + Vite.

## Chạy toàn bộ bằng Docker

Cần cài Docker Desktop và bật Linux containers. Tại thư mục gốc project:

```powershell
docker compose up --build
```

- Giao diện: <http://localhost:5173>
- Azure Functions host: <http://localhost:7071>
- Health check: <http://localhost:7071/api/health>
- SQL Server: `localhost,1433`, database `BATTLEGAME`, user `sa`

Lần khởi động đầu, Azure Functions host đợi SQL Server sẵn sàng rồi dùng EF Core `EnsureCreated` để tạo database `BATTLEGAME` cùng ba bảng `Player`, `Asset`, `PlayerAsset`. Azurite cung cấp Azure Storage giả lập cho Functions khi chạy local. Dữ liệu SQL và storage được giữ trong các Docker volume.

Mật khẩu mặc định trong compose chỉ dành cho chạy local. Có thể ghi đè trước khi chạy:

```powershell
$env:MSSQL_SA_PASSWORD = 'YourStrongLocalPassword_2026!'
docker compose up --build
```

Dừng ứng dụng: `docker compose down`. Xóa cả dữ liệu database local: `docker compose down -v`.

## Chạy riêng để phát triển

1. Khởi động SQL Server bằng Docker: `docker compose up -d sqlserver`.
2. Copy `backend/BattleGame.Functions/local.settings.sample.json` thành `local.settings.json`, rồi chạy Functions host: `cd backend/BattleGame.Functions; func start`.
3. Chạy giao diện trong terminal khác: `cd frontend; npm install; npm run dev`.

Vite chuyển tiếp `/api` tới `http://localhost:7071`. Functions host sẽ tự tạo database và bảng khi chạy lần đầu. Cài Azure Functions Core Tools và chạy SQL Server + Azurite bằng Docker trước.

## API

| Method | Route | Mô tả |
|---|---|---|
| `POST` | `/api/registerplayer` | Azure Function đăng ký người chơi |
| `POST` | `/api/createasset` | Azure Function tạo hero/equipment/asset |
| `POST` | `/api/assignasset` | Azure Function gán asset cho người chơi |
| `GET` | `/api/getassetsbyplayer` | Azure Function báo cáo tất cả người chơi; hỗ trợ `?playerId=1` |
| `GET` | `/api/players` | Azure Function lấy danh sách người chơi cho form |
| `GET` | `/api/assets` | Azure Function lấy danh sách asset cho form |

Ví dụ request:

```json
POST /api/registerplayer
{
  "playerName": "Player 1",
  "fullName": "Nguyen Van A",
  "age": 20,
  "currentLevel": 10
}
```

```json
POST /api/createasset
{
  "name": "Hero 1",
  "type": "Hero",
  "description": "Starter hero"
}
```

## Cấu trúc

- `backend/BattleGame.Functions`: Azure Functions .NET Isolated, các HTTP trigger, model và cấu hình EF Core SQL Server.
- `frontend`: React dashboard, báo cáo và form tạo/gán dữ liệu.
- `docker-compose.yml`: SQL Server, API và frontend.
- `docs/DEPLOYMENT.md`: hướng dẫn chạy Docker và triển khai Azure Function App lên Azure.

Schema hiện được tạo tự động từ model bằng `EnsureCreated`. Khi cần cập nhật schema cho hệ thống đã có dữ liệu, hãy chuyển sang EF Core migrations và triển khai migration trước khi phát hành phiên bản mới.
