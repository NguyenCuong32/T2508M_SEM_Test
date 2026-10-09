# HƯỚNG DẪN TRIỂN KHAI AZURE FUNCTION LÊN AZURE CLOUD
## Azure Function Deployment Tutorial Guide (SET01 - BATTLEGAME)

Tài liệu này hướng dẫn chi tiết từng bước (Step-by-Step) cách triển khai dịch vụ **Azure Function** (Backend .NET Isolated Worker) kết nối cơ sở dữ liệu **MySQL Database** và Frontend lên nền tảng **Microsoft Azure Cloud**.

---

## 1. Kiến Trúc Hệ Thống Trên Azure Cloud

```
+-------------------------------------------------------+
|                   Microsoft Azure Cloud               |
|                                                       |
|   +-----------------------+     +------------------+  |
|   | Azure Static Web App  | --> |  Azure Function  |  |
|   | (React + Vite Web UI) |     |  App (.NET 8/10) |  |
|   +-----------------------+     +--------+---------+  |
|                                          |            |
|                                          v            |
|                              +----------------------+ |
|                              | Azure Database for   | |
|                              | MySQL Flexible Server| |
|                              +----------------------+ |
+-------------------------------------------------------+
```

---

## 2. Các Công Cụ Cần Chuẩn Bị (Prerequisites)

1. **Tài khoản Azure**: Tài khoản Microsoft Azure còn hạn mức (Subscription active / Azure for Students).
2. **Azure CLI**: Cài đặt công cụ dòng lệnh Azure (`az`).
3. **Azure Functions Core Tools v4**: Cài đặt công cụ `func` (`npm install -g azure-functions-core-tools@4`).
4. **.NET SDK & Git**: Đã có trên máy.
5. **VS Code** kèm extension:
   - *Azure Tools*
   - *Azure Functions*
   - *C# Dev Kit*

---

## 3. Bước 1: Đăng Nhập Vào Azure Bằng CLI

Mở PowerShell / Command Prompt tại thư mục dự án và chạy:

```powershell
# 1. Đăng nhập vào tài khoản Azure
az login

# 2. Kiểm tra subscription đang hoạt động
az account show --output table

# 3. (Tùy chọn) Chọn subscription nếu có nhiều tài khoản
az account set --subscription "<TÊN_HOẶC_ID_SUBSCRIPTION>"
```

---

## 4. Bước 2: Tạo Nhóm Tài Nguyên (Resource Group)

Tạo một Resource Group để quản lý toàn bộ tài nguyên của bài thi:

```powershell
az group create \
  --name rg-battlegame-eastasia \
  --location eastasia
```

---

## 5. Bước 3: Tạo Azure Database for MySQL Flexible Server

### 3.1. Khởi tạo MySQL Flexible Server trên Azure
```powershell
az mysql flexible-server create \
  --resource-group rg-battlegame-eastasia \
  --name mysql-battlegame-prod \
  --location eastasia \
  --admin-user battlegameadmin \
  --admin-password "P@ssw0rdSecure2026!" \
  --sku-name Standard_B1ms \
  --tier Burstable \
  --storage-size 32 \
  --version 8.0.21
```

### 3.2. Cấu hình Firewall cho phép truy cập
```powershell
# Cho phép tất cả dịch vụ trong Azure kết nối vào MySQL
az mysql flexible-server firewall-rule create \
  --resource-group rg-battlegame-eastasia \
  --name mysql-battlegame-prod \
  --rule-name AllowAllAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Cho phép IP máy cá nhân để khởi tạo schema
az mysql flexible-server firewall-rule create \
  --resource-group rg-battlegame-eastasia \
  --name mysql-battlegame-prod \
  --rule-name AllowMyLocalIP \
  --start-ip-address "<YOUR_LOCAL_IP>" \
  --end-ip-address "<YOUR_LOCAL_IP>"
```

### 3.3. Tạo Database `BATTLEGAME` và chạy script khởi tạo
Chạy script `database.sql` vào Azure MySQL:
```powershell
mysql -h mysql-battlegame-prod.mysql.database.azure.com \
      -u battlegameadmin \
      -p"P@ssw0rdSecure2026!" \
      --ssl-mode=REQUIRED < database.sql
```

---

## 6. Bước 4: Tạo Storage Account & Azure Function App

Azure Function yêu cầu một Storage Account để lưu trữ trạng thái runtime và logs.

### 4.1. Tạo Storage Account:
```powershell
az storage account create \
  --name stabattlegamestorage \
  --resource-group rg-battlegame-eastasia \
  --location eastasia \
  --sku Standard_LRS
```

### 4.2. Tạo Azure Function App (.NET Isolated Worker):
```powershell
az functionapp create \
  --resource-group rg-battlegame-eastasia \
  --consumption-plan-location eastasia \
  --runtime dotnet-isolated \
  --functions-version 4 \
  --name func-battlegame-api \
  --storage-account stabattlegamestorage
```

---

## 7. Bước 5: Cấu Hình Biến Môi Trường (Application Settings) & CORS

