# Hướng dẫn triển khai Battle Game lên Azure

Tài liệu này hướng dẫn triển khai dự án Azure Functions Isolated Worker dùng .NET 10, Entity Framework Core 10 và SQL Server. Backend được đưa lên Azure Functions, database dùng Azure SQL Database và frontend HTML/CSS/JavaScript được triển khai riêng.

Đây là tài liệu hướng dẫn cho người triển khai. Dự án chưa được triển khai thực tế lên Azure; các bước cloud dưới đây chưa được kiểm thử trên một Azure subscription. Việc tạo tài nguyên chỉ thực hiện khi người dùng chủ động triển khai và có thể phát sinh chi phí.

## 1. Chuẩn bị

- Azure account có subscription và quyền tạo tài nguyên.
- .NET SDK 10.x.
- Azure CLI bản hiện hành.
- Azure Functions Core Tools 4.x bản hiện hành có hỗ trợ .NET 10.
- PowerShell 7 để chạy các lệnh trong tài liệu.
- Công cụ `dotnet-ef` 10.0.12, tương ứng phiên bản EF Core trong dự án.

Kiểm tra công cụ:

```powershell
dotnet --version
az version
func --version
dotnet ef --version
```

Nếu chưa có Azure CLI và Core Tools trên Windows:

```powershell
winget install --id Microsoft.AzureCLI --exact
winget install --id Microsoft.Azure.FunctionsCoreTools --exact
```

Mở terminal mới sau khi cài đặt. Nếu chưa có EF CLI:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.12
```

Nếu đã cài phiên bản cũ, dùng `dotnet tool update --global dotnet-ef --version 10.0.12`.

Từ thư mục gốc repository, chuyển vào thư mục chứa project và kiểm tra build:

```powershell
cd BattleGame
dotnet restore
dotnet build
```

Các bước tiếp theo chạy từ thư mục `BattleGame`, trong cùng một terminal PowerShell, trừ khi được chỉ rõ khác đi.

## 2. Đăng nhập và chọn subscription

```powershell
az login
az account list --output table
az account set --subscription "<SUBSCRIPTION_ID>"
```

Thay các giá trị trong dấu `<...>` bằng thông tin của người triển khai trước khi chạy lệnh.

## 3. Tạo resource group và storage account

```powershell
$resourceGroup = "rg-battlegame"
$location = "southeastasia"
$storageName = "<TEN_STORAGE_DUY_NHAT>"
$functionName = "<TEN_FUNCTION_APP_DUY_NHAT>"

az group create --name $resourceGroup --location $location

az storage account create `
  --name $storageName `
  --resource-group $resourceGroup `
  --location $location `
  --sku Standard_LRS `
  --kind StorageV2
```

Storage account cần tên duy nhất toàn cục, dài 3-24 ký tự và chỉ gồm chữ thường cùng chữ số. Function App cũng cần tên duy nhất để tạo hostname công khai. Storage account này phục vụ Azure Functions, không thay thế database BATTLEGAME.

## 4. Tạo Azure Function App

Sử dụng Flex Consumption với Linux và runtime `dotnet-isolated` phiên bản `10.0`. .NET 10 không được hỗ trợ trên Linux Consumption kiểu cũ.

Kiểm tra khu vực và runtime khả dụng:

```powershell
az functionapp list-flexconsumption-locations --output table

az functionapp list-flexconsumption-runtimes `
  --location $location `
  --runtime dotnet-isolated `
  --output table
```

Nếu khu vực đã chọn chưa cung cấp .NET 10, chọn khu vực có hỗ trợ rồi tạo tài nguyên ở khu vực đó; không hạ target framework của project.

```powershell
az functionapp create `
  --resource-group $resourceGroup `
  --name $functionName `
  --storage-account $storageName `
  --flexconsumption-location $location `
  --runtime dotnet-isolated `
  --runtime-version 10.0
