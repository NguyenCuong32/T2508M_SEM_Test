# Tutorial: Deploying BattleGame Azure Function to Microsoft Azure Cloud

This comprehensive deployment guide covers deploying the **BattleGame Solution** (SET01 Practical Exam) to Microsoft Azure Cloud.

---

## 1. System Architecture Overview

```
[ Web Frontend (React) ]
          |
          | HTTPS (REST API)
          v
[ Azure Functions App ] (v4 .NET 8 Isolated Worker)
   ├── registerplayer (POST)
   ├── createasset (POST)
   └── getassetsbyplayer (GET)
          |
          | ADO.NET / EF Core (Port 1433, TLS Encrypted)
          v
[ Azure SQL Database ] (Database: BATTLEGAME)
   ├── Player Table
   ├── Asset Table
   └── PlayerAsset Junction Table
```

---

## 2. Prerequisites

1. An active **Microsoft Azure Subscription** (Free tier or Student subscription).
2. **Azure CLI** installed (`az --version`).
3. **Azure Functions Core Tools v4** installed (`func --version`).
4. **.NET 8 SDK** installed (`dotnet --version`).
5. SQL Server Management Studio (SSMS) or Azure Data Studio.

Login to Azure CLI:
```bash
az login
```

Set target subscription (if you have multiple):
```bash
az account set --subscription "<SUBSCRIPTION_ID_OR_NAME>"
```

---

## 3. Step 1: Create Resource Group & Azure Storage Account

Create a dedicated resource group in your preferred region (e.g., `southeastasia` or `eastasia`):

```bash
# 1. Create Resource Group
az group create --name rg-battlegame --location southeastasia

# 2. Create Storage Account (required by Azure Function host)
az storage account create \
  --name stbattlegameprod \
  --resource-group rg-battlegame \
  --location southeastasia \
  --sku Standard_LRS
```

---

## 4. Step 2: Provision Azure SQL Database

The game database requires database `BATTLEGAME` with tables `Player`, `Asset`, and `PlayerAsset`.

### 4.1. Create Azure SQL Logical Server & Database
```bash
# 1. Create SQL Server
az sql server create \
  --name sql-battlegame-srv \
  --resource-group rg-battlegame \
  --location southeastasia \
  --admin-user battlegameadmin \
  --admin-password "P@ssw0rdAzure2026!"

# 2. Allow Azure Services through the Firewall
az sql server firewall-rule create \
  --resource-group rg-battlegame \
  --server sql-battlegame-srv \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# 3. Allow your current client IP (for executing scripts)
az sql server firewall-rule create \
  --resource-group rg-battlegame \
  --server sql-battlegame-srv \
  --name AllowClientIP \
  --start-ip-address <YOUR_PUBLIC_IP> \
  --end-ip-address <YOUR_PUBLIC_IP>

# 4. Create the BATTLEGAME Database (Basic tier is budget-friendly for labs)
az sql db create \
  --resource-group rg-battlegame \
  --server sql-battlegame-srv \
  --name BATTLEGAME \
  --service-objective Basic
```

### 4.2. Run the Database Initialization Script
Connect via `sqlcmd`, SSMS, or Azure Portal Query Editor and run `database/init.sql`:
```bash
sqlcmd -S "sql-battlegame-srv.database.windows.net" \
  -d "BATTLEGAME" \
  -U "battlegameadmin" \
  -P "P@ssw0rdAzure2026!" \
  -i "database/init.sql"
```

---

## 5. Step 3: Create Azure Function App

Create a serverless Consumption Function App targeting .NET 8 Isolated Worker:

```bash
az functionapp create \
  --resource-group rg-battlegame \
  --name func-battlegame-api \
  --storage-account stbattlegameprod \
  --consumption-plan-location southeastasia \
  --functions-version 4 \
  --os-type Windows \
  --runtime dotnet-isolated \
  --runtime-version 8.0
```

---

## 6. Step 4: Configure App Settings & Connection String

Set the `SqlConnectionString` in Azure Function Application Settings so EF Core connects to Azure SQL Database securely:

```bash
AZURE_SQL_CONN="Server=tcp:sql-battlegame-srv.database.windows.net,1433;Initial Catalog=BATTLEGAME;Persist Security Info=False;User ID=battlegameadmin;Password=P@ssw0rdAzure2026!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

az functionapp config appsettings set \
  --resource-group rg-battlegame \
  --name func-battlegame-api \
  --settings "SqlConnectionString=$AZURE_SQL_CONN"
```

