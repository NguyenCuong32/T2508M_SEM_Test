-- Script khởi tạo Database cho đề bài Sunrise Hotel (SET02)
-- Sử dụng cho MySQL / MariaDB

CREATE DATABASE IF NOT EXISTS `SUNRISEHOTEL` 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE `SUNRISEHOTEL`;

-- 1. Bảng Room
CREATE TABLE IF NOT EXISTS `Room` (
    `RoomId` INT NOT NULL AUTO_INCREMENT,
    `RoomNumber` VARCHAR(10) NOT NULL,
    `RoomType` VARCHAR(32) NOT NULL,
    `PricePerNight` DECIMAL(18,2) NOT NULL,
    `MaxGuests` INT NOT NULL,
    PRIMARY KEY (`RoomId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 2. Bảng Booking
CREATE TABLE IF NOT EXISTS `Booking` (
    `BookingId` INT NOT NULL AUTO_INCREMENT,
    `RoomId` INT NOT NULL,
    `GuestName` VARCHAR(128) NOT NULL,
    `Phone` VARCHAR(20) NOT NULL,
    `CheckIn` DATE NOT NULL,
    `CheckOut` DATE NOT NULL,
    `TotalPrice` DECIMAL(18,2) NOT NULL,
    `Status` VARCHAR(16) NOT NULL,
    PRIMARY KEY (`BookingId`),
    CONSTRAINT `FK_Booking_Room_RoomId` FOREIGN KEY (`RoomId`) 
        REFERENCES `Room` (`RoomId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 3. Nạp dữ liệu ban đầu cho bảng Room (Seed Data)
INSERT INTO `Room` (`RoomId`, `RoomNumber`, `RoomType`, `PricePerNight`, `MaxGuests`) 
VALUES
(1, '101', 'Single', 600000.00, 1),
(2, '102', 'Single', 600000.00, 1),
(3, '201', 'Double', 950000.00, 2),
(4, '202', 'Double', 950000.00, 2),
(5, '301', 'Suite', 2200000.00, 4)
ON DUPLICATE KEY UPDATE 
    `RoomNumber` = VALUES(`RoomNumber`),
    `RoomType` = VALUES(`RoomType`),
    `PricePerNight` = VALUES(`PricePerNight`),
    `MaxGuests` = VALUES(`MaxGuests`);
