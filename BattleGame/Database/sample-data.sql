USE [BATTLEGAME];
SET XACT_ABORT ON;
BEGIN TRANSACTION;

INSERT INTO [Player] ([PlayerId], [PlayerName], [FullName], [Age], [Level], [Email])
SELECT source.[PlayerId], source.[PlayerName], source.[FullName], source.[Age], source.[Level], source.[Email]
FROM (VALUES
    (CAST('11111111-1111-4111-8111-111111111111' AS uniqueidentifier), N'Player 1', N'Nguyen Van A', N'20', 10, N'player1@example.com'),
    (CAST('22222222-2222-4222-8222-222222222222' AS uniqueidentifier), N'Player 2', N'Tran Van B', N'19', 3, N'player2@example.com'),
    (CAST('33333333-3333-4333-8333-333333333333' AS uniqueidentifier), N'Player 3', N'Le Van C', N'23', 10, N'player3@example.com')
) AS source ([PlayerId], [PlayerName], [FullName], [Age], [Level], [Email])
WHERE NOT EXISTS (SELECT 1 FROM [Player] WHERE [Email] = source.[Email]);

INSERT INTO [Asset] ([AssetId], [AssetName], [LevelRequire])
SELECT source.[AssetId], source.[AssetName], source.[LevelRequire]
FROM (VALUES
    (CAST('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa' AS uniqueidentifier), N'Hero 1', 1),
    (CAST('bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb' AS uniqueidentifier), N'Hero 2', 3)
) AS source ([AssetId], [AssetName], [LevelRequire])
WHERE NOT EXISTS (SELECT 1 FROM [Asset] WHERE [AssetId] = source.[AssetId]);

INSERT INTO [PlayerAsset] ([PlayerId], [AssetId])
SELECT player.[PlayerId], assignments.[AssetId]
FROM (VALUES
    (N'player1@example.com', CAST('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa' AS uniqueidentifier)),
    (N'player2@example.com', CAST('bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb' AS uniqueidentifier)),
    (N'player3@example.com', CAST('aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa' AS uniqueidentifier))
) AS assignments ([Email], [AssetId])
INNER JOIN [Player] player ON player.[Email] = assignments.[Email]
WHERE NOT EXISTS (
    SELECT 1 FROM [PlayerAsset]
    WHERE [PlayerId] = player.[PlayerId] AND [AssetId] = assignments.[AssetId]
);

COMMIT TRANSACTION;
