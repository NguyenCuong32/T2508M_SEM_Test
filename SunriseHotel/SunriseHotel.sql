If DB_ID('SunriseHotel') is Null
Begin
create database SunriseHotel
End
GO

USE SunriseHotel
GO

create table Room
(
	roomID int Primary key,
	roomNumber nvarchar(10) not null unique,
	roomType nvarchar(32) not null,
	PricePerNight decimal(18,2) not null,
	MaxGuests int not null,

	constraint CK_Room_RoomType
	Check (RoomType IN('Single', 'Double', 'Suite')),

	constraint CK_Room_Price
	check (PricePerNight > 0),

	constraint CK_Room_MaxGuests
	check (MaxGuests > 0)
);
GO

Create table Booking
(
	BookingID int Identity(1,1) Primary key,
	RoomID int not null,
	GuestName nvarchar(128) not null,
	CheckIn Date not null,
	CheckOut Date not null,
	TotalPrice Decimal(18,2) not null,
	Status Nvarchar(16) not null Default 'Confirmed',

	Constraint FK_Booking_Room
	Foreign Key (RoomID)
	References Room(RoomID),

	Constraint CK_Booking_Dates
	Check (CheckOut > CheckIn),

	Constraint CK_Booking_TotalPrice
	Check (TotalPrice >=0),

	Constraint CK_Booking_Status
	Check (Status IN('Confirmed','Cancelled'))
);
GO

Insert into Room(roomID, roomNumber, roomType, PricePerNight, MaxGuests)
Values
	(1, '101', 'Single', 600000, 1),
	(2, '102', 'Single', 600000, 1),
	(3, '201', 'Double', 950000, 2),
	(4, '202', 'Double', 950000, 2),
	(5, '301', 'Suite', 2200000, 4);
GO

Use SunriseHotel;
GO

Select * From Room;
Select * From Booking;