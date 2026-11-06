-- SUNRISEHOTEL database script (generated from EF Core Code First migrations)
-- Usage: sqlcmd -S localhost -U sa -P <password> -C -i SUNRISEHOTEL.sql
-- (Optional: the API also creates/migrates the database automatically on startup.)

IF DB_ID(N'SUNRISEHOTEL') IS NULL
    CREATE DATABASE [SUNRISEHOTEL];
GO

USE [SUNRISEHOTEL];
GO

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
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106020128_InitialCreate'
)
BEGIN
    CREATE TABLE [Room] (
        [RoomId] int NOT NULL,
        [RoomNumber] nvarchar(10) NOT NULL,
        [RoomType] nvarchar(32) NOT NULL,
        [PricePerNight] decimal(18,2) NOT NULL,
        [MaxGuests] int NOT NULL,
        CONSTRAINT [PK_Room] PRIMARY KEY ([RoomId]),
        CONSTRAINT [CK_Room_MaxGuests] CHECK ([MaxGuests] > 0),
        CONSTRAINT [CK_Room_PricePerNight] CHECK ([PricePerNight] >= 0),
        CONSTRAINT [CK_Room_RoomType] CHECK ([RoomType] IN ('Single','Double','Suite'))
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106020128_InitialCreate'
)
BEGIN
    CREATE TABLE [Booking] (
        [BookingId] int NOT NULL IDENTITY,
        [RoomId] int NOT NULL,
        [GuestName] nvarchar(128) NOT NULL,
        [Phone] nvarchar(20) NOT NULL,
        [CheckIn] date NOT NULL,
        [CheckOut] date NOT NULL,
        [TotalPrice] decimal(18,2) NOT NULL,
        [Status] nvarchar(16) NOT NULL,
        CONSTRAINT [PK_Booking] PRIMARY KEY ([BookingId]),
        CONSTRAINT [CK_Booking_Dates] CHECK ([CheckOut] > [CheckIn]),
        CONSTRAINT [CK_Booking_Status] CHECK ([Status] IN ('Confirmed','Cancelled')),
        CONSTRAINT [CK_Booking_TotalPrice] CHECK ([TotalPrice] >= 0),
        CONSTRAINT [FK_Booking_Room_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Room] ([RoomId]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106020128_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoomId', N'MaxGuests', N'PricePerNight', N'RoomNumber', N'RoomType') AND [object_id] = OBJECT_ID(N'[Room]'))
        SET IDENTITY_INSERT [Room] ON;
    EXEC(N'INSERT INTO [Room] ([RoomId], [MaxGuests], [PricePerNight], [RoomNumber], [RoomType])
    VALUES (1, 1, 600000.0, N''101'', N''Single''),
    (2, 1, 600000.0, N''102'', N''Single''),
    (3, 2, 950000.0, N''201'', N''Double''),
    (4, 2, 950000.0, N''202'', N''Double''),
    (5, 4, 2200000.0, N''301'', N''Suite'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoomId', N'MaxGuests', N'PricePerNight', N'RoomNumber', N'RoomType') AND [object_id] = OBJECT_ID(N'[Room]'))
        SET IDENTITY_INSERT [Room] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106020128_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Booking_Phone] ON [Booking] ([Phone]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106020128_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Booking_RoomId_CheckIn_CheckOut] ON [Booking] ([RoomId], [CheckIn], [CheckOut]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106020128_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Room_RoomNumber] ON [Room] ([RoomNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261106020128_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261106020128_InitialCreate', N'8.0.22');
END;
GO

COMMIT;
GO

