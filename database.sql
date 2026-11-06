-- ============================================================================
-- DỰ ÁN: SUNRISE HOTEL - GENERATIVE AI FOR .NET DEVELOPERS (SET02)
-- SCRIPT TẠO CƠ SỞ DỮ LIỆU, BẢNG VÀ DỮ LIỆU KHỞI TẠO (SEED DATA)
-- Tên cơ sở dữ liệu: SUNRISEHOTEL
-- ============================================================================

USE master;
GO

-- 1. Tạo Database nếu chưa tồn tại
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SUNRISEHOTEL')
BEGIN
    CREATE DATABASE [SUNRISEHOTEL];
    PRINT N'Cơ sở dữ liệu SUNRISEHOTEL đã được tạo thành công.';
END
GO

USE [SUNRISEHOTEL];
GO

-- 2. Xóa bảng cũ nếu đã tồn tại để tạo mới sạch sẽ
IF OBJECT_ID(N'[dbo].[Booking]', N'U') IS NOT NULL
    DROP TABLE [dbo].[Booking];
GO

IF OBJECT_ID(N'[dbo].[Room]', N'U') IS NOT NULL
    DROP TABLE [dbo].[Room];
GO

-- 3. Tạo bảng Room
CREATE TABLE [dbo].[Room] (
    [RoomId]        INT             NOT NULL,
    [RoomNumber]    NVARCHAR(10)    NOT NULL,
    [RoomType]      NVARCHAR(32)    NOT NULL, -- Single / Double / Suite
    [PricePerNight] DECIMAL(18, 2)  NOT NULL, -- Giá mỗi đêm (VND)
    [MaxGuests]     INT             NOT NULL, -- Số khách tối đa
    CONSTRAINT [PK_Room] PRIMARY KEY CLUSTERED ([RoomId] ASC)
);
GO

-- 4. Tạo bảng Booking
CREATE TABLE [dbo].[Booking] (
    [BookingId]   INT             IDENTITY(1, 1) NOT NULL,
    [RoomId]      INT             NOT NULL,
    [GuestName]   NVARCHAR(128)   NOT NULL,
    [Phone]       NVARCHAR(20)    NOT NULL,
    [CheckIn]     DATE            NOT NULL,
    [CheckOut]    DATE            NOT NULL,
    [TotalPrice]  DECIMAL(18, 2)  NOT NULL, -- (CheckOut - CheckIn).Days * PricePerNight
    [Status]      NVARCHAR(16)    NOT NULL, -- Confirmed hoặc Cancelled
    CONSTRAINT [PK_Booking] PRIMARY KEY CLUSTERED ([BookingId] ASC),
    CONSTRAINT [FK_Booking_Room] FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Room] ([RoomId])
);
GO

-- 5. Seed Data cho bảng Room (5 phòng)
INSERT INTO [dbo].[Room] ([RoomId], [RoomNumber], [RoomType], [PricePerNight], [MaxGuests])
VALUES 
    (1, N'101', N'Single', 600000.00, 1),
    (2, N'102', N'Single', 600000.00, 1),
    (3, N'201', N'Double', 950000.00, 2),
    (4, N'202', N'Double', 950000.00, 2),
    (5, N'301', N'Suite',  2200000.00, 4);
GO

-- 6. Seed Data cho bảng Booking (3 đơn mẫu trạng thái Confirmed)
SET IDENTITY_INSERT [dbo].[Booking] ON;
GO

INSERT INTO [dbo].[Booking] ([BookingId], [RoomId], [GuestName], [Phone], [CheckIn], [CheckOut], [TotalPrice], [Status])
VALUES 
    (1, 3, N'Nguyễn Văn An',  N'0901234567', '2026-11-20', '2026-11-22', 1900000.00, N'Confirmed'),
    (2, 1, N'Trần Thị Bình',  N'0902345678', '2026-11-21', '2026-11-24', 1800000.00, N'Confirmed'),
    (3, 5, N'Lê Hoàng Nam',   N'0903456789', '2026-11-25', '2026-11-26', 2200000.00, N'Confirmed');
GO

SET IDENTITY_INSERT [dbo].[Booking] OFF;
GO

-- Kiểm tra dữ liệu vừa tạo
SELECT * FROM [dbo].[Room];
SELECT * FROM [dbo].[Booking];
GO

