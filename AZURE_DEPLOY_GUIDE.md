# HƯỚNG DẪN TRIỂN KHAI (DEPLOY) LÊN AZURE CLOUD
**Môn học:** Developing Microsoft Azure Solutions - Practical Exam  
**Database:** `BATTLEGAME` | **API Service:** Azure Functions / Web API

---

## 1. Chuẩn bị tài nguyên trên Azure Portal (Prerequisites)
1. Truy cập vào [Azure Portal](https://portal.azure.com) và đăng nhập tài khoản Azure.
2. Tạo **Resource Group**:
   - Tên: `rg-battlegame-prod`
   - Region: `Southeast Asia` (hoặc `East Asia`).

---

## 2. Triển khai Cơ sở dữ liệu SQL lên Azure SQL Database
1. Trong Resource Group, chọn **+ Create** -> Tìm kiếm **Azure SQL Database**.
2. Thiết lập:
   - **Database name:** `BATTLEGAME`
   - **Server:** Chọn **Create new** (VD: `sql-battlegame-server.database.windows.net`)
   - **Authentication:** SQL Server Authentication (Username & Password).
   - **Compute + storage:** Chọn tier tiết kiệm chi phí (`Basic` hoặc `Serverless`).
3. Cấu hình Firewall:
   - Vào SQL Server vừa tạo -> **Security** -> **Networking**.
   - Bật tùy chọn **"Allow Azure services and resources to access this server"**.
   - Thêm IP Client hiện tại của máy bạn để kết nối.
4. Chạy Script khởi tạo:
   - Dùng SSMS hoặc Azure Query Editor để chạy file script khởi tạo bảng `Player`, `Asset`, `PlayerAsset` và insert sample data.

---

## 3. Triển khai API / Azure Function lên Azure

### Cách 1: Triển khai trực tiếp từ Visual Studio (Khuyên dùng)
1. Mở Solution trong **Visual Studio**.
2. Nhấp chuột phải vào Project -> Chọn **Publish...**.
3. Chọn Target:
   - **Azure** -> Nhấn **Next**.
   - Chọn **Azure App Service (Windows/Linux)** hoặc **Azure Function App**.
4. Đăng nhập tài khoản Microsoft Azure.
5. Chọn Resource Group `rg-battlegame-prod` và tạo mới hoặc chọn App Service/Function App sẵn có.
6. Nhấn **Finish** và sau đó nhấn nút **Publish**.
7. Visual Studio sẽ build và đẩy toàn bộ code cùng frontend lên Azure.

### Cách 2: Triển khai bằng Azure CLI
Mở terminal PowerShell và thực hiện các lệnh sau:

```bash
# 1. Đăng nhập Azure
az login

# 2. Tạo Resource Group
az group create --name rg-battlegame-prod --location southeastasia

# 3. Tạo Azure Storage Account (bắt buộc cho Azure Function)
az storage account create --name stbattlegameprod --location southeastasia --resource-group rg-battlegame-prod --sku Standard_LRS

# 4. Tạo Function App (hoặc App Service)
az functionapp create --resource-group rg-battlegame-prod \
  --consumption-plan-location southeastasia \
  --runtime dotnet-isolated --functions-version 4 \
  --name func-battlegame-api \
  --storage-account stbattlegameprod

# 5. Build và publish project
dotnet publish -c Release -o ./publish

# 6. Deploy code lên Function App
az webapp deploy --resource-group rg-battlegame-prod --name func-battlegame-api --src-path ./publish.zip --type zip
```

---

## 4. Cấu hình Connection String & Environment Variables trên Azure
1. Trên Azure Portal, truy cập vào Function App / App Service vừa deploy.
2. Vào menu **Settings** -> **Configuration** (hoặc **Environment variables**).
3. Thêm mới Connection String:
   - **Name:** `DefaultConnection`
   - **Value:** `Server=tcp:sql-battlegame-server.database.windows.net,1433;Initial Catalog=BATTLEGAME;Persist Security Info=False;User ID=<your-username>;Password=<your-password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;`
   - **Type:** `SQLAzure`
4. Bật **CORS**:
   - Menu **API** -> **CORS**.
   - Thêm `*` (hoặc domain frontend của bạn) để cho phép gọi API từ mọi nguồn.
5. Nhấn **Save** và **Restart** service.

---

## 5. Kiểm tra và Nghiệm thu hệ thống
1. Truy cập Swagger UI:
   - `https://<your-app-name>.azurewebsites.net/swagger`
2. Kiểm tra API `registerplayer`:
   - Gửi request POST với payload JSON của Player mới.
3. Kiểm tra API `createasset`:
   - Gửi request POST tạo Asset mới.
4. Kiểm tra API `getassetsbyplayer`:
   - Gửi request GET để lấy danh sách báo cáo.
5. Truy cập giao diện Website:
   - `https://<your-app-name>.azurewebsites.net/index.html`
   - Kiểm tra bảng báo cáo hiển thị đầy đủ các cột: **No | Player name | Level | Age | Asset name**.
