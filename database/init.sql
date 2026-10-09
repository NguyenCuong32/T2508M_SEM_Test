-- ==========================================================
-- Database Script: BATTLEGAME
-- Subject: Developing Microsoft Azure Solutions - SET01
-- Description: Table creation, Stored Procedures, and Seed Data
-- ==========================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'BATTLEGAME')
BEGIN
    CREATE DATABASE [BATTLEGAME];
END
GO

USE [BATTLEGAME];
GO

-- Drop existing tables if re-running script
IF OBJECT_ID('dbo.PlayerAsset', 'U') IS NOT NULL DROP TABLE dbo.PlayerAsset;
IF OBJECT_ID('dbo.Asset', 'U') IS NOT NULL DROP TABLE dbo.Asset;
IF OBJECT_ID('dbo.Player', 'U') IS NOT NULL DROP TABLE dbo.Player;
GO

-- 1. Create Player table
CREATE TABLE dbo.Player (
    PlayerId UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Player_PlayerId DEFAULT NEWID(),
    PlayerName NVARCHAR(64) NOT NULL,
    FullName NVARCHAR(128) NOT NULL,
    Age NVARCHAR(10) NOT NULL,
    [Level] INT NOT NULL CONSTRAINT DF_Player_Level DEFAULT 1,
    Email NVARCHAR(64) NOT NULL,
    CONSTRAINT PK_Player PRIMARY KEY CLUSTERED (PlayerId)
);
GO

-- 2. Create Asset table
CREATE TABLE dbo.Asset (
    AssetId UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Asset_AssetId DEFAULT NEWID(),
    AssetName NVARCHAR(64) NOT NULL,
    LevelRequire INT NOT NULL CONSTRAINT DF_Asset_LevelRequire DEFAULT 1,
    CONSTRAINT PK_Asset PRIMARY KEY CLUSTERED (AssetId)
);
GO

-- 3. Create PlayerAsset junction table
CREATE TABLE dbo.PlayerAsset (
    PlayerId UNIQUEIDENTIFIER NOT NULL,
    AssetId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_PlayerAsset PRIMARY KEY CLUSTERED (PlayerId, AssetId),
    CONSTRAINT FK_PlayerAsset_Player FOREIGN KEY (PlayerId) 
        REFERENCES dbo.Player (PlayerId) ON DELETE CASCADE,
    CONSTRAINT FK_PlayerAsset_Asset FOREIGN KEY (AssetId) 
        REFERENCES dbo.Asset (AssetId) ON DELETE CASCADE
);
GO

-- Indexes for performance
CREATE NONCLUSTERED INDEX IX_Player_PlayerName ON dbo.Player (PlayerName);
CREATE NONCLUSTERED INDEX IX_Asset_AssetName ON dbo.Asset (AssetName);
CREATE NONCLUSTERED INDEX IX_PlayerAsset_AssetId ON dbo.PlayerAsset (AssetId);
GO

-- ==========================================================
-- Stored Procedures for Database-First Support
-- ==========================================================

-- Procedure: Register Player
IF OBJECT_ID('dbo.sp_RegisterPlayer', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_RegisterPlayer;
GO
CREATE PROCEDURE dbo.sp_RegisterPlayer
    @PlayerName NVARCHAR(64),
    @FullName NVARCHAR(128),
    @Age NVARCHAR(10),
    @Level INT,
    @Email NVARCHAR(64),
    @NewPlayerId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @NewPlayerId = NEWID();

    INSERT INTO dbo.Player (PlayerId, PlayerName, FullName, Age, [Level], Email)
    VALUES (@NewPlayerId, @PlayerName, @FullName, @Age, @Level, @Email);
END;
GO

-- Procedure: Create Asset
IF OBJECT_ID('dbo.sp_CreateAsset', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_CreateAsset;
GO
CREATE PROCEDURE dbo.sp_CreateAsset
    @AssetName NVARCHAR(64),
    @LevelRequire INT,
    @NewAssetId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @NewAssetId = NEWID();

    INSERT INTO dbo.Asset (AssetId, AssetName, LevelRequire)
    VALUES (@NewAssetId, @AssetName, @LevelRequire);
END;
GO

-- Procedure: Get Assets By Player (matching Exam requirement #3)
IF OBJECT_ID('dbo.sp_GetAssetsByPlayer', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_GetAssetsByPlayer;
GO
CREATE PROCEDURE dbo.sp_GetAssetsByPlayer
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        ROW_NUMBER() OVER (ORDER BY p.PlayerName ASC, a.AssetName ASC) AS [No],
        p.PlayerName AS [Player name],
        p.[Level] AS [Level],
        p.Age AS [Age],
        a.AssetName AS [Asset name],
        p.PlayerId,
        a.AssetId
    FROM dbo.Player p
    INNER JOIN dbo.PlayerAsset pa ON p.PlayerId = pa.PlayerId
    INNER JOIN dbo.Asset a ON pa.AssetId = a.AssetId
    ORDER BY [No] ASC;
END;
GO

-- ==========================================================
-- Seed Initial Sample Data matching Exam Paper Table
-- ==========================================================
-- Target output table from requirement (3):
-- No | Player name | Level | Age | Asset name
-- 1  | Player 1    | 10    | 20  | Hero 1
-- 2  | Player 2    | 3     | 19  | Hero 2
-- 3  | Player 3    | 10    | 23  | Hero 1

DECLARE @p1 UNIQUEIDENTIFIER = NEWID();
DECLARE @p2 UNIQUEIDENTIFIER = NEWID();
DECLARE @p3 UNIQUEIDENTIFIER = NEWID();

DECLARE @a1 UNIQUEIDENTIFIER = NEWID();
DECLARE @a2 UNIQUEIDENTIFIER = NEWID();

-- Insert Players
INSERT INTO dbo.Player (PlayerId, PlayerName, FullName, Age, [Level], Email)
VALUES 
    (@p1, N'Player 1', N'Le Trung Kien', N'20', 10, N'player1@battlegame.vn'),
    (@p2, N'Player 2', N'Tran Van B', N'19', 3,  N'player2@battlegame.vn'),
    (@p3, N'Player 3', N'Nguyen Thi C', N'23', 10, N'player3@battlegame.vn');

-- Insert Assets
INSERT INTO dbo.Asset (AssetId, AssetName, LevelRequire)
VALUES 
    (@a1, N'Hero 1', 10),
    (@a2, N'Hero 2', 3);

-- Map PlayerAssets
INSERT INTO dbo.PlayerAsset (PlayerId, AssetId)
VALUES 
    (@p1, @a1),
    (@p2, @a2),
    (@p3, @a1);

GO

-- Verification Query
EXEC dbo.sp_GetAssetsByPlayer;
GO
