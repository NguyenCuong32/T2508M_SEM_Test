-- ========================================================
-- Database Name: BATTLEGAME
-- Mô tả: Khởi tạo database và cấu trúc các bảng theo sơ đồ
-- ========================================================

USE master;
GO

-- 1. Tạo Database BATTLEGAME nếu chưa có
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'BATTLEGAME')
BEGIN
    CREATE DATABASE BATTLEGAME;
END
GO

USE BATTLEGAME;
GO

-- ========================================================
-- 2. Xóa bảng cũ nếu đã tồn tại (theo thứ tự khóa ngoại)
-- ========================================================
IF OBJECT_ID(N'dbo.PlayerAsset', N'U') IS NOT NULL DROP TABLE dbo.PlayerAsset;
IF OBJECT_ID(N'dbo.Player', N'U') IS NOT NULL DROP TABLE dbo.Player;
IF OBJECT_ID(N'dbo.Asset', N'U') IS NOT NULL DROP TABLE dbo.Asset;
GO

-- ========================================================
-- 3. Tạo bảng Asset (Định danh và thuộc tính tài sản)
-- ========================================================
CREATE TABLE dbo.Asset (
    AssetId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    AssetName NVARCHAR(64) NOT NULL,
    LevelRequire INT NOT NULL DEFAULT 1,
    CONSTRAINT PK_Asset PRIMARY KEY (AssetId)
);
GO

-- ========================================================
-- 4. Tạo bảng Player (Thông tin người chơi)
-- ========================================================
CREATE TABLE dbo.Player (
    PlayerId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    PlayerName NVARCHAR(64) NOT NULL,
    FullName NVARCHAR(128) NULL,
    Age NVARCHAR(10) NULL,
    [Level] INT NOT NULL DEFAULT 1,
    Email NVARCHAR(64) NULL,
    CONSTRAINT PK_Player PRIMARY KEY (PlayerId)
);
GO

-- ========================================================
-- 5. Tạo bảng PlayerAsset (Bảng trung gian n - n giữa Player và Asset)
-- ========================================================
CREATE TABLE dbo.PlayerAsset (
    PlayerId UNIQUEIDENTIFIER NOT NULL,
    AssetId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_PlayerAsset PRIMARY KEY (PlayerId, AssetId),
    CONSTRAINT FK_PlayerAsset_Player FOREIGN KEY (PlayerId) 
        REFERENCES dbo.Player (PlayerId) ON DELETE CASCADE,
    CONSTRAINT FK_PlayerAsset_Asset FOREIGN KEY (AssetId) 
        REFERENCES dbo.Asset (AssetId) ON DELETE CASCADE
);
GO

-- ========================================================
-- 6. Dữ liệu mẫu (Insert Sample Data để kiểm tra)
-- ========================================================
DECLARE @P1 UNIQUEIDENTIFIER = NEWID();
DECLARE @P2 UNIQUEIDENTIFIER = NEWID();

DECLARE @A1 UNIQUEIDENTIFIER = NEWID();
DECLARE @A2 UNIQUEIDENTIFIER = NEWID();
DECLARE @A3 UNIQUEIDENTIFIER = NEWID();

-- Thêm người chơi mẫu
INSERT INTO dbo.Player (PlayerId, PlayerName, FullName, Age, [Level], Email)
VALUES 
(@P1, N'DragonSlayer', N'Nguyễn Văn A', N'22', 15, N'slayer@example.com'),
(@P2, N'ShadowHunter', N'Trần Thị B', N'19', 5, N'hunter@example.com');

-- Thêm tài sản mẫu (Vũ khí, Tướng, Trang bị)
INSERT INTO dbo.Asset (AssetId, AssetName, LevelRequire)
VALUES 
(@A1, N'Excalibur Sword', 10),
(@A2, N'Shadow Armor', 5),
(@A3, N'Dragon Pet', 1);

-- Gán tài sản cho người chơi
INSERT INTO dbo.PlayerAsset (PlayerId, AssetId)
VALUES 
(@P1, @A1),
(@P1, @A2),
(@P2, @A2),
(@P2, @A3);
GO

-- Kiểm tra truy vấn dữ liệu
SELECT 
    p.PlayerName,
    p.FullName,
    p.[Level] AS PlayerLevel,
    a.AssetName,
    a.LevelRequire
FROM dbo.Player p
JOIN dbo.PlayerAsset pa ON p.PlayerId = pa.PlayerId
JOIN dbo.Asset a ON pa.AssetId = a.AssetId;
GO
