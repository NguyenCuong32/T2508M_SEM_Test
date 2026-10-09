-- ============================================================
-- BATTLEGAME Database - Database First Script
-- ============================================================

CREATE DATABASE BATTLEGAME;
GO

USE BATTLEGAME;
GO

-- ============================================================
-- Table: Asset
-- ============================================================
CREATE TABLE Asset (
    AssetId       UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWID() PRIMARY KEY,
    AssetName     NVARCHAR(64)      NOT NULL,
    LevelRequire  INT               NOT NULL DEFAULT 1
);
GO

-- ============================================================
-- Table: Player
-- ============================================================
CREATE TABLE Player (
    PlayerId    UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWID() PRIMARY KEY,
    PlayerName  NVARCHAR(64)      NOT NULL,
    FullName    NVARCHAR(128)     NOT NULL,
    Age         NVARCHAR(10)      NOT NULL,
    [Level]     INT               NOT NULL DEFAULT 1,
    Email       NVARCHAR(64)      NOT NULL
);
GO

-- ============================================================
-- Table: PlayerAsset  (junction table)
-- ============================================================
CREATE TABLE PlayerAsset (
    PlayerId  UNIQUEIDENTIFIER  NOT NULL,
    AssetId   UNIQUEIDENTIFIER  NOT NULL,
    CONSTRAINT PK_PlayerAsset PRIMARY KEY (PlayerId, AssetId),
    CONSTRAINT FK_PlayerAsset_Player FOREIGN KEY (PlayerId) REFERENCES Player(PlayerId),
    CONSTRAINT FK_PlayerAsset_Asset  FOREIGN KEY (AssetId)  REFERENCES Asset(AssetId)
);
GO
