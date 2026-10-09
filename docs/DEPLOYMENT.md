# Hướng dẫn chạy và triển khai BattleGame

Backend của project là Azure Functions .NET Isolated (Functions runtime v4); các function HTTP dùng EF Core kết nối SQL Server. Frontend là React. Project nhắm .NET 10 trên isolated worker. Microsoft hỗ trợ .NET 10 với Functions v4 ở isolated worker; khi chạy Linux, .NET 10 cần Flex Consumption thay vì Linux Consumption. [Tài liệu hỗ trợ .NET trên Azure Functions](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide)

## 1. Chạy cả project bằng Docker Compose

Cài Docker Desktop và bật Docker Linux containers. Mở PowerShell ở thư mục gốc project:

```powershell
docker compose up --build
```

Compose khởi động SQL Server, Azurite, Azure Functions và React/Nginx:

- React: <http://localhost:5173>
- Functions host: <http://localhost:7071>
- Function health check: <http://localhost:7071/api/health>
- SQL Server: `localhost,1433`; database `BATTLEGAME`; login `sa`

Khi Functions host khởi động, EF Core `EnsureCreated` tạo database và ba bảng `Player`, `Asset`, `PlayerAsset` nếu chúng chưa có. Azurite thay Azure Storage trong môi trường Docker local. SQL Server và Azurite dùng Docker volumes để giữ dữ liệu.

Mật khẩu SQL trong compose là mật khẩu mẫu cho máy local. Có thể đặt mật khẩu riêng trước khi chạy:

```powershell
$env:MSSQL_SA_PASSWORD = 'YourStrongLocalPassword_2026!'
docker compose up --build
```

Dừng container bằng `docker compose down`. Lệnh `docker compose down -v` xóa cả dữ liệu SQL Server và Azurite.

## 2. Chạy riêng để phát triển

1. Khởi động SQL Server và Azurite: `docker compose up -d sqlserver azurite`.
2. Tạo local settings:

   ```powershell
   Copy-Item backend/BattleGame.Functions/local.settings.sample.json backend/BattleGame.Functions/local.settings.json
   ```

3. Cài Azure Functions Core Tools v4. Từ `backend/BattleGame.Functions`, chạy `func start`.
4. Trong terminal khác, từ `frontend`, chạy `npm install` rồi `npm run dev`.

Vite proxy các request `/api/*` đến Functions host `http://localhost:7071`. Không commit `local.settings.json`; file này đã được gitignore.

## 3. HTTP Functions

Azure Functions tự thêm route prefix `/api` theo cấu hình `host.json`.

| Trigger name | Method và route | Mô tả |
|---|---|---|
| `registerplayer` | `POST /api/registerplayer` | Thêm player vào bảng `Player` |
| `createasset` | `POST /api/createasset` | Thêm hero/equipment/asset vào bảng `Asset` |
| `getassetsbyplayer` | `GET /api/getassetsbyplayer` | Báo cáo player và asset; hỗ trợ `?playerId=1` |
| `assignasset` | `POST /api/assignasset` | Gán asset cho player, ghi vào bảng liên kết |
| `getplayers` | `GET /api/players` | Trả danh sách player cho giao diện |
| `getassets` | `GET /api/assets` | Trả danh sách asset cho giao diện |
| `health` | `GET /api/health` | Health check |

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

## 4. Triển khai Azure Function App lên Azure

Project dùng .NET 10 isolated worker. Trên Azure Linux, tạo Function App với Flex Consumption, Azure Functions runtime v4, và runtime `dotnet-isolated` phiên bản `10.0`. Flex Consumption hỗ trợ lựa chọn runtime bằng Azure CLI. [Tạo Function App Flex Consumption](https://learn.microsoft.com/en-us/azure/azure-functions/flex-consumption-how-to)

### Tạo Azure SQL

1. Tạo Azure SQL logical server và database `BATTLEGAME`.
2. Cấu hình firewall/VNet để Function App kết nối được đến SQL server; ưu tiên private endpoint cho ứng dụng production.
3. Tạo connection string cho Azure SQL và lưu dưới dạng Function App setting/secret, không lưu trong Git:

```text
Server=tcp:<server>.database.windows.net,1433;Initial Catalog=BATTLEGAME;Persist Security Info=False;User ID=<user>;Password=<secret>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

Database `BATTLEGAME` cần được tạo như tài nguyên Azure SQL. Khi Function App khởi động, EF Core tự tạo ba bảng nếu database chưa có schema. Identity/credentials của ứng dụng cần quyền phù hợp để tạo bảng. Với schema thay đổi sau này, chuyển sang EF Core migrations và triển khai migration trước khi phát hành code mới.

### Tạo Function App (.NET 10 isolated)

Các lệnh dưới đây là mẫu PowerShell; thay tên và khu vực bằng giá trị Azure của bạn:

```powershell
az login
az group create --name <RESOURCE_GROUP> --location <REGION>
az storage account create --name <STORAGE_NAME> --location <REGION> --resource-group <RESOURCE_GROUP> --sku Standard_LRS --allow-blob-public-access false
az functionapp create --resource-group <RESOURCE_GROUP> --name <FUNCTION_APP_NAME> --storage-account <STORAGE_NAME> --flexconsumption-location <REGION> --runtime dotnet-isolated --runtime-version 10.0
```

Đặt setting `ConnectionStrings__BattleGame` của Function App bằng connection string Azure SQL qua Azure Portal > Function App > Settings > Environment variables (hoặc secret reference). Function App cần `FUNCTIONS_WORKER_RUNTIME=dotnet-isolated`; lệnh tạo app thường cấu hình runtime. Worker isolated và các file cần thiết được tạo bởi Azure Functions SDK trong quá trình publish. [Hướng dẫn isolated worker và publish](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide)

### Publish Function App

Cài Azure Functions Core Tools v4, đăng nhập Azure CLI, rồi chạy từ thư mục project Function:

```powershell
cd backend/BattleGame.Functions
func azure functionapp publish <FUNCTION_APP_NAME>
```

Sau khi publish, kiểm tra `https://<FUNCTION_APP_NAME>.azurewebsites.net/api/health`, sau đó thử ba route theo đề bài. Endpoint trong project để `AuthorizationLevel.Anonymous` cho demo; bật Function key hoặc bổ sung xác thực trước khi public các thao tác quản trị.

## 5. Triển khai React

React đang dùng URL tương đối `/api`, được Nginx chuyển tiếp tới Functions container khi chạy Docker Compose. Nếu host React độc lập (ví dụ Azure Static Web Apps), cấu hình reverse proxy/API gateway `/api/*` tới Function App để giữ cùng origin. Nếu frontend gọi trực tiếp domain Azure Functions, thêm domain frontend vào mục CORS của Function App và thay API base URL trong React.

## 6. Lưu ý production

- Thay mật khẩu mặc định bằng secret manager và giới hạn quyền truy cập database.
- Giới hạn CORS theo domain frontend thật.
- Các function ghi dữ liệu hiện cho phép gọi anonymous theo cấu hình demo; cần Function key hoặc xác thực/ủy quyền trước khi public.
- `EnsureCreated` phù hợp bootstrap project trống; dùng EF Core migrations khi quản lý thay đổi schema qua nhiều phiên bản.
