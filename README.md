# BATTLEGAME - Developing Microsoft Azure Solutions (SET01)

- **Subject:** Developing Microsoft Azure Solutions - SET01 Practical Paper
- **Duration:** 60 minutes | Marks: 15 / 15
- **Student Name:** Lê Trung Kiên
- **Class / Branch:** PRAM_LeTrungKien (T2508M)
- **Architecture:** 3-Tier Enterprise Pattern (Controllers / Functions ➔ Services ➔ Repositories ➔ Entity Framework Core 8)

---

## 📌 Project Structure

```text
├── database/
│   └── init.sql                       # Database creation script (Tables, Constraints, SP, Seed Data)
├── backend/
│   └── BattleGame.Functions/          # Azure Functions v4 (.NET 8 Isolated Worker)
│       ├── Data/
│       │   └── BattleGameDbContext.cs # EF Core DbContext with Fluent API mappings
│       ├── Models/                    # Entity models (Player, Asset, PlayerAsset)
│       ├── DTOs/                      # Request & Response models (Validation attributes)
│       │   ├── RegisterPlayerRequest.cs
│       │   ├── CreateAssetRequest.cs
│       │   ├── PlayerAssetReportItem.cs
│       │   ├── AssignAssetRequest.cs
│       │   └── ApiResponse.cs
│       ├── Repositories/              # Data Access Layer (Repository Pattern)
│       │   ├── Interfaces/
│       │   │   ├── IPlayerRepository.cs
│       │   │   ├── IAssetRepository.cs
│       │   │   └── IPlayerAssetRepository.cs
│       │   ├── PlayerRepository.cs
│       │   ├── AssetRepository.cs
│       │   └── PlayerAssetRepository.cs
│       ├── Services/                  # Business Logic Layer (Service Pattern)
│       │   ├── Interfaces/
│       │   │   ├── IPlayerService.cs
│       │   │   ├── IAssetService.cs
│       │   │   └── IPlayerAssetService.cs
│       │   ├── PlayerService.cs
│       │   ├── AssetService.cs
│       │   └── PlayerAssetService.cs
│       ├── Functions/                 # Controller / HTTP Trigger Layer
│       │   ├── RegisterPlayerFunction.cs   # API: registerplayer (POST) - Req 1
│       │   ├── CreateAssetFunction.cs      # API: createasset (POST) - Req 2
│       │   ├── GetAssetsByPlayerFunction.cs# API: getassetsbyplayer (GET) - Req 3
│       │   ├── AssignAssetFunction.cs      # API: assignasset (POST)
│       │   └── SeedDataFunction.cs         # API: seeddata (GET/POST)
│       ├── Program.cs                 # Dependency Injection & DI Service Lifetime Configuration
│       ├── host.json                  # Azure Function host configuration
│       └── local.settings.json        # Local settings & connection strings
├── frontend/                          # React + Vite Web Application (Req 4)
│   ├── src/
│   │   ├── services/api.js            # API client with fallback support
│   │   ├── App.jsx                    # Modern UI with Table & Forms
│   │   ├── index.css                  # Modern gaming dark UI styles
│   │   └── main.jsx
│   └── package.json
├── docs/
│   └── DEPLOYMENT.md                  # Azure Cloud Deployment Tutorial (Req 5)
├── BattleGame.slnx                    # Solution file
└── README.md
```

---

## 🏛️ Architectural Design (Repository - Service - Controller)

1. **Controller / Function Layer (`Functions/`)**:
   - Nhận HTTP Request từ Client / Frontend.
   - Deserialize JSON payload vào Request DTO.
   - Gọi trực tiếp xuống `Service` tương ứng (không truy cập trực tiếp Database).
   - Chuyển đổi kết quả / ngoại lệ nghiệp vụ thành HTTP Response chuẩn (`200 OK`, `201 Created`, `400 Bad Request`, `409 Conflict`, `404 Not Found`).

2. **Service Layer (`Services/`)**:
   - Chứa toàn bộ Business Logic và quy tắc xác thực nghiệp vụ (validation, kiểm tra trùng lặp người chơi, tên tài sản, cấp độ yêu cầu).
   - Gọi xuống tầng `Repository` để lưu trữ và truy vấn dữ liệu.

3. **Repository Layer (`Repositories/`)**:
   - Tách biệt hoàn toàn tầng lưu trữ dữ liệu (Data Access Layer).
   - Tương tác với SQL Server thông qua `BattleGameDbContext` (EF Core).
   - Hỗ trợ các thao tác CRUD, JOIN giữa các bảng `PlayerAsset`, `Player`, `Asset`.