---

## 7. Step 5: Configure CORS for Web Application

To allow web frontends (React / Angular) to communicate with Azure Functions without browser CORS restrictions:

```bash
# Allow all origins (or specify your frontend URL)
az functionapp cors add \
  --resource-group rg-battlegame \
  --name func-battlegame-api \
  --allowed-origins "*"
```

---

## 8. Step 6: Deploy Azure Function Code

### Option A: Using Azure Functions Core Tools (CLI - Recommended)
Navigate to the Function project folder and publish:
```bash
cd backend/BattleGame.Functions
func azure functionapp publish func-battlegame-api
```

### Option B: Using Visual Studio Code
1. Install **Azure Functions** and **Azure Resources** extensions in VS Code.
2. Sign in to your Azure account in the Azure sidebar tab.
3. Right-click on the `backend/BattleGame.Functions` project folder.
4. Select **Deploy to Function App...**.
5. Select subscription and pick `func-battlegame-api`.
6. Confirm deployment and view progress in the Output panel.

### Option C: Using GitHub Actions (CI/CD)
Create `.github/workflows/deploy-functions.yml`:
```yaml
name: Deploy Azure Functions
on:
  push:
    branches: [ PRAM_LeTrungKien ]

jobs:
  build-and-deploy:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - name: Publish Project
        run: dotnet publish backend/BattleGame.Functions/BattleGame.Functions.csproj -c Release -o ./publish
      - name: Deploy to Azure Functions
        uses: Azure/functions-action@v1
        with:
          app-name: 'func-battlegame-api'
          package: './publish'
          publish-profile: ${{ secrets.AZURE_FUNCTIONAPP_PUBLISH_PROFILE }}
```

---

## 9. Step 7: Verify Cloud Endpoints

Once deployed, retrieve the live URLs:
```bash
az functionapp function show \
  --resource-group rg-battlegame \
  --name func-battlegame-api \
  --function-name getassetsbyplayer \
  --query "invokeUrlTemplate" -o tsv
```

### Test Cloud APIs:
1. **GET Player Asset Report**:
   ```bash
   curl -X GET "https://func-battlegame-api.azurewebsites.net/api/getassetsbyplayer"
   ```
2. **POST Register Player**:
   ```bash
   curl -X POST "https://func-battlegame-api.azurewebsites.net/api/registerplayer" \
     -H "Content-Type: application/json" \
     -d '{"playerName":"Player 5","fullName":"Nguyen Van E","age":"22","level":4,"email":"player5@battlegame.vn"}'
   ```
3. **POST Create Asset**:
   ```bash
   curl -X POST "https://func-battlegame-api.azurewebsites.net/api/createasset" \
     -H "Content-Type: application/json" \
     -d '{"assetName":"Hero 4 - Shadow Assassin","levelRequire":7}'
   ```

---

## 10. Step 8: Deploy Frontend to Azure Static Web Apps

Deploy the React frontend using **Azure Static Web Apps**:

```bash
# 1. Build the frontend
cd frontend
npm run build

# 2. Deploy using Azure CLI / SWA CLI
az staticwebapp create \
  --name swa-battlegame \
  --resource-group rg-battlegame \
  --location southeastasia \
  --source https://github.com/NguyenCuong32/T2508M_SEM_Test \
  --branch PRAM_LeTrungKien \
  --app-location "frontend" \
  --output-location "dist"
```

---

## 11. Troubleshooting Checklist

| Issue | Root Cause | Solution |
| :--- | :--- | :--- |
| `Cannot open server "..." requested by the login.` | Firewall blocking client / Azure IP | Enable "Allow Azure services to access server" in Azure SQL Server firewall settings. |
| `HTTP 500: Login failed for user` | Invalid SQL credentials or missing setting | Verify `SqlConnectionString` in Azure Function Configuration -> Application settings. |
| `CORS Error in Browser` | Function App does not permit client origin | Add `*` or the frontend origin under `API -> CORS` in Azure Portal. |
| Function cold start latency | Serverless consumption plan spin-up | Keep-alive ping or upgrade to Premium Elastic plan (`EP1`) if strict low latency is required. |