```

Với Flex Consumption, runtime được cấu hình qua Function App và Azure CLI. Không sao chép toàn bộ `local.settings.json` lên Azure, không đưa `UseDevelopmentStorage=true` lên cloud và không tự thêm các setting runtime kiểu cũ vào Flex Consumption.

## 5. Tạo database trên Azure SQL

LocalDB chỉ phục vụ máy phát triển. Azure Functions không thể sử dụng `(localdb)\MSSQLLocalDB` của máy cá nhân.

Trong Azure Portal:

1. Chọn **Create a resource > SQL Database**.
2. Chọn subscription và resource group vừa tạo.
3. Đặt database name là **BATTLEGAME**.
4. Tạo SQL logical server với tên duy nhất và khu vực phù hợp. Để làm theo connection string bên dưới, cấu hình SQL authentication và lưu tài khoản quản trị ở nơi riêng tư.
5. Chọn cấu hình compute/storage phù hợp nhu cầu thử nghiệm rồi tạo database.
6. Mở SQL server > **Networking**. Cho phép truy cập từ địa chỉ IP công khai của máy chạy migration bằng **Add your client IPv4 address**.
7. Cấu hình đường truy cập từ Function App tới SQL server. Với môi trường bài thực hành dùng public endpoint, có thể bật **Allow Azure services and resources to access this server**. Tùy chọn này cho phép kết nối mạng từ các tài nguyên Azure khác, không chỉ subscription của bạn; SQL authentication vẫn bắt buộc. Với triển khai cần giới hạn truy cập, dùng VNet integration và private endpoint hoặc quy tắc mạng phù hợp.
8. Lưu cấu hình networking.

Mẫu connection string, không điền mật khẩu thật vào file trong repository:

```text
Server=tcp:<SQL_SERVER_NAME>.database.windows.net,1433;Initial Catalog=BATTLEGAME;User ID=<SQL_USERNAME>;Password=<SQL_PASSWORD>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

Nếu mật khẩu có ký tự đặc biệt của connection string như dấu chấm phẩy, phải escape/quote đúng cú pháp SqlClient. Có thể lấy mẫu ADO.NET connection string từ trang database trên Azure Portal.

## 6. Áp dụng Code First migration

Project đã có migration `InitialCreate`. Không tạo migration cùng tên lần nữa và không dùng `EnsureCreated` thay thế migration.

Nhập connection string tại terminal; giá trị không hiển thị khi nhập:

```powershell
$azureSqlConnection = Read-Host "Nhap connection string Azure SQL" -MaskInput
$env:ConnectionStrings__DefaultConnection = $azureSqlConnection

dotnet ef database update
```

Chỉ tiếp tục nếu lệnh thành công. Database sẽ có:

- `Player`, khóa chính `PlayerId`, unique index cho `Email`.
- `Asset`, khóa chính `AssetId`.
- `PlayerAsset`, khóa chính ghép `PlayerId` và `AssetId`, liên kết hai bảng trên.
- `__EFMigrationsHistory`, bảng theo dõi migration của EF Core.

Để có report mẫu trên cloud, mở Query editor hoặc SQL Server Management Studio và kết nối trực tiếp vào database BATTLEGAME. Chạy phần nội dung của `Database/sample-data.sql` từ `SET XACT_ABORT ON;` đến hết, bỏ dòng `USE [BATTLEGAME];`. Azure SQL không dùng `USE` để chuyển database như SQL Server local. Script mẫu có thể chạy lại mà không tạo bản ghi mẫu trùng.

## 7. Cấu hình connection string cho Function App

```powershell
az functionapp config appsettings set `
  --resource-group $resourceGroup `
  --name $functionName `
  --settings "ConnectionStrings__DefaultConnection=$azureSqlConnection" `
  --output none
```

Cũng có thể mở Function App > **Settings > Environment variables**, thêm application setting `ConnectionStrings__DefaultConnection` và lưu giá trị Azure SQL connection string. Không đưa mật khẩu vào ảnh chụp báo cáo hoặc Git.

