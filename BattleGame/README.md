# BattleGame

| Thư mục | Nội dung |
|---|---|
| `Api/` | Azure Functions (.NET, EF Core Code First): `registerplayer`, `createasset`, `getassetsbyplayer` |
| `Api/Migrations/` | EF migration tạo DB `BATTLEGAME` |
| `Database/create_database.sql` | Script SQL tạo DB (sinh từ migration, kèm dữ liệu mẫu) |
| `Web/` | React + Vite hiển thị báo cáo tài sản theo người chơi |
| `DEPLOYMENT.md` | Hướng dẫn deploy lên Azure |

## Chạy local
```bash
# API (cần SQL Server LocalDB hoặc sửa ConnectionStrings trong Api/local.settings.json, và Azure Functions Core Tools)
cd Api && func start            # http://localhost:7071/api

# Web
cd Web && npm install && npm run dev
```

## API
| Method | Route | Body | Kết quả |
|---|---|---|---|
| POST | `/api/registerplayer` | `{playerName, fullName, age, level, email}` | 201 player / 400 / 409 trùng tên hoặc email |
| POST | `/api/createasset` | `{assetName, levelRequire}` | 201 asset / 400 |
| GET | `/api/getassetsbyplayer` | – | `[{no, playerName, level, age, assetName}]` |
