# Deploy Azure Function to Azure Cloud

## Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| Node.js | 18 LTS or 20 LTS | Runtime |
| Azure Functions Core Tools | v4 | Local dev & deploy |
| Azure CLI | latest | Azure resource management |
| An Azure subscription | – | Host resources |

Install Azure Functions Core Tools:

```bash
npm install -g azure-functions-core-tools@4 --unsafe-perm true
```

---

## Step 1 – Login to Azure

```bash
az login
```

A browser window opens. Sign in with your Azure account.  
After login, set the active subscription:

```bash
az account set --subscription "<YOUR_SUBSCRIPTION_ID>"
```

---

## Step 2 – Create Azure Resources

### 2.1 Resource Group

```bash
az group create \
  --name battlegame-rg \
  --location eastasia
```

### 2.2 Azure SQL Server + Database (BATTLEGAME)

```bash
# Create SQL Server
az sql server create \
  --name battlegame-sql-server \
  --resource-group battlegame-rg \
  --location eastasia \
  --admin-user sqladmin \
  --admin-password "YourStrongPassword@123"

# Allow Azure services to access
az sql server firewall-rule create \
  --resource-group battlegame-rg \
  --server battlegame-sql-server \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Create the database
az sql db create \
  --resource-group battlegame-rg \
  --server battlegame-sql-server \
  --name BATTLEGAME \
  --service-objective S0
```

### 2.3 Storage Account (required by Azure Functions)

```bash
az storage account create \
  --name battlegamestorage \
  --resource-group battlegame-rg \
  --location eastasia \
  --sku Standard_LRS
```

### 2.4 Function App

```bash
az functionapp create \
  --resource-group battlegame-rg \
  --consumption-plan-location eastasia \
  --runtime node \
  --runtime-version 20 \
  --functions-version 4 \
  --name battlegame-func-app \
  --storage-account battlegamestorage \
  --os-type Linux
```

---

## Step 3 – Configure Application Settings

Set the database connection environment variables on the Function App:

```bash
az functionapp config appsettings set \
  --name battlegame-func-app \
  --resource-group battlegame-rg \
  --settings \
    SQL_SERVER="battlegame-sql-server.database.windows.net" \
    SQL_DATABASE="BATTLEGAME" \
    SQL_USER="sqladmin" \
    SQL_PASSWORD="YourStrongPassword@123" \
    SQL_PORT="1433"
```

---

## Step 4 – Run Database Scripts

Connect to the Azure SQL Database using SQL Server Management Studio (SSMS) or Azure Data Studio, then execute in order:

1. `database/create-database.sql` — creates tables
2. `database/seed-data.sql`      — inserts sample data

---

## Step 5 – Build & Deploy Backend

```bash
cd backend
npm install
npm run build

# Deploy using Azure Functions Core Tools
func azure functionapp publish battlegame-func-app
```

After deployment the three API endpoints will be available at:

```
POST https://battlegame-func-app.azurewebsites.net/api/registerplayer
POST https://battlegame-func-app.azurewebsites.net/api/createasset
GET  https://battlegame-func-app.azurewebsites.net/api/getassetsbyplayer
```

---

## Step 6 – Build & Deploy Frontend (Static Web App)

### 6.1 Update API base URL

Edit `frontend/src/services/api.service.ts` and change `BASE_URL` to your deployed Function App URL:

```ts
const BASE_URL = "https://battlegame-func-app.azurewebsites.net/api";
```

### 6.2 Build the React app

```bash
cd frontend
npm install
npm run build
# Built output is in frontend/dist/
```

### 6.3 Create and deploy a Static Web App

```bash
az staticwebapp create \
  --name battlegame-web \
  --resource-group battlegame-rg \
  --source https://github.com/<your-repo> \
  --location eastasia \
  --branch main \
  --app-location "frontend" \
  --output-location "dist" \
  --login-with-github
```

Or manually deploy the `frontend/dist` folder using the Azure portal → Static Web Apps → Deploy.

---

## Step 7 – Verify Deployment

Test the APIs with curl or Postman:

```bash
# Test registerplayer
curl -X POST https://battlegame-func-app.azurewebsites.net/api/registerplayer \
  -H "Content-Type: application/json" \
  -d '{"PlayerName":"testplayer","FullName":"Test Player","Age":"22","Level":1,"Email":"test@game.com"}'

# Test createasset
curl -X POST https://battlegame-func-app.azurewebsites.net/api/createasset \
  -H "Content-Type: application/json" \
  -d '{"AssetName":"Shield","LevelRequire":3}'

# Test getassetsbyplayer
curl https://battlegame-func-app.azurewebsites.net/api/getassetsbyplayer
```

Expected response shape:

```json
{
  "success": true,
  "message": "Report fetched successfully.",
  "data": [
    { "No": 1, "PlayerName": "Player 1", "Level": 10, "Age": "20", "AssetName": "Hero 1" }
  ]
}
```

---

## Clean Up (when no longer needed)

```bash
az group delete --name battlegame-rg --yes --no-wait
```

This removes all resources in the resource group.