Project ưu tiên biến môi trường này trước cấu hình local. `local.settings.json` bị Git ignore và không được publish; giá trị LocalDB trong `appsettings.json` không phải cấu hình database production.

## 8. Publish backend

Tại thư mục `BattleGame`:

```powershell
func azure functionapp publish $functionName
```

Sau khi publish thành công, lấy hostname thực tế do Azure cấp thay vì tự suy đoán hostname:

```powershell
$hostname = az functionapp show `
  --resource-group $resourceGroup `
  --name $functionName `
  --query defaultHostName `
  --output tsv

$apiBaseUrl = "https://$hostname/api"
```

Xóa connection string khỏi các biến trong terminal sau khi hoàn tất migration và cấu hình:

```powershell
Remove-Item Env:ConnectionStrings__DefaultConnection
Remove-Variable azureSqlConnection
```

## 9. Kiểm tra API trên Azure

Trong Azure Portal > Function App > **Functions**, kiểm tra đủ ba function:

| Function | Method | URL |
|---|---|---|
| registerplayer | POST | `https://<DEFAULT_HOSTNAME>/api/registerplayer` |
| createasset | POST | `https://<DEFAULT_HOSTNAME>/api/createasset` |
| getassetsbyplayer | GET | `https://<DEFAULT_HOSTNAME>/api/getassetsbyplayer` |

Kiểm tra report:

```powershell
Invoke-RestMethod "$apiBaseUrl/getassetsbyplayer"
```

Kết quả thành công có `success: true`, `message` và `data`. Nếu chưa có liên kết PlayerAsset, `data` là mảng rỗng; đó không phải lỗi.

Đăng ký player với email chưa có trong database:

```powershell
$player = Invoke-RestMethod "$apiBaseUrl/registerplayer" `
  -Method Post `
  -ContentType "application/json" `
  -Body '{"playerName":"Cloud Player","fullName":"Nguyen Van A","age":"20","level":10,"email":"cloudplayer@example.com"}'

$player
```

`createasset` dùng `AuthorizationLevel.Function`. Lấy function key trong Azure Portal > Function App > Functions > **createasset > Function Keys**, rồi gửi bằng header `x-functions-key`. Key cấp quyền gọi function, không phải cơ chế đăng nhập/phân quyền người dùng.

```powershell
$functionKey = Read-Host "Nhap function key cua createasset" -MaskInput

$asset = Invoke-RestMethod "$apiBaseUrl/createasset" `
  -Method Post `
  -ContentType "application/json" `
  -Headers @{ "x-functions-key" = $functionKey } `
  -Body '{"assetName":"Cloud Hero","levelRequire":1}'

$asset
Remove-Variable functionKey
```

Hai API tạo dữ liệu trả HTTP `201`. Dữ liệu không hợp lệ trả `400`, email đã tồn tại trả `409`, lỗi server trả `500` với thông báo chung. Report trả `200`.

Để asset vừa tạo xuất hiện trong report, lấy `playerId` và `assetId` từ hai response rồi chạy SQL trong BATTLEGAME:

```sql
INSERT INTO PlayerAsset (PlayerId, AssetId)
VALUES ('<PLAYER_GUID>', '<ASSET_GUID>');
```

Sau đó gọi lại report. Dự án không tự gán asset khi đăng ký và không có API assign asset vì đề không yêu cầu.

## 10. Triển khai frontend và CORS

Frontend được triển khai riêng với backend. Thư mục cần đưa lên static hosting là `frontend`, gồm `index.html`, `css/style.css` và `js/app.js`.

1. Mở `frontend/js/app.js`.
2. Đổi giá trị `API_BASE_URL` từ localhost sang `https://<DEFAULT_HOSTNAME>/api`, dùng hostname lấy ở bước 8.
3. Trên Azure Portal, tạo một Storage Account riêng cho website, bật **Data management > Static website** và đặt **Index document name** thành `index.html`.
4. Upload nội dung bên trong thư mục `frontend` vào container `$web`, giữ nguyên thư mục `css` và `js`. `index.html` phải nằm trực tiếp ở gốc container, không nằm trong thư mục `frontend` lồng bên trong.
5. Sao chép **Primary endpoint** của Static website.
6. Thêm origin website vào CORS của Function App. Origin chỉ gồm scheme và hostname, không có path hoặc dấu `/` cuối.

