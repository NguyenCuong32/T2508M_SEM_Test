IF DB_ID(N'BATTLEGAME') IS NULL
BEGIN
    CREATE DATABASE BATTLEGAME;
END;
GO

USE BATTLEGAME;
GO

IF OBJECT_ID(N'dbo.PlayerAsset', N'U') IS NOT NULL DROP TABLE dbo.PlayerAsset;
IF OBJECT_ID(N'dbo.Asset', N'U') IS NOT NULL DROP TABLE dbo.Asset;
IF OBJECT_ID(N'dbo.Player', N'U') IS NOT NULL DROP TABLE dbo.Player;
GO

CREATE TABLE dbo.Player
(
    PlayerId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Player PRIMARY KEY,
    PlayerName NVARCHAR(64) NOT NULL,
    FullName NVARCHAR(128) NOT NULL,
    Age INT NOT NULL,
    [Level] INT NOT NULL CONSTRAINT DF_Player_Level DEFAULT 1,
    Email NVARCHAR(64) NOT NULL,
    CONSTRAINT UQ_Player_PlayerName UNIQUE (PlayerName),
    CONSTRAINT UQ_Player_Email UNIQUE (Email),
    CONSTRAINT CK_Player_Age CHECK (Age BETWEEN 1 AND 120),
    CONSTRAINT CK_Player_Level CHECK ([Level] >= 1)
);

CREATE TABLE dbo.Asset
(
    AssetId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Asset PRIMARY KEY,
    AssetName NVARCHAR(64) NOT NULL,
    LevelRequire INT NOT NULL,
    CONSTRAINT UQ_Asset_AssetName UNIQUE (AssetName),
    CONSTRAINT CK_Asset_LevelRequire CHECK (LevelRequire >= 0)
);

CREATE TABLE dbo.PlayerAsset
(
    PlayerId UNIQUEIDENTIFIER NOT NULL,
    AssetId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_PlayerAsset PRIMARY KEY (PlayerId, AssetId),
    CONSTRAINT FK_PlayerAsset_Player FOREIGN KEY (PlayerId)
        REFERENCES dbo.Player(PlayerId) ON DELETE CASCADE,
    CONSTRAINT FK_PlayerAsset_Asset FOREIGN KEY (AssetId)
        REFERENCES dbo.Asset(AssetId) ON DELETE CASCADE
);
GO

INSERT INTO dbo.Player (PlayerId, PlayerName, FullName, Age, [Level], Email)
VALUES
('11111111-1111-1111-1111-111111111111', N'Player 1', N'Demo Player One', 20, 10, N'player1@example.com'),
('22222222-2222-2222-2222-222222222222', N'Player 2', N'Demo Player Two', 19, 3, N'player2@example.com'),
('33333333-3333-3333-3333-333333333333', N'Player 3', N'Demo Player Three', 23, 10, N'player3@example.com');

INSERT INTO dbo.Asset (AssetId, AssetName, LevelRequire)
VALUES
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', N'Hero 1', 1),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', N'Hero 2', 1);

INSERT INTO dbo.PlayerAsset (PlayerId, AssetId)
VALUES
('11111111-1111-1111-1111-111111111111', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'),
('22222222-2222-2222-2222-222222222222', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb'),
('33333333-3333-3333-3333-333333333333', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa');
GO
