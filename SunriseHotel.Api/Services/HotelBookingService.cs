using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SunriseHotel.Api.Data;
using SunriseHotel.Api.DTOs;
using SunriseHotel.Api.Models;

namespace SunriseHotel.Api.Services;

public class HotelBookingService : IHotelBookingService
{
    private readonly SunriseHotelDbContext _dbContext;
    private readonly ILogger<HotelBookingService> _logger;

    private static readonly string[] AllowedRoomTypes = { "Single", "Double", "Suite" };
    private static readonly string[] DateFormats = { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "yyyy/MM/dd" };

    public HotelBookingService(SunriseHotelDbContext dbContext, ILogger<HotelBookingService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<Room>> CheckAvailabilityAsync(string? roomType, DateOnly checkIn, DateOnly checkOut)
    {
        if (checkOut <= checkIn)
        {
            return new List<Room>();
        }

        var query = _dbContext.Rooms.AsQueryable();

        if (!string.IsNullOrWhiteSpace(roomType))
        {
            var normalizedType = NormalizeRoomType(roomType);
            if (!string.IsNullOrEmpty(normalizedType))
            {
                query = query.Where(r => r.RoomType.ToLower() == normalizedType.ToLower());
            }
        }

        // Available rooms: rooms with NO active Confirmed booking that overlaps with [checkIn, checkOut]
        // Two intervals [A, B] and [C, D] overlap iff A < D and C < B
        var bookedRoomIds = await _dbContext.Bookings
            .Where(b => b.Status == "Confirmed" && b.CheckIn < checkOut && checkIn < b.CheckOut)
            .Select(b => b.RoomId)
            .Distinct()
            .ToListAsync();

        var availableRooms = await query
            .Where(r => !bookedRoomIds.Contains(r.RoomId))
            .OrderBy(r => r.RoomNumber)
            .ToListAsync();

        return availableRooms;
    }

    public async Task<List<BookingResultDto>> GetBookingsByPhoneAsync(string phone)
    {
        var cleanPhone = phone?.Trim() ?? string.Empty;

        var bookings = await _dbContext.Bookings
            .Include(b => b.Room)
            .Where(b => b.Phone == cleanPhone || b.Phone.EndsWith(cleanPhone) || cleanPhone.EndsWith(b.Phone))
            .OrderByDescending(b => b.BookingId)
            .ToListAsync();

        return bookings.Select(b => new BookingResultDto
        {
            BookingId = b.BookingId,
            RoomId = b.RoomId,
            RoomNumber = b.Room?.RoomNumber ?? "N/A",
            RoomType = b.Room?.RoomType ?? "N/A",
            GuestName = b.GuestName,
            Phone = b.Phone,
            CheckIn = b.CheckIn.ToString("dd/MM/yyyy"),
            CheckOut = b.CheckOut.ToString("dd/MM/yyyy"),
            Nights = b.CheckOut.DayNumber - b.CheckIn.DayNumber,
            TotalPrice = b.TotalPrice,
            Status = b.Status
        }).ToList();
    }

    public async Task<(bool Success, string Message)> CancelBookingAsync(int bookingId)
    {
        var booking = await _dbContext.Bookings.FindAsync(bookingId);
        if (booking == null)
        {
            return (false, $"Không tìm thấy booking với mã #{bookingId}.");
        }

        if (booking.Status == "Cancelled")
        {
            return (false, $"Booking #{bookingId} đã được hủy trước đó.");
        }

        booking.Status = "Cancelled";
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Booking #{BookingId} was successfully cancelled.", bookingId);
        return (true, $"Đã hủy thành công booking #{bookingId} của khách hàng {booking.GuestName}.");
    }

    public async Task<(bool Success, string Message, BookingResultDto? Booking)> CreateBookingAsync(ParsedBookingDto dto)
    {
        // 1. Validate Guest Name & Phone
        if (string.IsNullOrWhiteSpace(dto.GuestName))
        {
            return (false, "Tên khách hàng không được để trống.", null);
        }

        if (string.IsNullOrWhiteSpace(dto.Phone))
        {
            return (false, "Số điện thoại không được để trống.", null);
        }

        // 2. Parse Dates
        if (!TryParseDate(dto.CheckIn, out var checkIn))
        {
            return (false, $"Ngày nhận phòng không hợp lệ: '{dto.CheckIn}'. Định dạng hợp lệ: YYYY-MM-DD hoặc DD/MM/YYYY.", null);
        }

        if (!TryParseDate(dto.CheckOut, out var checkOut))
        {
            return (false, $"Ngày trả phòng không hợp lệ: '{dto.CheckOut}'. Định dạng hợp lệ: YYYY-MM-DD hoặc DD/MM/YYYY.", null);
        }

        // 3. Validate checkOut > checkIn
        if (checkOut <= checkIn)
        {
            return (false, $"Ngày trả phòng ({checkOut:dd/MM/yyyy}) phải sau ngày nhận phòng ({checkIn:dd/MM/yyyy}).", null);
        }

        // 4. Validate & Normalize roomType
        var normalizedRoomType = NormalizeRoomType(dto.RoomType);
        if (string.IsNullOrEmpty(normalizedRoomType))
        {
            return (false, $"Loại phòng không hợp lệ: '{dto.RoomType}'. Khách sạn chỉ có các loại phòng: Single, Double, Suite.", null);
        }

        // 5. Find an available room of that type
        var availableRooms = await CheckAvailabilityAsync(normalizedRoomType, checkIn, checkOut);

        // Optionally filter by max guests if specified
        if (dto.Guests > 0)
        {
            var capacityMatched = availableRooms.Where(r => r.MaxGuests >= dto.Guests).ToList();
            if (capacityMatched.Any())
            {
                availableRooms = capacityMatched;
            }
        }

        if (!availableRooms.Any())
        {
            return (false, $"Hiện không còn phòng {normalizedRoomType} nào trống trong khoảng thời gian từ {checkIn:dd/MM/yyyy} đến {checkOut:dd/MM/yyyy}.", null);
        }

        var room = availableRooms.First();

        // 6. Calculate TotalPrice
        int nights = checkOut.DayNumber - checkIn.DayNumber;
        decimal totalPrice = nights * room.PricePerNight;

        // 7. Save new booking with status Confirmed
        var booking = new Booking
        {
            RoomId = room.RoomId,
            GuestName = dto.GuestName.Trim(),
            Phone = dto.Phone.Trim(),
            CheckIn = checkIn,
            CheckOut = checkOut,
            TotalPrice = totalPrice,
            Status = "Confirmed"
        };

        _dbContext.Bookings.Add(booking);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created new booking #{BookingId} for {GuestName}, Room {RoomNumber}",
            booking.BookingId, booking.GuestName, room.RoomNumber);

        var resultDto = new BookingResultDto
        {
            BookingId = booking.BookingId,
            RoomId = room.RoomId,
            RoomNumber = room.RoomNumber,
            RoomType = room.RoomType,
            GuestName = booking.GuestName,
            Phone = booking.Phone,
            CheckIn = booking.CheckIn.ToString("dd/MM/yyyy"),
            CheckOut = booking.CheckOut.ToString("dd/MM/yyyy"),
            Nights = nights,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status
        };

        return (true, "Đặt phòng thành công!", resultDto);
    }