### 7.1. Cấu hình Chuỗi Kết Nối MySQL (MySqlConnectionString):
Chuỗi kết nối trỏ tới Azure Database for MySQL:
```powershell
az functionapp config appsettings set \
  --name func-battlegame-api \
  --resource-group rg-battlegame-eastasia \
  --settings "MySqlConnectionString=Server=mysql-battlegame-prod.mysql.database.azure.com;Port=3306;Database=BATTLEGAME;User=battlegameadmin;Password=P@ssw0rdSecure2026!;SslMode=Required;"
```

### 7.2. Cấu hình CORS để Frontend gọi API không bị chặn:
```powershell
az functionapp cors add \
  --name func-battlegame-api \
  --resource-group rg-battlegame-eastasia \
  --allowed-origins "*"
```

---

## 8. Bước 6: Triển Khai Code Lên Azure Function App

Có 3 cách để deploy mã nguồn lên Azure:

### Cách 1: Sử dụng Azure Functions Core Tools (Khuyên dùng - Nhanh nhất)
Từ thư mục `BattleGameFunction/`:
```powershell
cd BattleGameFunction
func azure functionapp publish func-battlegame-api
```
Hệ thống sẽ tự động build release, đóng gói zip và đẩy lên Azure. Kết quả sau khi deploy thành công:
```
Getting site publishing info...
Uploading package...
Uploading 14.2 MB...
Upload completed successfully.
Deployment completed successfully.
Functions in func-battlegame-api:
    assignasset - [httpTrigger]
        Invoke url: https://func-battlegame-api.azurewebsites.net/api/assignasset
    createasset - [httpTrigger]
        Invoke url: https://func-battlegame-api.azurewebsites.net/api/createasset
    getassets - [httpTrigger]
        Invoke url: https://func-battlegame-api.azurewebsites.net/api/assets
    getassetsbyplayer - [httpTrigger]
        Invoke url: https://func-battlegame-api.azurewebsites.net/api/getassetsbyplayer
    getplayers - [httpTrigger]
        Invoke url: https://func-battlegame-api.azurewebsites.net/api/players
    registerplayer - [httpTrigger]
        Invoke url: https://func-battlegame-api.azurewebsites.net/api/registerplayer
```

---

### Cách 2: Triển Khai Qua VS Code (Giao Diện Trực Quan)
1. Mở thư mục dự án trong **VS Code**.
2. Nhấn vào biểu tượng **Azure** ở thanh sidebar bên trái.
3. Trong mục **WORKSPACE**, chọn icon Azure Functions -> **Deploy to Function App...**.
4. Chọn Subscription của bạn -> Chọn Function App `func-battlegame-api`.
5. Xác nhận **Deploy**. Theo dõi tiến trình trong tab Output.

---

### Cách 3: Tự Động Hóa CI/CD Qua GitHub Actions
Tạo file `.github/workflows/deploy.yml`:
```yaml
name: Deploy BattleGame Function to Azure

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
        
    - name: Build Project
      run: |
        cd BattleGameFunction
        dotnet build --configuration Release
        
    - name: Deploy to Azure Functions
      uses: Azure/functions-action@v1
      with:
        app-name: 'func-battlegame-api'
        package: 'BattleGameFunction/bin/Release/net8.0/publish'
        publish-profile: ${{ secrets.AZURE_FUNCTIONAPP_PUBLISH_PROFILE }}
```

---

## 9. Bước 7: Kiểm Thử Các Endpoints Trên Cloud

Sau khi deploy, thay `localhost:7071` bằng domain Azure:
`https://func-battlegame-api.azurewebsites.net/api/`

### 1. Test API 3: `getassetsbyplayer`
```powershell
curl -X GET "https://func-battlegame-api.azurewebsites.net/api/getassetsbyplayer"
```
Kết quả trả về danh sách JSON đúng theo bảng yêu cầu bài thi.

### 2. Test API 1: `registerplayer`
```powershell
curl -X POST "https://func-battlegame-api.azurewebsites.net/api/registerplayer" \
  -H "Content-Type: application/json" \
  -d '{
    "playerName": "Player 10",
    "fullName": "Le Van Test",
    "age": "22",
    "level": 15,
    "email": "player10@battlegame.com"
  }'
```

### 3. Test API 2: `createasset`
```powershell
curl -X POST "https://func-battlegame-api.azurewebsites.net/api/createasset" \
  -H "Content-Type: application/json" \
  -d '{
    "assetName": "Thunder Blade",
    "levelRequire": 10
  }'
```

---

## 10. Bước 8: Giám Sát và Khắc Phục Lỗi (Troubleshooting & Monitoring)

- **Xem Live Logs theo thời gian thực**:
  ```powershell
  func azure functionapp logstream func-battlegame-api
  ```
- **Kiểm tra Application Insights**:
  Vào Azure Portal -> `func-battlegame-api` -> **Application Insights** -> **Transaction search** / **Failures** để theo dõi vết lỗi và hiệu năng.
- **Lỗi 500 khi kết nối CSDL**:
  Kiểm tra lại Firewall Rule của Azure Database for MySQL Flexible Server xem đã bật tùy chọn `Allow public access from any Azure service within Azure to this server` chưa.
- **Lỗi CORS khi gọi từ trình duyệt**:
  Kiểm tra lại mục **CORS** trên Function App trong Azure Portal hoặc chạy lại lệnh `az functionapp cors add --name func-battlegame-api --resource-group rg-battlegame-eastasia --allowed-origins "*"`.
