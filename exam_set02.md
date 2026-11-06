# Generative AI for .NET Developers with Google AI – Exam Paper (SET02)

**Duration:** 60 minutes | **Marks:** 20

## Question

**Sunrise Hotel** wants to build an AI booking assistant using **Google Gemini**. Guests can ask questions about the hotel, send booking requests in natural language and chat with an assistant that can check room availability and make bookings by calling the hotel's C# functions (**Function Calling**).

**Database name:** `SUNRISEHOTEL`

### Room

| Column Name | Data Type | Description |
|---|---|---|
| RoomId | int (PK) | Room identifier |
| RoomNumber | nvarchar(10) | Room number |
| RoomType | nvarchar(32) | Single / Double / Suite |
| PricePerNight | decimal(18,2) | Price per night (VND) |
| MaxGuests | int | Maximum number of guests |

### Booking

| Column Name | Data Type | Description |
|---|---|---|
| BookingId | int (PK, identity) | Booking identifier |
| RoomId | int (FK) | Booked room |
| GuestName | nvarchar(128) | Guest full name |
| Phone | nvarchar(20) | Guest phone number |
| CheckIn | date | Check-in date |
| CheckOut | date | Check-out date |
| TotalPrice | decimal(18,2) | Number of nights x PricePerNight |
| Status | nvarchar(16) | Confirmed / Cancelled |

### Initial data for the Room table

| RoomId | Room number | Room type | Price per night | Max guests |
|---|---|---|---|---|
| 1 | 101 | Single | 600.000 | 1 |
| 2 | 102 | Single | 600.000 | 1 |
| 3 | 201 | Double | 950.000 | 2 |
| 4 | 202 | Double | 950.000 | 2 |
| 5 | 301 | Suite | 2.200.000 | 4 |

**Hotel information:** check-in from 14:00, check-out before 12:00, free breakfast 6:30 - 9:30, free airport pick-up for Suite rooms, swimming pool on the 5th floor, address 25 Tran Phu, Nha Trang.

## Requirements

### 1. API `askhotel`

When a guest asks about the hotel, create API: **askhotel** (`POST /api/ai/ask`, body: `question`). Use a **system instruction** containing the hotel information above so that Gemini acts as a polite receptionist, only answers questions about Sunrise Hotel and refuses unrelated questions.

### 2. API `parsebooking`

When a guest sends a booking request in natural language, create API: **parsebooking** (`POST /api/bookings/parse`, body: `text`). Gemini must return **JSON only** (use response schema / JSON mode) with the format below. Deserialize the JSON into a C# object, validate it (checkOut > checkIn, roomType is valid), find an available room of that type, calculate TotalPrice and save a new booking with status Confirmed.

**Input:**

```
Tôi là Nguyễn Văn An, sđt 0901234567, muốn đặt 1 phòng đôi từ 20/11/2026 đến 22/11/2026 cho 2 người
```

**Output:**

```json
{
  "guestName": "Nguyễn Văn An",
  "phone": "0901234567",
  "roomType": "Double",
  "checkIn": "2026-11-20",
  "checkOut": "2026-11-22",
  "guests": 2
}
```

### 3. API `bookingassistant`

Create API: **bookingassistant** (`POST /api/assistant/chat`, body: `message`) using **Function Calling**. Declare the following functions for Gemini and execute them in C# when Gemini requests, then send the result back to Gemini to get the final answer:

| Function | Parameters | Description |
|---|---|---|
| CheckAvailability | roomType, checkIn, checkOut | Return available rooms and price |
| GetBookingsByPhone | phone | Return the bookings of a guest |
| CancelBooking | bookingId | Change booking status to Cancelled |

**Example:** "Còn phòng Suite nào trống từ 24/12 đến 26/12 không?" or "Hủy giúp tôi booking số 3".

### 4. API `getbookingreport`

When admin needs a booking report, create API: **getbookingreport** (`GET /api/reports/bookings?from=...&to=...`) that returns confirmed bookings as the table below:

| No | Room number | Room type | Guest name | Check-in | Check-out | Nights | Total price |
|---|---|---|---|---|---|---|---|
| 1 | 201 | Double | Nguyễn Văn An | 20/11/2026 | 22/11/2026 | 2 | 1.900.000 |
| 2 | 101 | Single | Trần Thị Bình | 21/11/2026 | 24/11/2026 | 3 | 1.800.000 |
| 3 | 301 | Suite | Lê Hoàng Nam | 25/11/2026 | 26/11/2026 | 1 | 2.200.000 |

### 5. Website

Using Razor Pages, MVC, Blazor, ReactJS or any front end to make a website that shows the table in requirement number (4), with a filter by date range.

### 6. README.md

Create a README.md document that lists the prompts and function declarations used in requirements 1, 2, 3 and explains how the Gemini API key is protected.

## Marking Schema

| Question | Mark |
|---|---|
| Create database and configure Gemini API key (User Secrets / environment variable) | 2 |
| Question 1 – Make API askhotel | 3 |
| Question 2 – Make API parsebooking (JSON structured output) | 4 |
| Question 3 – Make API bookingassistant (Function Calling) | 4 |
| Question 4 – Make API getbookingreport | 3 |
| Question 5 – Make a website | 2 |
| Question 6 – Make document README.md | 1 |
| Bonus – Good coding convention, error handling (invalid JSON, quota 429, ...) | 1 |
| **Total** | **20** |

## Note

1. Use .NET 8 (or later) with ASP.NET Core Web API (Controllers or Minimal API).
2. Students can use any Google AI library: Google.GenAI, Mscc.GenerativeAI, Semantic Kernel (Google connector) or call the Gemini REST API directly with HttpClient. Model: gemini-2.5-flash or a newer Flash model.
3. The Gemini API key must **NOT** be hard-coded in source code. Use User Secrets or environment variables, and remove the key before submitting.
4. Students can make it with Code first or Database first. If you use Database First method please commit all scripts to create database, store procedure ….
5. Database: SQL Server or MySQL. Front End: Razor Pages, MVC, Blazor, ReactJS, Angular, ...
6. AI responses are non-deterministic: marks are given for the correct API name, prompt design, valid JSON structure and data saved to the database, not for the exact wording of the AI output.
7. Submission: source code (remove bin/obj), database script, README.md and screenshots (or a Postman collection) of each API.

***END***
