-- ============================================================
-- BATTLEGAME - Sample seed data
-- ============================================================

USE BATTLEGAME;
GO

-- Seed Assets
INSERT INTO Asset (AssetId, AssetName, LevelRequire) VALUES
    ('A1000000-0000-0000-0000-000000000001', 'Hero 1', 1),
    ('A1000000-0000-0000-0000-000000000002', 'Hero 2', 2),
    ('A1000000-0000-0000-0000-000000000003', 'Sword',  5);
GO

-- Seed Players
INSERT INTO Player (PlayerId, PlayerName, FullName, Age, [Level], Email) VALUES
    ('B1000000-0000-0000-0000-000000000001', 'Player 1', 'Nguyen Van A', '20', 10, 'player1@game.com'),
    ('B1000000-0000-0000-0000-000000000002', 'Player 2', 'Tran Thi B',   '19',  3, 'player2@game.com'),
    ('B1000000-0000-0000-0000-000000000003', 'Player 3', 'Le Van C',     '23', 10, 'player3@game.com');
GO

-- Seed PlayerAsset
INSERT INTO PlayerAsset (PlayerId, AssetId) VALUES
    ('B1000000-0000-0000-0000-000000000001', 'A1000000-0000-0000-0000-000000000001'),
    ('B1000000-0000-0000-0000-000000000002', 'A1000000-0000-0000-0000-000000000002'),
    ('B1000000-0000-0000-0000-000000000003', 'A1000000-0000-0000-0000-000000000001');
GO
