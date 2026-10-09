# Hướng dẫn deploy Azure Functions lên Azure Cloud

Hệ thống: Azure Functions (.NET isolated, EF Core) + Azure SQL Database + React (Vite).

## 0. Chuẩn bị
- Tài khoản Azure (có subscription), đã cài [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli), .NET SDK, [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local), Node.js.
- Đăng nhập: `az login`

Đặt biến (đổi tên cho duy nhất toàn cầu):
```bash
RG=rg-battlegame
LOC=southeastasia
SA=stbattlegame$RANDOM
FUNC=func-battlegame-$RANDOM
SQLSRV=sql-battlegame-$RANDOM
SQLADMIN=sqladmin
SQLPASS='<Mat-khau-manh-cua-ban>'
```

## 1. Tạo Resource Group
```bash
az group create -n $RG -l $LOC
```

## 2. Tạo Azure SQL Database
```bash
az sql server create -g $RG -n $SQLSRV -l $LOC -u $SQLADMIN -p "$SQLPASS"
az sql db create -g $RG -s $SQLSRV -n BATTLEGAME --service-objective Basic
# Cho phép dịch vụ Azure truy cập SQL Server
az sql server firewall-rule create -g $RG -s $SQLSRV -n AllowAzure --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
```
Muốn chạy script từ máy cá nhân: thêm firewall rule cho IP của bạn.

## 3. Tạo Storage Account và Function App
```bash
az storage account create -g $RG -n $SA -l $LOC --sku Standard_LRS
az functionapp create -g $RG -n $FUNC --storage-account $SA \
  --consumption-plan-location $LOC --runtime dotnet-isolated \
  --runtime-version 10 --functions-version 4 --os-type Linux
```
> Nếu region/CLI chưa hỗ trợ .NET 10, đổi `<TargetFramework>` trong `Api/BattleGame.Api.csproj` sang `net8.0` (và các package `10.*` sang `8.*`), rồi dùng `--runtime-version 8`.

## 4. Cấu hình connection string và CORS
```bash
CONN="Server=tcp:$SQLSRV.database.windows.net,1433;Database=BATTLEGAME;User ID=$SQLADMIN;Password=$SQLPASS;Encrypt=True;TrustServerCertificate=False;"
az functionapp config connection-string set -g $RG -n $FUNC \
  --connection-string-type SQLAzure --settings BattleGameDb="$CONN"
# Thay bằng domain frontend thật khi đã deploy web
az functionapp cors add -g $RG -n $FUNC --allowed-origins https://<domain-frontend>
```
Khi Function khởi động lần đầu, `Database.Migrate()` tự tạo bảng `Player`, `Asset`, `PlayerAsset` và dữ liệu mẫu. Hoặc chạy thủ công file `Database/create_database.sql` bằng Azure Data Studio / SSMS.

## 5. Deploy code
```bash
cd BattleGame/Api
func azure functionapp publish $FUNC
```
Cách khác: VS Code → extension *Azure Functions* → *Deploy to Function App*; hoặc GitHub Actions (Deployment Center → GitHub).

## 6. Kiểm thử
```bash
URL=https://$FUNC.azurewebsites.net/api
curl -X POST $URL/registerplayer -H "Content-Type: application/json" \
  -d '{"playerName":"Hero99","fullName":"Nguyen Van D","age":"21","level":5,"email":"d@game.com"}'
curl -X POST $URL/createasset -H "Content-Type: application/json" \
  -d '{"assetName":"Sword","levelRequire":2}'
curl $URL/getassetsbyplayer
```

## 7. Deploy website (React)
1. Sửa `Web/.env`: `VITE_API_BASE_URL=https://<FUNC>.azurewebsites.net/api`
2. `cd BattleGame/Web && npm install && npm run build`
3. Đưa thư mục `dist/` lên Azure Static Web Apps:
   ```bash
   az staticwebapp create -g $RG -n swa-battlegame -l eastasia
   npx @azure/static-web-apps-cli deploy ./dist --deployment-token <token>
   ```
4. Thêm domain của Static Web App vào CORS của Function App (bước 4).

## 8. Dọn dẹp
```bash
az group delete -n $RG --yes
```
