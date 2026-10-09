-- ============================================================================
-- PROJECT: BATTLEGAME - Microsoft Azure Solutions
-- FILE: schema.sql
-- DESCRIPTION: Database initialization script for Azure SQL / SQL Server
-- TABLES: Player, Asset, PlayerAsset
-- ============================================================================

-- 1. Create Database if not exists (Uncomment if running on local SQL Server instance)
-- IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'BATTLEGAME')
-- BEGIN
--     CREATE DATABASE BATTLEGAME;
-- END
-- GO
-- USE BATTLEGAME;
-- GO

-- Drop tables if already exist (order respects foreign key constraints)
IF OBJECT_ID('dbo.PlayerAsset', 'U') IS NOT NULL DROP TABLE dbo.PlayerAsset;
IF OBJECT_ID('dbo.Asset', 'U') IS NOT NULL DROP TABLE dbo.Asset;
IF OBJECT_ID('dbo.Player', 'U') IS NOT NULL DROP TABLE dbo.Player;
GO

-- 2. Create Table: Player
CREATE TABLE dbo.Player (
    PlayerId    UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Player PRIMARY KEY DEFAULT NEWID(),
    PlayerName  NVARCHAR(64)      NOT NULL,
    FullName    NVARCHAR(128)     NULL,
    Age         NVARCHAR(10)      NULL,
    [Level]     INT               NOT NULL DEFAULT 1,
    Email       NVARCHAR(64)      NULL,
    CreatedAt   DATETIME2         NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- 3. Create Table: Asset
CREATE TABLE dbo.Asset (
    AssetId      UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Asset PRIMARY KEY DEFAULT NEWID(),
    AssetName    NVARCHAR(64)     NOT NULL,
    LevelRequire INT              NOT NULL DEFAULT 1,
    CreatedAt    DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- 4. Create Table: PlayerAsset (Junction Table)
CREATE TABLE dbo.PlayerAsset (
    PlayerId   UNIQUEIDENTIFIER NOT NULL,
    AssetId    UNIQUEIDENTIFIER NOT NULL,
    AcquiredAt DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_PlayerAsset PRIMARY KEY (PlayerId, AssetId),
    CONSTRAINT FK_PlayerAsset_Player FOREIGN KEY (PlayerId) 
        REFERENCES dbo.Player(PlayerId) ON DELETE CASCADE,
    CONSTRAINT FK_PlayerAsset_Asset FOREIGN KEY (AssetId) 
        REFERENCES dbo.Asset(AssetId) ON DELETE CASCADE
);
GO

-- 5. Insert Mock Data for Testing & Demonstration
-- Fixed GUIDs for consistent seeding
DECLARE @Player1 UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @Player2 UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @Player3 UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333';
DECLARE @Player4 UNIQUEIDENTIFIER = '44444444-4444-4444-4444-444444444444';
DECLARE @Player5 UNIQUEIDENTIFIER = '55555555-5555-5555-5555-555555555555';

DECLARE @Asset1 UNIQUEIDENTIFIER = 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA';
DECLARE @Asset2 UNIQUEIDENTIFIER = 'BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB';
DECLARE @Asset3 UNIQUEIDENTIFIER = 'CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC';
DECLARE @Asset4 UNIQUEIDENTIFIER = 'DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD';
DECLARE @Asset5 UNIQUEIDENTIFIER = 'EEEEEEEE-EEEE-EEEE-EEEE-EEEEEEEEEEEE';
DECLARE @Asset6 UNIQUEIDENTIFIER = 'FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF';

-- Insert Players
INSERT INTO dbo.Player (PlayerId, PlayerName, FullName, Age, [Level], Email)
VALUES 
    (@Player1, N'ShadowHunter', N'Nguyen Van An',   N'24', 45, N'an.nguyen@battlegame.io'),
    (@Player2, N'MysticMage',   N'Tran Thi Mai',    N'21', 38, N'mai.tran@battlegame.io'),
    (@Player3, N'IronVanguard', N'Le Hoang Long',   N'28', 60, N'long.le@battlegame.io'),
    (@Player4, N'CyberNinja',   N'Pham Minh Duc',   N'19', 15, N'duc.pham@battlegame.io'),
    (@Player5, N'PhoenixQueen', N'Vo Hoang Yen',    N'26', 52, N'yen.vo@battlegame.io');

-- Insert Assets
INSERT INTO dbo.Asset (AssetId, AssetName, LevelRequire)
VALUES 
    (@Asset1, N'Dragon Slayer Sword',   40),
    (@Asset2, N'Staff of Arcane Light', 35),
    (@Asset3, N'Titanium Aegis Shield', 50),
    (@Asset4, N'Shadow Walker Boots',   15),
    (@Asset5, N'Phoenix Wings Armor',   50),
    (@Asset6, N'Frostbite Crossbow',    25);

-- Insert PlayerAsset Associations
INSERT INTO dbo.PlayerAsset (PlayerId, AssetId)
VALUES 
    (@Player1, @Asset1), -- ShadowHunter -> Dragon Slayer Sword
    (@Player1, @Asset4), -- ShadowHunter -> Shadow Walker Boots
    (@Player2, @Asset2), -- MysticMage -> Staff of Arcane Light
    (@Player2, @Asset4), -- MysticMage -> Shadow Walker Boots
    (@Player3, @Asset1), -- IronVanguard -> Dragon Slayer Sword
    (@Player3, @Asset3), -- IronVanguard -> Titanium Aegis Shield
    (@Player4, @Asset4), -- CyberNinja -> Shadow Walker Boots
    (@Player5, @Asset5), -- PhoenixQueen -> Phoenix Wings Armor
    (@Player5, @Asset6), -- PhoenixQueen -> Frostbite Crossbow
    (@Player5, @Asset2); -- PhoenixQueen -> Staff of Arcane Light

-- Verification Query (matches GET /api/getassetsbyplayer)
SELECT 
    ROW_NUMBER() OVER (ORDER BY p.PlayerName, a.AssetName) AS [No],
    p.PlayerName,
    p.[Level],
    p.Age,
    a.AssetName
FROM dbo.Player p
INNER JOIN dbo.PlayerAsset pa ON p.PlayerId = pa.PlayerId
INNER JOIN dbo.Asset a ON pa.AssetId = a.AssetId
ORDER BY [No];
GO
