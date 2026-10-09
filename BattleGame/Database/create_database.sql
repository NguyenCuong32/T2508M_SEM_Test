IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013740_InitialCreate'
)
BEGIN
    CREATE TABLE [Asset] (
        [AssetId] uniqueidentifier NOT NULL,
        [AssetName] nvarchar(64) NOT NULL,
        [LevelRequire] int NOT NULL,
        CONSTRAINT [PK_Asset] PRIMARY KEY ([AssetId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013740_InitialCreate'
)
BEGIN
    CREATE TABLE [Player] (
        [PlayerId] uniqueidentifier NOT NULL,
        [PlayerName] nvarchar(64) NOT NULL,
        [FullName] nvarchar(128) NOT NULL,
        [Age] nvarchar(10) NOT NULL,
        [Level] int NOT NULL,
        [Email] nvarchar(64) NOT NULL,
        CONSTRAINT [PK_Player] PRIMARY KEY ([PlayerId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013740_InitialCreate'
)
BEGIN
    CREATE TABLE [PlayerAsset] (
        [PlayerId] uniqueidentifier NOT NULL,
        [AssetId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_PlayerAsset] PRIMARY KEY ([PlayerId], [AssetId]),
        CONSTRAINT [FK_PlayerAsset_Asset_AssetId] FOREIGN KEY ([AssetId]) REFERENCES [Asset] ([AssetId]) ON DELETE CASCADE,
        CONSTRAINT [FK_PlayerAsset_Player_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Player] ([PlayerId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013740_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'AssetId', N'AssetName', N'LevelRequire') AND [object_id] = OBJECT_ID(N'[Asset]'))
        SET IDENTITY_INSERT [Asset] ON;
    EXEC(N'INSERT INTO [Asset] ([AssetId], [AssetName], [LevelRequire])
    VALUES (''aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'', N''Hero 1'', 1),
    (''bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb'', N''Hero 2'', 3)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'AssetId', N'AssetName', N'LevelRequire') AND [object_id] = OBJECT_ID(N'[Asset]'))
        SET IDENTITY_INSERT [Asset] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013740_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PlayerId', N'Age', N'Email', N'FullName', N'Level', N'PlayerName') AND [object_id] = OBJECT_ID(N'[Player]'))
        SET IDENTITY_INSERT [Player] ON;
    EXEC(N'INSERT INTO [Player] ([PlayerId], [Age], [Email], [FullName], [Level], [PlayerName])
    VALUES (''11111111-1111-1111-1111-111111111111'', N''20'', N''p1@game.com'', N''Nguyen Van A'', 10, N''Player 1''),
    (''22222222-2222-2222-2222-222222222222'', N''19'', N''p2@game.com'', N''Tran Thi B'', 3, N''Player 2''),
    (''33333333-3333-3333-3333-333333333333'', N''23'', N''p3@game.com'', N''Le Van C'', 10, N''Player 3'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PlayerId', N'Age', N'Email', N'FullName', N'Level', N'PlayerName') AND [object_id] = OBJECT_ID(N'[Player]'))
        SET IDENTITY_INSERT [Player] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013740_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'AssetId', N'PlayerId') AND [object_id] = OBJECT_ID(N'[PlayerAsset]'))
        SET IDENTITY_INSERT [PlayerAsset] ON;
    EXEC(N'INSERT INTO [PlayerAsset] ([AssetId], [PlayerId])
    VALUES (''aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'', ''11111111-1111-1111-1111-111111111111''),
    (''bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb'', ''22222222-2222-2222-2222-222222222222''),
    (''aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'', ''33333333-3333-3333-3333-333333333333'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'AssetId', N'PlayerId') AND [object_id] = OBJECT_ID(N'[PlayerAsset]'))
        SET IDENTITY_INSERT [PlayerAsset] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013740_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Player_Email] ON [Player] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013740_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Player_PlayerName] ON [Player] ([PlayerName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013740_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PlayerAsset_AssetId] ON [PlayerAsset] ([AssetId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261009013740_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261009013740_InitialCreate', N'10.0.12');
END;

COMMIT;
GO

