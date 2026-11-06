-- Database: SUNRISEHOTEL
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SUNRISEHOTEL')
BEGIN
    CREATE DATABASE [SUNRISEHOTEL];
END
GO

USE [SUNRISEHOTEL];
GO

-- Drop tables if exists
IF OBJECT_ID(N'dbo.Booking', N'U') IS NOT NULL
    DROP TABLE dbo.Booking;
GO

IF OBJECT_ID(N'dbo.Room', N'U') IS NOT NULL
    DROP TABLE dbo.Room;
GO

-- 1. Create Room table
CREATE TABLE dbo.Room (
    RoomId INT NOT NULL PRIMARY KEY,
    RoomNumber NVARCHAR(10) NOT NULL,
    RoomType NVARCHAR(32) NOT NULL, -- Single / Double / Suite
    PricePerNight DECIMAL(18, 2) NOT NULL,
    MaxGuests INT NOT NULL
);
GO

-- 2. Create Booking table
CREATE TABLE dbo.Booking (
    BookingId INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    RoomId INT NOT NULL,
    GuestName NVARCHAR(128) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    CheckIn DATE NOT NULL,
    CheckOut DATE NOT NULL,
    TotalPrice DECIMAL(18, 2) NOT NULL,
    Status NVARCHAR(16) NOT NULL, -- Confirmed / Cancelled
    CONSTRAINT FK_Booking_Room FOREIGN KEY (RoomId) REFERENCES dbo.Room(RoomId)
);
GO

-- 3. Initial data for Room table
INSERT INTO dbo.Room (RoomId, RoomNumber, RoomType, PricePerNight, MaxGuests)
VALUES
    (1, N'101', N'Single', 600000.00, 1),
    (2, N'102', N'Single', 600000.00, 1),
    (3, N'201', N'Double', 950000.00, 2),
    (4, N'202', N'Double', 950000.00, 2),
    (5, N'301', N'Suite', 2200000.00, 4);
GO

-- 4. Initial sample bookings matching exam paper report
INSERT INTO dbo.Booking (RoomId, GuestName, Phone, CheckIn, CheckOut, TotalPrice, Status)
VALUES
    (3, N'Nguyễn Văn An', N'0901234567', '2026-11-20', '2026-11-22', 1900000.00, N'Confirmed'),
    (1, N'Trần Thị Bình', N'0912345678', '2026-11-21', '2026-11-24', 1800000.00, N'Confirmed'),
    (5, N'Lê Hoàng Nam', N'0987654321', '2026-11-25', '2026-11-26', 2200000.00, N'Confirmed');
GO

-- Verify Data
SELECT * FROM dbo.Room;
SELECT * FROM dbo.Booking;
GO
