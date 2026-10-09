# HƯỚNG DẪN TRIỂN KHAI AZURE FUNCTION LÊN MICROSOFT AZURE CLOUD
**Dự án:** BattleGame Operations API (.NET 8 Isolated Worker)  
**Tác giả:** Fullstack & Cloud Engineer  

---

## 1. Chuẩn bị Cơ sở dữ liệu (Azure SQL Database)
1. **Tạo Database trên Azure Portal:**
   - Vào [portal.azure.com](https://portal.azure.com) > Tìm **SQL databases** > Nhấn **Create**.
   - **Database name:** `BATTLEGAME`.
   - **Server:** Tạo mới server (hoặc chọn server sẵn có), chọn Authentication là *SQL Server authentication* (tạo tài khoản `adminuser` & mật khẩu mạnh).
   - **Compute + Storage:** Chọn gói tiết kiệm chi phí (*Basic* hoặc *Serverless General Purpose*).
   - **Networking:** Tại tab Networking, bật **Allow Azure services and resources to access this server** = `Yes`, đồng thời thêm IP hiện tại của bạn vào Client IP Firewall rules.
2. **Khởi tạo dữ liệu:**
   - Dùng **Query editor (preview)** trên Azure Portal hoặc dùng **SQL Server Management Studio (SSMS)** / **Azure Data Studio**.
   - Mở file `database/schema.sql` và thực thi toàn bộ script để tạo bảng `Player`, `Asset`, `PlayerAsset` cùng dữ liệu mẫu ban đầu.
3. **Lấy Connection String:**
   - Vào SQL Database `BATTLEGAME` > **Settings** > **Connection strings** > Tab **ADO.NET**.
   - Sao chép chuỗi kết nối dạng:
     ```text
     Server=tcp:<server-name>.database.windows.net,1433;Initial Catalog=BATTLEGAME;Persist Security Info=False;User ID=<adminuser>;Password=<password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
     ```

---

## 2. Tạo Azure Function App trên Azure Portal
1. Trên Azure Portal, tìm kiếm **Function App** > Nhấn **Create**.
2. Thiết lập thông số cơ bản:
   - **Subscription:** Chọn subscription của bạn (ví dụ Azure for Students hoặc Pay-As-You-Go).
   - **Resource Group:** Chọn hoặc tạo mới (ví dụ: `rg-battlegame-prod`).
   - **Function App name:** Đặt tên duy nhất (ví dụ: `func-battlegame-api`).
   - **Deploy:** Chọn *Code*.
   - **Runtime stack:** `.NET`.
   - **Version:** `8 Isolated` (hoặc .NET 8).
   - **Operating System:** `Windows` hoặc `Linux`.
   - **Hosting plan:** `Consumption (Serverless)` để tối ưu chi phí học tập/thực hành.
3. Nhấn **Review + create** > **Create** và chờ tài nguyên khởi tạo hoàn tất.

---

## 3. Cấu hình Connection String & CORS trên Azure Portal
Sau khi Function App tạo xong, truy cập vào Function App đó:

### 3.1. Cấu hình Chuỗi kết nối (Environment Variables)
1. Ở menu bên trái, vào **Settings** > **Environment variables** (hoặc **Configuration**).
2. Tại tab **App settings**, nhấn **+ Add**:
   - **Name:** `SqlConnectionString`
   - **Value:** Dán chuỗi kết nối Azure SQL đã lấy ở Bước 1 (thay password thật).
3. Nhấn **Apply** > **Confirm** để lưu.

### 3.2. Cấu hình CORS (Cho phép Frontend gọi API)
1. Ở menu bên trái, tìm mục **API** > **CORS**.
2. Thêm Allowed Origins:
   - Thêm `*` (để cho phép mọi domain trong quá trình test/chấm điểm).
   - Hoặc thêm URL cụ thể của Frontend (ví dụ `http://localhost:5173`, `https://your-frontend.azurestaticapps.net`).
3. Tích chọn **Enable Access-Control-Allow-Credentials** nếu cần.
4. Nhấn **Save**.

---

## 4. Deploy (Publish) Dự án từ Visual Studio hoặc VS Code

### Cách 1: Publish từ Visual Studio 2022 (Khuyên dùng)
1. Mở thư mục `backend` bằng Visual Studio 2022 (hoặc mở file `BattleGame.Api.csproj`).
2. Trong cửa sổ **Solution Explorer**, chuột phải vào project `BattleGame.Api` > chọn **Publish...**.
3. Tại cửa sổ Publish:
   - **Target:** Chọn **Azure** > Nhấn **Next**.
   - **Specific target:** Chọn **Azure Function App (Windows)** hoặc **(Linux)** tương ứng với cấu hình ở Bước 2.
   - **Function Instances:** Đăng nhập tài khoản Microsoft Azure > Chọn đúng Resource Group và Function App (`func-battlegame-api`) vừa tạo.
4. Nhấn **Finish**.
5. Nhấn nút **Publish** màu xanh ở góc trên. Visual Studio sẽ tự động build release và đẩy toàn bộ mã nguồn lên Azure Function App.

### Cách 2: Publish từ Visual Studio Code
1. Cài extension **Azure Functions** và **Azure Resources** trong VS Code.
2. Mở thư mục `backend` trong VS Code.
3. Mở tab **Azure** ở thanh bên trái > Đăng nhập tài khoản Azure (`Sign in to Azure...`).
4. Tại mục **Workspace (local)**, rê chuột vào biểu tượng đám mây có mũi tên hướng lên (**Deploy to Azure...**).
5. Chọn Function App đã tạo trên Cloud (`func-battlegame-api`).
6. Xác nhận triển khai ("Deploy"). Quá trình publish hoàn tất sẽ hiển thị thông báo với URL của Function App.

### Cách 3: Publish bằng Azure Functions Core Tools (CLI)
Nếu có `azure-functions-core-tools` trên máy, mở terminal tại thư mục `backend`:
```powershell
func azure functionapp publish func-battlegame-api --csharp
```

---

## 5. Kiểm thử các API đã Publish
Sau khi deploy thành công, các API của bạn sẽ có dạng:
- `https://<your-function-app-name>.azurewebsites.net/api/getassetsbyplayer`
- `https://<your-function-app-name>.azurewebsites.net/api/registerplayer`
- `https://<your-function-app-name>.azurewebsites.net/api/createasset`

### Kiểm tra bằng Browser / Postman:
1. **GET Query:** Mở trình duyệt truy cập:
   ```text
   https://<your-function-app-name>.azurewebsites.net/api/getassetsbyplayer
   ```
   Kết quả trả về danh sách JSON gồm 5 trường: `No`, `PlayerName`, `Level`, `Age`, `AssetName`.
2. **POST Register Player:** Dùng Postman gửi `POST` tới `.../api/registerplayer`:
   ```json
   {
     "playerName": "CloudHero",
     "fullName": "Nguyen Van B",
     "age": "22",
     "level": 10,
     "email": "cloudhero@battlegame.io"
   }
   ```
3. **POST Create Asset:** Dùng Postman gửi `POST` tới `.../api/createasset`:
   ```json
   {
     "assetName": "Celestial Thunder Bow",
     "levelRequire": 30
   }
   ```

---

## 6. Cập nhật Frontend kết nối Azure Function Cloud
1. Mở giao diện Frontend (file `src/App.jsx` hoặc file `preview.html`).
2. Vào tab **API Settings**.
3. Điền URL Function App của bạn: `https://<your-function-app-name>.azurewebsites.net` và nhấn **Save & Connect**.
4. Toàn bộ bảng dữ liệu sẽ tự động fetch trực tiếp từ cloud database qua Azure Function!
