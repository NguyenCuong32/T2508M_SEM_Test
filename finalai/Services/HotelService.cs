using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SunriseHotelAI.Data;
using SunriseHotelAI.DTOs;
using SunriseHotelAI.Models;

namespace SunriseHotelAI.Services;

public class HotelService : IHotelService
{
    private readonly HotelDbContext _context;
    private readonly ILogger<HotelService> _logger;

    public HotelService(HotelDbContext context, ILogger<HotelService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public static bool TryParseDate(string? input, out DateOnly date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(input)) return false;

        string[] formats = { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "yyyy/MM/dd", "dd-MM-yyyy" };
        return DateOnly.TryParseExact(input.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)
            || DateOnly.TryParse(input.Trim(), CultureInfo.InvariantCulture, out date);
    }

    public static string NormalizeRoomType(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var lower = input.Trim().ToLowerInvariant();

        if (lower.Contains("suite")) return "Suite";
        if (lower.Contains("double") || lower.Contains("đôi") || lower.Contains("doi")) return "Double";
        if (lower.Contains("single") || lower.Contains("đơn") || lower.Contains("don")) return "Single";

        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lower);
    }

    public async Task<List<RoomAvailabilityResult>> CheckAvailabilityAsync(string? roomType, string checkInStr, string checkOutStr)
    {
        if (!TryParseDate(checkInStr, out var checkIn) || !TryParseDate(checkOutStr, out var checkOut))
        {
            _logger.LogWarning("Invalid date range in CheckAvailability: checkIn={CheckIn}, checkOut={CheckOut}", checkInStr, checkOutStr);
            return new List<RoomAvailabilityResult>();
        }

        if (checkOut <= checkIn)
        {
            _logger.LogWarning("CheckOut must be greater than CheckIn: {CheckIn} -> {CheckOut}", checkIn, checkOut);
            return new List<RoomAvailabilityResult>();
        }

        var normalizedType = NormalizeRoomType(roomType);

        var query = _context.Rooms.AsQueryable();
        if (!string.IsNullOrWhiteSpace(normalizedType))
        {
            query = query.Where(r => r.RoomType.ToLower() == normalizedType.ToLower());
        }

        // Available rooms = rooms with no overlapping Confirmed bookings
        var bookedRoomIds = await _context.Bookings
            .Where(b => b.Status == "Confirmed" && b.CheckIn < checkOut && b.CheckOut > checkIn)
            .Select(b => b.RoomId)
            .Distinct()
            .ToListAsync();

        var availableRooms = await query
            .Where(r => !bookedRoomIds.Contains(r.RoomId))
            .OrderBy(r => r.RoomId)
            .Select(r => new RoomAvailabilityResult
            {
                RoomId = r.RoomId,
                RoomNumber = r.RoomNumber,
                RoomType = r.RoomType,
                PricePerNight = r.PricePerNight,
                MaxGuests = r.MaxGuests
            })
            .ToListAsync();

        return availableRooms;
    }

    public async Task<List<object>> GetBookingsByPhoneAsync(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return new List<object>();

        var cleanPhone = phone.Replace(" ", "").Replace("-", "").Replace(".", "").Trim();

        var bookings = await _context.Bookings
            .Include(b => b.Room)
            .Where(b => b.Phone.Replace(" ", "").Replace("-", "").Replace(".", "") == cleanPhone)
            .OrderByDescending(b => b.BookingId)
            .Select(b => new
            {
                b.BookingId,
                RoomNumber = b.Room != null ? b.Room.RoomNumber : "N/A",
                RoomType = b.Room != null ? b.Room.RoomType : "N/A",
                b.GuestName,
                b.Phone,
                CheckIn = b.CheckIn.ToString("dd/MM/yyyy"),
                CheckOut = b.CheckOut.ToString("dd/MM/yyyy"),
                Nights = b.CheckOut.DayNumber - b.CheckIn.DayNumber,
                b.TotalPrice,
                b.Status
            })
            .ToListAsync();

        return bookings.Cast<object>().ToList();
    }

    public async Task<object> CancelBookingAsync(int bookingId)
    {
        var booking = await _context.Bookings.Include(b => b.Room).FirstOrDefaultAsync(b => b.BookingId == bookingId);
        if (booking == null)
        {
            return new
            {
                success = false,
                message = $"Không tìm thấy đơn đặt phòng với mã #{bookingId}."
            };
        }

        if (booking.Status == "Cancelled")
        {
            return new
            {
                success = false,
                message = $"Đơn đặt phòng #{bookingId} của khách {booking.GuestName} đã bị hủy trước đó."
            };
        }

        booking.Status = "Cancelled";
        await _context.SaveChangesAsync();

        return new
        {
            success = true,
            bookingId = booking.BookingId,
            guestName = booking.GuestName,
            roomNumber = booking.Room?.RoomNumber,
            message = $"Đã hủy thành công đơn đặt phòng #{bookingId} của khách hàng {booking.GuestName}."
        };
    }

    public async Task<BookingResponse> CreateBookingFromParsedDataAsync(ParsedBookingData data)
    {
        if (!TryParseDate(data.CheckIn, out var checkIn))
        {
            return new BookingResponse
            {
                Success = false,
                Message = $"Ngày nhận phòng '{data.CheckIn}' không hợp lệ (định dạng hợp lệ: YYYY-MM-DD hoặc dd/MM/yyyy).",
                ExtractedData = data
            };
        }

        if (!TryParseDate(data.CheckOut, out var checkOut))
        {
            return new BookingResponse
            {
                Success = false,
                Message = $"Ngày trả phòng '{data.CheckOut}' không hợp lệ (định dạng hợp lệ: YYYY-MM-DD hoặc dd/MM/yyyy).",
                ExtractedData = data
            };
        }

        if (checkOut <= checkIn)
        {
            return new BookingResponse
            {
                Success = false,
                Message = $"Ngày trả phòng ({checkOut:dd/MM/yyyy}) phải sau ngày nhận phòng ({checkIn:dd/MM/yyyy}).",
                ExtractedData = data
            };
        }

        var normalizedType = NormalizeRoomType(data.RoomType);
        var validTypes = new[] { "Single", "Double", "Suite" };
        if (!validTypes.Contains(normalizedType))
        {
            return new BookingResponse
            {
                Success = false,
                Message = $"Loại phòng '{data.RoomType}' không hợp lệ. Khách sạn chỉ có các loại phòng: Single, Double, Suite.",
                ExtractedData = data
            };
        }

        // Check availability
        var bookedRoomIds = await _context.Bookings
            .Where(b => b.Status == "Confirmed" && b.CheckIn < checkOut && b.CheckOut > checkIn)
            .Select(b => b.RoomId)
            .Distinct()
            .ToListAsync();

        var availableRoom = await _context.Rooms
            .Where(r => r.RoomType.ToLower() == normalizedType.ToLower() && !bookedRoomIds.Contains(r.RoomId))
            .OrderBy(r => r.RoomId)
            .FirstOrDefaultAsync();

        if (availableRoom == null)
        {
            return new BookingResponse
            {
                Success = false,
                Message = $"Rất tiếc, loại phòng {normalizedType} đã hết phòng trống trong khoảng thời gian từ {checkIn:dd/MM/yyyy} đến {checkOut:dd/MM/yyyy}.",
                ExtractedData = data
            };
        }

        // Validate guests against max guests
        if (data.Guests > availableRoom.MaxGuests)
        {
            return new BookingResponse
            {
                Success = false,
                Message = $"Phòng {normalizedType} chỉ chứa tối đa {availableRoom.MaxGuests} khách. Yêu cầu của bạn là {data.Guests} khách.",
                ExtractedData = data
            };
        }

        int nights = checkOut.DayNumber - checkIn.DayNumber;
        decimal totalPrice = nights * availableRoom.PricePerNight;

        var newBooking = new Booking
        {
            RoomId = availableRoom.RoomId,
            GuestName = string.IsNullOrWhiteSpace(data.GuestName) ? "Khách vãng lai" : data.GuestName.Trim(),
            Phone = string.IsNullOrWhiteSpace(data.Phone) ? "N/A" : data.Phone.Trim(),
            CheckIn = checkIn,
            CheckOut = checkOut,
            TotalPrice = totalPrice,
            Status = "Confirmed"
        };

        _context.Bookings.Add(newBooking);
        await _context.SaveChangesAsync();

        return new BookingResponse
        {
            Success = true,
            Message = $"Đặt phòng thành công! Mã đơn: #{newBooking.BookingId}, Phòng {availableRoom.RoomNumber} ({availableRoom.RoomType}).",
            BookingId = newBooking.BookingId,
            RoomId = availableRoom.RoomId,
            RoomNumber = availableRoom.RoomNumber,
            RoomType = availableRoom.RoomType,
            GuestName = newBooking.GuestName,
            Phone = newBooking.Phone,
            CheckIn = checkIn.ToString("dd/MM/yyyy"),
            CheckOut = checkOut.ToString("dd/MM/yyyy"),
            Nights = nights,
            PricePerNight = availableRoom.PricePerNight,
            TotalPrice = totalPrice,
            Status = newBooking.Status,
            ExtractedData = data
        };
    }

    public async Task<List<BookingReportItemDto>> GetBookingReportAsync(DateOnly? from, DateOnly? to)
    {
        var query = _context.Bookings
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

        var list = await query
            .OrderBy(b => b.CheckIn)
            .ThenBy(b => b.BookingId)
            .ToListAsync();

        int stt = 1;
        return list.Select(b => new BookingReportItemDto
        {
            No = stt++,
            RoomNumber = b.Room?.RoomNumber ?? "N/A",
            RoomType = b.Room?.RoomType ?? "N/A",
            GuestName = b.GuestName,
            CheckIn = b.CheckIn.ToString("dd/MM/yyyy"),
            CheckOut = b.CheckOut.ToString("dd/MM/yyyy"),
            Nights = b.CheckOut.DayNumber - b.CheckIn.DayNumber,
            TotalPrice = b.TotalPrice,
            Status = b.Status
        }).ToList();
    }
}