    public async Task<List<BookingReportItemDto>> GetBookingReportAsync(DateOnly? from, DateOnly? to)
    {
        var query = _dbContext.Bookings
            .Include(b => b.Room)
            .Where(b => b.Status == "Confirmed")
            .AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(b => b.CheckIn >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(b => b.CheckIn <= to.Value);
        }

        var bookings = await query
            .OrderBy(b => b.CheckIn)
            .ThenBy(b => b.BookingId)
            .ToListAsync();

        var report = new List<BookingReportItemDto>();
        int index = 1;
        foreach (var b in bookings)
        {
            report.Add(new BookingReportItemDto
            {
                No = index++,
                RoomNumber = b.Room?.RoomNumber ?? "N/A",
                RoomType = b.Room?.RoomType ?? "N/A",
                GuestName = b.GuestName,
                CheckIn = b.CheckIn.ToString("dd/MM/yyyy"),
                CheckOut = b.CheckOut.ToString("dd/MM/yyyy"),
                Nights = b.CheckOut.DayNumber - b.CheckIn.DayNumber,
                TotalPrice = b.TotalPrice
            });
        }

        return report;
    }

    private static bool TryParseDate(string? input, out DateOnly date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(input)) return false;

        foreach (var fmt in DateFormats)
        {
            if (DateOnly.TryParseExact(input.Trim(), fmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return true;
            }
        }

        return DateOnly.TryParse(input.Trim(), out date);
    }

    private static string? NormalizeRoomType(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var lower = raw.Trim().ToLowerInvariant();
        if (lower.Contains("đơn") || lower == "single") return "Single";
        if (lower.Contains("đôi") || lower == "double") return "Double";
        if (lower.Contains("suite") || lower.Contains("vip") || lower.Contains("cao cấp")) return "Suite";

        foreach (var t in AllowedRoomTypes)
        {
            if (t.Equals(raw.Trim(), StringComparison.OrdinalIgnoreCase)) return t;
        }

        return null;
    }
}

