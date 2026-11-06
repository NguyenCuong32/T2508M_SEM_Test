-- =============================================================
-- DATABASE CREATION SCRIPT: SUNRISEHOTEL
-- Project: Generative AI for .NET Developers with Google AI (SET02)
-- Author: GANA_LeTrungKien
-- =============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SUNRISEHOTEL')
BEGIN
    CREATE DATABASE SUNRISEHOTEL;
END
GO

USE SUNRISEHOTEL;
GO

-- 1. Create Room Table
IF OBJECT_ID(N'dbo.Booking', N'U') IS NOT NULL DROP TABLE dbo.Booking;
IF OBJECT_ID(N'dbo.Room', N'U') IS NOT NULL DROP TABLE dbo.Room;
GO

CREATE TABLE dbo.Room (
    RoomId INT NOT NULL,
    RoomNumber NVARCHAR(10) NOT NULL,
    RoomType NVARCHAR(32) NOT NULL, -- Single / Double / Suite
    PricePerNight DECIMAL(18, 2) NOT NULL,
    MaxGuests INT NOT NULL,
    CONSTRAINT PK_Room PRIMARY KEY CLUSTERED (RoomId ASC)
);
GO

-- 2. Create Booking Table
CREATE TABLE dbo.Booking (
    BookingId INT IDENTITY(1,1) NOT NULL,
    RoomId INT NOT NULL,
    GuestName NVARCHAR(128) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    CheckIn DATE NOT NULL,
    CheckOut DATE NOT NULL,
    TotalPrice DECIMAL(18, 2) NOT NULL,
    Status NVARCHAR(16) NOT NULL CONSTRAINT DF_Booking_Status DEFAULT (N'Confirmed'), -- Confirmed / Cancelled
    CONSTRAINT PK_Booking PRIMARY KEY CLUSTERED (BookingId ASC),
    CONSTRAINT FK_Booking_Room FOREIGN KEY (RoomId) REFERENCES dbo.Room(RoomId)
);
GO

-- 3. Initial Data for Room Table
INSERT INTO dbo.Room (RoomId, RoomNumber, RoomType, PricePerNight, MaxGuests)
VALUES
    (1, N'101', N'Single', 600000.00, 1),
    (2, N'102', N'Single', 600000.00, 1),
    (3, N'201', N'Double', 950000.00, 2),
    (4, N'202', N'Double', 950000.00, 2),
    (5, N'301', N'Suite', 2200000.00, 4);
GO

-- 4. Initial Sample Bookings (matching Question 4 report)
INSERT INTO dbo.Booking (RoomId, GuestName, Phone, CheckIn, CheckOut, TotalPrice, Status)
VALUES
    (3, N'Nguyễn Văn An', N'0901234567', '2026-11-20', '2026-11-22', 1900000.00, N'Confirmed'),
    (1, N'Trần Thị Bình', N'0912345678', '2026-11-21', '2026-11-24', 1800000.00, N'Confirmed'),
    (5, N'Lê Hoàng Nam', N'0987654321', '2026-11-25', '2026-11-26', 2200000.00, N'Confirmed');
GO

-- 5. Stored Procedure: GetBookingReport (Requirement 4)
IF OBJECT_ID(N'dbo.sp_GetBookingReport', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_GetBookingReport;
GO

CREATE PROCEDURE dbo.sp_GetBookingReport
    @FromDate DATE = NULL,
    @ToDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        ROW_NUMBER() OVER(ORDER BY b.CheckIn ASC, b.BookingId ASC) AS [No],
        r.RoomNumber AS [Room number],
        r.RoomType AS [Room type],
        b.GuestName AS [Guest name],
        CONVERT(VARCHAR(10), b.CheckIn, 103) AS [Check-in],
        CONVERT(VARCHAR(10), b.CheckOut, 103) AS [Check-out],
        DATEDIFF(DAY, b.CheckIn, b.CheckOut) AS [Nights],
        b.TotalPrice AS [Total price]
    FROM dbo.Booking b
    INNER JOIN dbo.Room r ON b.RoomId = r.RoomId
    WHERE b.Status = N'Confirmed'
      AND (@FromDate IS NULL OR b.CheckIn >= @FromDate)
      AND (@ToDate IS NULL OR b.CheckIn <= @ToDate)
    ORDER BY b.CheckIn ASC, b.BookingId ASC;
END
GO

-- 6. Stored Procedure: CheckAvailableRooms
IF OBJECT_ID(N'dbo.sp_CheckAvailableRooms', N'P') IS NOT NULL DROP PROCEDURE dbo.sp_CheckAvailableRooms;
GO

CREATE PROCEDURE dbo.sp_CheckAvailableRooms
    @RoomType NVARCHAR(32) = NULL,
    @CheckIn DATE,
    @CheckOut DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        r.RoomId,
        r.RoomNumber,
        r.RoomType,
        r.PricePerNight,
        r.MaxGuests
    FROM dbo.Room r
    WHERE (@RoomType IS NULL OR r.RoomType = @RoomType)
      AND r.RoomId NOT IN (
          SELECT b.RoomId 
          FROM dbo.Booking b
          WHERE b.Status = N'Confirmed'
            AND b.CheckIn < @CheckOut
            AND @CheckIn < b.CheckOut
      )
    ORDER BY r.RoomNumber;
END
GO

-- Verification Queries
SELECT * FROM dbo.Room;
SELECT * FROM dbo.Booking;
EXEC dbo.sp_GetBookingReport;
GO
