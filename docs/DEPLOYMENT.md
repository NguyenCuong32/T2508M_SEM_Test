# Hướng dẫn triển khai BATTLEGAME lên Azure

Tài liệu này triển khai SQL Server thành Azure SQL, backend thành Azure Function App và frontend thành Azure Static Web Apps.

## 1. Điều kiện

- Azure subscription.
- Azure CLI đã đăng nhập bằng `az login`.
- Azure Functions Core Tools 4.x.
- .NET 8 SDK và Node.js 20 trở lên.
- Tên Function App và Storage Account phải duy nhất trên Azure.

Các lệnh dưới đây dùng PowerShell. Thay các giá trị trong dấu `<...>` trước khi chạy.

## 2. Kiểm tra project trước khi deploy

```powershell
dotnet build backend/BattleGame.Functions/BattleGame.Functions.csproj --configuration Release
npm --prefix frontend install
npm --prefix frontend run build
```

Kết quả mong đợi: backend build không có error và frontend tạo thư mục `frontend/dist`.

## 3. Tạo resource group và storage account

```powershell
az group create --name <resource-group> --location southeastasia

az storage account create `
  --name <unique-storage-name> `
  --resource-group <resource-group> `
  --location southeastasia `
  --sku Standard_LRS
```

Tên Storage Account chỉ chứa chữ thường và số.

## 4. Tạo Azure SQL

```powershell
az sql server create `
  --name <unique-sql-server> `
  --resource-group <resource-group> `
  --location southeastasia `
  --admin-user <sql-admin> `
  --admin-password '<strong-password>'

az sql db create `
  --resource-group <resource-group> `
  --server <unique-sql-server> `
  --name BATTLEGAME `
  --service-objective Basic
```

Thêm địa chỉ IP hiện tại vào firewall của SQL Server từ Azure Portal: SQL server > Networking > Firewall rules. Để Function App truy cập được database trong bài demo, bật "Allow Azure services and resources to access this server"; trong production nên giới hạn mạng bằng Private Endpoint hoặc firewall phù hợp. Chỉ mở đúng IP cần thiết, không mở toàn Internet.

Tạo connection string theo mẫu:

```text
Server=tcp:<unique-sql-server>.database.windows.net,1433;Initial Catalog=BATTLEGAME;Persist Security Info=False;User ID=<sql-admin>;Password=<strong-password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

Khởi tạo bảng bằng một trong hai cách:

- Chạy `database/init.sql` trong Azure Portal Query Editor, SSMS hoặc Azure Data Studio.
- Đặt biến môi trường `BATTLEGAME_CONNECTION_STRING`, sau đó chạy `dotnet ef database update` trong thư mục backend.

Không chạy đồng thời cả hai cách trên một database đã có schema.

## 5. Tạo Function App

```powershell
az functionapp create `
  --resource-group <resource-group> `
  --name <unique-function-app> `
  --storage-account <unique-storage-name> `
  --consumption-plan-location southeastasia `
  --os-type Windows `
  --runtime dotnet-isolated `
  --runtime-version 8 `
  --functions-version 4
```

Thiết lập connection string bằng App Settings:

```powershell
az functionapp config appsettings set `
  --resource-group <resource-group> `
  --name <unique-function-app> `
  --settings BATTLEGAME_CONNECTION_STRING='<azure-sql-connection-string>'
```

App settings được Azure lưu riêng với source code. Không đưa secret vào repository.

## 6. Deploy Azure Functions

Chạy lệnh từ thư mục chứa `host.json`:

```powershell
cd backend/BattleGame.Functions
func azure functionapp publish <unique-function-app>
```

Sau khi deploy, kiểm tra:

```text
GET https://<unique-function-app>.azurewebsites.net/api/getassetsbyplayer
```

Response phải có HTTP `200` và ba dòng demo nếu database đã được seed.

## 7. Deploy React lên Azure Static Web Apps

1. Push repository lên GitHub.
2. Trong Azure Portal, chọn Create a resource > Static Web App.
3. Kết nối repository và branch cần deploy.
4. Chọn framework React.
5. Đặt các đường dẫn build:
   - App location: `/frontend`
   - API location: để trống
   - Output location: `dist`
6. Thêm biến build `VITE_API_BASE_URL` với giá trị:

```text
https://<unique-function-app>.azurewebsites.net
```

7. Chạy lại GitHub Actions workflow và mở URL Static Web App sau khi workflow hoàn tất.

## 8. Cấu hình CORS

Sau khi có URL frontend:

```powershell
az functionapp cors add `
  --resource-group <resource-group> `
  --name <unique-function-app> `
  --allowed-origins https://<static-web-app-domain>
```

Không dùng `*` cho production nếu không thực sự cần thiết.

## 9. Kiểm thử sau deploy

### Đăng ký người chơi

```http
POST https://<unique-function-app>.azurewebsites.net/api/registerplayer
Content-Type: application/json

{
  "playerName": "Cloud Player",
  "fullName": "Azure Demo Player",
  "age": 22,
  "level": 1,
  "email": "cloud.player@example.com"
}
```

Kết quả mong đợi: HTTP `201 Created`.

### Tạo tài sản

```http
POST https://<unique-function-app>.azurewebsites.net/api/createasset
Content-Type: application/json

{
  "assetName": "Cloud Sword",
  "levelRequire": 2
}
```

Kết quả mong đợi: HTTP `201 Created`.

### Báo cáo

```http
GET https://<unique-function-app>.azurewebsites.net/api/getassetsbyplayer
```

Kết quả mong đợi: HTTP `200 OK` và JSON array. Người chơi hoặc tài sản vừa tạo chưa xuất hiện trong báo cáo cho đến khi có bản ghi liên kết trong `PlayerAsset`.

## 10. Checklist nộp bài

- Database có đủ `Player`, `Asset`, `PlayerAsset`, PK/FK và demo data.
- Ba endpoint trả đúng status code và JSON.
- Website tải dữ liệu thật từ Function App.
- CORS chỉ cho phép domain frontend.
- Không có password, token hoặc connection string thật trong Git.
- README chứa lệnh chạy local.
- Có ảnh chụp API và website đã deploy nếu giảng viên yêu cầu.

## Tài liệu Microsoft tham khảo

- [Azure Functions .NET isolated process guide](https://learn.microsoft.com/azure/azure-functions/dotnet-isolated-process-guide)
- [Develop Azure Functions locally with Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local)
- [Configure Function App settings](https://learn.microsoft.com/azure/azure-functions/functions-how-to-use-azure-function-app-settings)
- [Deploy a React app on Azure Static Web Apps](https://learn.microsoft.com/azure/static-web-apps/deploy-react)