4. **Dependency Injection Container (`Program.cs`)**:
   - Tất cả Repository và Service đều được đăng ký Scoped:
     - `builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();`
     - `builder.Services.AddScoped<IAssetRepository, AssetRepository>();`
     - `builder.Services.AddScoped<IPlayerAssetRepository, PlayerAssetRepository>();`
     - `builder.Services.AddScoped<IPlayerService, PlayerService>();`
     - `builder.Services.AddScoped<IAssetService, AssetService>();`
     - `builder.Services.AddScoped<IPlayerAssetService, PlayerAssetService>();`

---

## 🎯 Examination Requirements & Implementation

| No | Requirement | Component / File | Status | Mark |
|:---|:---|:---|:---:|:---:|
| 1 | **Create Database** (Code First / Database First) | [`database/init.sql`](file:///c:/Users/admin/Downloads/PRAM/T2508M_SEM_Test/database/init.sql), [`BattleGameDbContext.cs`](file:///c:/Users/admin/Downloads/PRAM/T2508M_SEM_Test/backend/BattleGame.Functions/Data/BattleGameDbContext.cs) | ✅ Completed | 1 |
| 2 | **API `registerplayer`** (Post player to Player table) | [`RegisterPlayerFunction.cs`](file:///c:/Users/admin/Downloads/PRAM/T2508M_SEM_Test/backend/BattleGame.Functions/Functions/RegisterPlayerFunction.cs), [`PlayerService.cs`](file:///c:/Users/admin/Downloads/PRAM/T2508M_SEM_Test/backend/BattleGame.Functions/Services/PlayerService.cs) | ✅ Completed | 3 |
| 3 | **API `createasset`** (Post asset to Asset table) | [`CreateAssetFunction.cs`](file:///c:/Users/admin/Downloads/PRAM/T2508M_SEM_Test/backend/BattleGame.Functions/Functions/CreateAssetFunction.cs), [`AssetService.cs`](file:///c:/Users/admin/Downloads/PRAM/T2508M_SEM_Test/backend/BattleGame.Functions/Services/AssetService.cs) | ✅ Completed | 3 |
| 4 | **API `getassetsbyplayer`** (Report table) | [`GetAssetsByPlayerFunction.cs`](file:///c:/Users/admin/Downloads/PRAM/T2508M_SEM_Test/backend/BattleGame.Functions/Functions/GetAssetsByPlayerFunction.cs), [`PlayerAssetService.cs`](file:///c:/Users/admin/Downloads/PRAM/T2508M_SEM_Test/backend/BattleGame.Functions/Services/PlayerAssetService.cs) | ✅ Completed | 3 |
| 5 | **Web Application** (Display table in Req 3) | [`frontend/`](file:///c:/Users/admin/Downloads/PRAM/T2508M_SEM_Test/frontend/) (React + Vite) | ✅ Completed | 3 |
| 6 | **Document Deploy Tutorial** | [`docs/DEPLOYMENT.md`](file:///c:/Users/admin/Downloads/PRAM/T2508M_SEM_Test/docs/DEPLOYMENT.md) | ✅ Completed | 1 |
| 7 | **Bonus Good Coding Convention** (Repository + Service Pattern, DI, Validation, CORS) | Entire Project Architecture | ✅ Completed | 1 |
| **TOTAL** | | | | **15 / 15** |

---

## 🚀 How to Run Locally

### 1. Initialize the Database
Open PowerShell and execute:
```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -i "database\init.sql"
```
The database `BATTLEGAME` will be created with tables `Player`, `Asset`, `PlayerAsset`, and initial seed data matching the exam paper.

---

### 2. Run the Azure Functions Backend
Ensure .NET SDK is available, navigate to the function project, and start:
```powershell
cd backend\BattleGame.Functions
func start --port 7071
```

Available API Endpoints:
- `POST http://localhost:7071/api/registerplayer` - Register a player
- `POST http://localhost:7071/api/createasset` - Create a new game asset
- `GET  http://localhost:7071/api/getassetsbyplayer` - Query assets report
- `POST http://localhost:7071/api/assignasset` - Link player with asset
- `GET  http://localhost:7071/api/seeddata` - Seed initial test data

---

### 3. Run the Frontend Web Application
In a separate terminal:
```powershell
cd frontend
npm install
npm run dev
```
Open `http://localhost:5173` in your browser. The web application displays:
- Requirement 3 report table: `No`, `Player name`, `Level`, `Age`, `Asset name`.
- Interactive testing forms for `registerplayer` and `createasset`.
- In-app tutorial for Azure Cloud Deployment.

---

## 📖 Deployment Documentation
Read [`docs/DEPLOYMENT.md`](file:///c:/Users/admin/Downloads/PRAM/T2508M_SEM_Test/docs/DEPLOYMENT.md) for step-by-step instructions on deploying the Azure Functions App and Azure SQL Database to Microsoft Azure Cloud.
