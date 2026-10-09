-- ========================================================
-- Database Script for BATTLEGAME
-- Developing Microsoft Azure Solutions - SET01
-- ========================================================

CREATE DATABASE IF NOT EXISTS `BATTLEGAME` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `BATTLEGAME`;

-- Drop tables in reverse order of foreign keys if they exist
DROP TABLE IF EXISTS `PlayerAsset`;
DROP TABLE IF EXISTS `Asset`;
DROP TABLE IF EXISTS `Player`;

-- 1. Table Player
CREATE TABLE `Player` (
    `PlayerId` VARCHAR(36) NOT NULL,
    `PlayerName` VARCHAR(64) NOT NULL,
    `FullName` VARCHAR(128) NOT NULL,
    `Age` VARCHAR(10) NOT NULL,
    `Level` INT NOT NULL,
    `Email` VARCHAR(64) NOT NULL,
    PRIMARY KEY (`PlayerId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 2. Table Asset
CREATE TABLE `Asset` (
    `AssetId` VARCHAR(36) NOT NULL,
    `AssetName` VARCHAR(64) NOT NULL,
    `LevelRequire` INT NOT NULL,
    PRIMARY KEY (`AssetId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 3. Table PlayerAsset (Junction table)
CREATE TABLE `PlayerAsset` (
    `PlayerId` VARCHAR(36) NOT NULL,
    `AssetId` VARCHAR(36) NOT NULL,
    PRIMARY KEY (`PlayerId`, `AssetId`),
    CONSTRAINT `FK_PlayerAsset_Player` FOREIGN KEY (`PlayerId`) REFERENCES `Player` (`PlayerId`) ON DELETE CASCADE,
    CONSTRAINT `FK_PlayerAsset_Asset` FOREIGN KEY (`AssetId`) REFERENCES `Asset` (`AssetId`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ========================================================
-- Sample Seed Data matching Requirement 3 specification:
-- No | Player name | Level | Age | Asset name
-- 1  | Player 1    | 10    | 20  | Hero 1
-- 2  | Player 2    | 3     | 19  | Hero 2
-- 3  | Player 3    | 10    | 23  | Hero 1
-- ========================================================

-- Insert Assets
INSERT INTO `Asset` (`AssetId`, `AssetName`, `LevelRequire`) VALUES
('a0000000-0000-0000-0000-000000000001', 'Hero 1', 1),
('a0000000-0000-0000-0000-000000000002', 'Hero 2', 3),
('a0000000-0000-0000-0000-000000000003', 'Dragon Sword', 5),
('a0000000-0000-0000-0000-000000000004', 'Shadow Armor', 10);

-- Insert Players
INSERT INTO `Player` (`PlayerId`, `PlayerName`, `FullName`, `Age`, `Level`, `Email`) VALUES
('p0000000-0000-0000-0000-000000000001', 'Player 1', 'Nguyen Van A', '20', 10, 'player1@battlegame.com'),
('p0000000-0000-0000-0000-000000000002', 'Player 2', 'Tran Thi B', '19', 3, 'player2@battlegame.com'),
('p0000000-0000-0000-0000-000000000003', 'Player 3', 'Le Van C', '23', 10, 'player3@battlegame.com');

-- Insert PlayerAsset associations
INSERT INTO `PlayerAsset` (`PlayerId`, `AssetId`) VALUES
('p0000000-0000-0000-0000-000000000001', 'a0000000-0000-0000-0000-000000000001'), -- Player 1 owns Hero 1
('p0000000-0000-0000-0000-000000000002', 'a0000000-0000-0000-0000-000000000002'), -- Player 2 owns Hero 2
('p0000000-0000-0000-0000-000000000003', 'a0000000-0000-0000-0000-000000000001'); -- Player 3 owns Hero 1

-- Verification query
SELECT 
    ROW_NUMBER() OVER (ORDER BY p.PlayerName ASC) AS `No`,
    p.PlayerName AS `Player name`,
    p.Level AS `Level`,
    p.Age AS `Age`,
    a.AssetName AS `Asset name`
FROM `PlayerAsset` pa
JOIN `Player` p ON pa.PlayerId = p.PlayerId
JOIN `Asset` a ON pa.AssetId = a.AssetId;
