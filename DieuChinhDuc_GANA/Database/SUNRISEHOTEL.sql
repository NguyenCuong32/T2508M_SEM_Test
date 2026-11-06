CREATE DATABASE IF NOT EXISTS SUNRISEHOTEL
    CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE SUNRISEHOTEL;

CREATE TABLE IF NOT EXISTS Room (
    RoomId INT PRIMARY KEY,
    RoomNumber VARCHAR(10) NOT NULL UNIQUE,
    RoomType ENUM('Single', 'Double', 'Suite') NOT NULL,
    PricePerNight DECIMAL(18,2) NOT NULL,
    MaxGuests INT NOT NULL,
    INDEX IX_Room_Type (RoomType, RoomId)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS Booking (
    BookingId INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    RoomId INT NOT NULL,
    GuestName VARCHAR(128) NOT NULL,
    Phone VARCHAR(20) NOT NULL,
    CheckIn DATE NOT NULL,
    CheckOut DATE NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    Status ENUM('Confirmed', 'Cancelled') NOT NULL DEFAULT 'Confirmed',
    CONSTRAINT FK_Booking_Room FOREIGN KEY (RoomId) REFERENCES Room(RoomId),
    INDEX IX_Booking_Availability (RoomId, Status, CheckIn, CheckOut),
    INDEX IX_Booking_Phone (Phone),
    INDEX IX_Booking_Report (Status, CheckIn, BookingId)
) ENGINE=InnoDB;

INSERT INTO Room (RoomId, RoomNumber, RoomType, PricePerNight, MaxGuests)
VALUES (1, '101', 'Single', 600000, 1),
       (2, '102', 'Single', 600000, 1),
       (3, '201', 'Double', 950000, 2),
       (4, '202', 'Double', 950000, 2),
       (5, '301', 'Suite', 2200000, 4)
ON DUPLICATE KEY UPDATE RoomId = RoomId;