```powershell
$frontendOrigin = "https://<STATIC_WEBSITE_HOSTNAME>"

az functionapp cors add `
  --resource-group $resourceGroup `
  --name $functionName `
  --allowed-origins $frontendOrigin
```

Mở website và kiểm tra bảng, nút **Refresh Data**, trạng thái loading và dữ liệu trả về từ API. Không đặt function key trong JavaScript public. Report hiện cho phép anonymous để frontend gọi trực tiếp; API tạo asset vẫn dùng function key trên Azure.

Khi chạy frontend local với backend local, `local.settings.json` đã cho phép `http://localhost:5500` và `http://127.0.0.1:5500`. Nếu dùng frontend local gọi backend Azure, cần thêm origin local vào CORS của Function App trên Azure. Cấu hình CORS local không tự được publish.

## 11. Xử lý lỗi thường gặp

| Hiện tượng | Cách kiểm tra |
|---|---|
| Không nhận lệnh `func` hoặc `az` | Kiểm tra cài đặt, PATH và mở terminal mới. |
| Không thấy runtime .NET 10 | Cập nhật công cụ, kiểm tra Flex Consumption và runtime khả dụng trong khu vực. |
| Migration không kết nối được | Kiểm tra SQL hostname, database, tài khoản, connection string và firewall cho IP máy phát triển. |
| API trả `500` | Xem Log stream/Application Insights; kiểm tra application setting và đường truy cập tới Azure SQL. Không công khai log chứa thông tin nhạy cảm. |
| `createasset` trả `401` | Kiểm tra function key và header `x-functions-key`. |
| `registerplayer` trả `409` | Email đã có trong database; thử email khác. |
| Report trả mảng rỗng | Kiểm tra dữ liệu `PlayerAsset`, không chỉ `Player` và `Asset`. |
| API chạy nhưng frontend lỗi CORS | Kiểm tra đúng origin website, HTTPS và `API_BASE_URL`. |
| Không thấy function sau publish | Kiểm tra publish từ thư mục chứa `BattleGame.csproj`, build thành công và runtime là .NET isolated. |

## 12. Checklist xác nhận sau triển khai

- [ ] Function App dùng .NET 10 isolated trên Flex Consumption.
- [ ] Azure SQL có database BATTLEGAME, đủ bảng và migration.
- [ ] Application setting `ConnectionStrings__DefaultConnection` đã cấu hình.
- [ ] Azure Functions truy cập được database qua cấu hình mạng đã chọn.
- [ ] Ba function xuất hiện trên Azure Portal.
- [ ] API đăng ký và tạo asset trả `201` với request hợp lệ.
- [ ] API report trả `200` và đọc dữ liệu thật từ SQL.
- [ ] Frontend trỏ tới hostname backend Azure và hiển thị đúng report.
- [ ] CORS cho phép đúng origin frontend.
- [ ] Không đưa mật khẩu hoặc function key vào repository/frontend.

Các ô trên dành cho người triển khai xác nhận sau khi thực hiện, không biểu thị rằng dự án đã được deploy.

## 13. Tài liệu tham khảo

- [Azure Functions isolated worker và hỗ trợ .NET](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide)
- [Tạo và quản lý Function App trên Flex Consumption](https://learn.microsoft.com/en-us/azure/azure-functions/flex-consumption-how-to)
- [Tạo Azure SQL Database](https://learn.microsoft.com/en-us/azure/azure-sql/database/single-database-create-quickstart)
- [Cấu hình firewall Azure SQL](https://learn.microsoft.com/en-us/azure/azure-sql/database/firewall-configure)
- [Azure Storage static website](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-static-website)
