using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SunriseHotel.Data;
using SunriseHotel.Models;
using SunriseHotel.Models.DTOs;

namespace SunriseHotel.Services;

public class BookingService : IBookingService
{
    private readonly SunriseDbContext _context;

    public BookingService(SunriseDbContext context)
    {
        _context = context;
    }

    public async Task<List<AvailableRoomDto>> CheckAvailabilityAsync(string roomType, string checkInStr, string checkOutStr)
    {
        if (!TryParseFlexibleDate(checkInStr, out var checkIn) || !TryParseFlexibleDate(checkOutStr, out var checkOut))
        {
            return new List<AvailableRoomDto>();
        }

        if (checkOut <= checkIn)
        {
            return new List<AvailableRoomDto>();
        }

        var bookedRoomIds = await _context.Booking
            .Where(b => b.Status == "Confirmed" && b.CheckIn < checkOut && b.CheckOut > checkIn)
            .Select(b => b.RoomId)
            .Distinct()
            .ToListAsync();

        var query = _context.Room.Where(r => !bookedRoomIds.Contains(r.RoomId));

        if (!string.IsNullOrWhiteSpace(roomType))
        {
            var normalizedType = NormalizeRoomType(roomType);
            query = query.Where(r => r.RoomType.ToLower() == normalizedType.ToLower());
        }

        var rooms = await query.OrderBy(r => r.PricePerNight).ToListAsync();

        return rooms.Select(r => new AvailableRoomDto
        {
            RoomId = r.RoomId,
            RoomNumber = r.RoomNumber,
            RoomType = r.RoomType,
            PricePerNight = r.PricePerNight,
            FormattedPrice = $"{r.PricePerNight:N0} VND",
            MaxGuests = r.MaxGuests
        }).ToList();
    }

    public async Task<BookingResponseDto> CreateBookingFromParsedDtoAsync(ParsedBookingDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.GuestName))
        {
            throw new ArgumentException("Tên khách hàng không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(dto.Phone))
        {
            throw new ArgumentException("Số điện thoại không được để trống.");
        }

        if (!TryParseFlexibleDate(dto.CheckIn, out var checkIn))
        {
            throw new ArgumentException($"Ngày nhận phòng không hợp lệ: {dto.CheckIn}");
        }

        if (!TryParseFlexibleDate(dto.CheckOut, out var checkOut))
        {
            throw new ArgumentException($"Ngày trả phòng không hợp lệ: {dto.CheckOut}");
        }

        if (checkOut <= checkIn)
        {
            throw new ArgumentException("Ngày trả phòng phải sau ngày nhận phòng.");
        }

        var normalizedType = NormalizeRoomType(dto.RoomType);
        if (normalizedType != "Single" && normalizedType != "Double" && normalizedType != "Suite")
        {
            throw new ArgumentException($"Loại phòng không hợp lệ: {dto.RoomType}. Các loại phòng hợp lệ: Single, Double, Suite.");
        }

        var bookedRoomIds = await _context.Booking
            .Where(b => b.Status == "Confirmed" && b.CheckIn < checkOut && b.CheckOut > checkIn)
            .Select(b => b.RoomId)
            .Distinct()
            .ToListAsync();

        var availableRoom = await _context.Room
            .Where(r => r.RoomType.ToLower() == normalizedType.ToLower() && !bookedRoomIds.Contains(r.RoomId))
            .OrderBy(r => r.RoomId)
            .FirstOrDefaultAsync();

        if (availableRoom == null)
        {
            throw new InvalidOperationException($"Rất tiếc, hiện tại không còn phòng {normalizedType} nào trống trong khoảng thời gian từ {checkIn:dd/MM/yyyy} đến {checkOut:dd/MM/yyyy}.");
        }

        var nights = checkOut.DayNumber - checkIn.DayNumber;
        var totalPrice = nights * availableRoom.PricePerNight;

        var booking = new Booking
        {
            RoomId = availableRoom.RoomId,
            GuestName = dto.GuestName.Trim(),
            Phone = dto.Phone.Trim(),
            CheckIn = checkIn,
            CheckOut = checkOut,
            TotalPrice = totalPrice,
            Status = "Confirmed"
        };

        _context.Booking.Add(booking);
        await _context.SaveChangesAsync();

        return new BookingResponseDto
        {
            BookingId = booking.BookingId,
            RoomId = availableRoom.RoomId,
            RoomNumber = availableRoom.RoomNumber,
            RoomType = availableRoom.RoomType,
            GuestName = booking.GuestName,
            Phone = booking.Phone,
            CheckIn = booking.CheckIn.ToString("yyyy-MM-dd"),
            CheckOut = booking.CheckOut.ToString("yyyy-MM-dd"),
            Nights = nights,
            TotalPrice = booking.TotalPrice,
            FormattedTotalPrice = $"{booking.TotalPrice:N0} VND",
            Status = booking.Status,
            Message = $"Đặt phòng thành công! Mã đặt phòng của bạn là #{booking.BookingId} (Phòng {availableRoom.RoomNumber} - {availableRoom.RoomType})."
        };
    }

    public async Task<List<PhoneBookingDto>> GetBookingsByPhoneAsync(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return new List<PhoneBookingDto>();
        }

        var cleanPhone = new string(phone.Where(char.IsDigit).ToArray());

        var bookings = await _context.Booking
            .Include(b => b.Room)
            .Where(b => b.Phone.Contains(cleanPhone) || b.Phone.Contains(phone.Trim()))
            .OrderByDescending(b => b.BookingId)
            .ToListAsync();

        return bookings.Select(b => new PhoneBookingDto
        {
            BookingId = b.BookingId,
            RoomNumber = b.Room?.RoomNumber ?? b.RoomId.ToString(),
            RoomType = b.Room?.RoomType ?? string.Empty,
            GuestName = b.GuestName,
            CheckIn = b.CheckIn.ToString("dd/MM/yyyy"),
            CheckOut = b.CheckOut.ToString("dd/MM/yyyy"),
            TotalPrice = b.TotalPrice,
            FormattedTotalPrice = $"{b.TotalPrice:N0} VND",
            Status = b.Status
        }).ToList();
    }

    public async Task<CancelBookingResultDto> CancelBookingAsync(int bookingId)
    {
        var booking = await _context.Booking
            .Include(b => b.Room)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);

        if (booking == null)
        {
            return new CancelBookingResultDto
            {
                Success = false,
                BookingId = bookingId,
                Message = $"Không tìm thấy mã đặt phòng #{bookingId} trong hệ thống Sunrise Hotel."
            };
        }

        if (booking.Status == "Cancelled")
        {
            return new CancelBookingResultDto
            {
                Success = true,
                BookingId = bookingId,
                Message = $"Đặt phòng #{bookingId} của khách hàng {booking.GuestName} đã được hủy từ trước đó."
            };
        }

        booking.Status = "Cancelled";
        await _context.SaveChangesAsync();

        return new CancelBookingResultDto
        {
            Success = true,
            BookingId = bookingId,
            Message = $"Đã hủy thành công mã đặt phòng #{bookingId} của khách hàng {booking.GuestName} (Phòng {booking.Room?.RoomNumber ?? booking.RoomId.ToString()})."
        };
    }

    public async Task<List<BookingReportDto>> GetBookingReportAsync(string? fromDate, string? toDate)
    {
        var query = _context.Booking
            .Include(b => b.Room)
            .Where(b => b.Status == "Confirmed");

        if (!string.IsNullOrWhiteSpace(fromDate) && TryParseFlexibleDate(fromDate, out var from))
        {
            query = query.Where(b => b.CheckIn >= from);
        }

        if (!string.IsNullOrWhiteSpace(toDate) && TryParseFlexibleDate(toDate, out var to))
        {
            query = query.Where(b => b.CheckIn <= to);
        }

        var list = await query
            .OrderBy(b => b.CheckIn)
            .ThenBy(b => b.BookingId)
            .ToListAsync();

        var result = new List<BookingReportDto>();
        for (int i = 0; i < list.Count; i++)
        {
            var b = list[i];
            var nights = b.CheckOut.DayNumber - b.CheckIn.DayNumber;
            result.Add(new BookingReportDto
            {
                No = i + 1,
                RoomNumber = b.Room?.RoomNumber ?? b.RoomId.ToString(),
                RoomType = b.Room?.RoomType ?? string.Empty,
                GuestName = b.GuestName,
                CheckIn = b.CheckIn.ToString("dd/MM/yyyy"),
                CheckOut = b.CheckOut.ToString("dd/MM/yyyy"),
                Nights = nights,
                TotalPrice = b.TotalPrice,
                FormattedTotalPrice = $"{b.TotalPrice:N0}",
                Status = b.Status
            });
        }

        return result;
    }

    public async Task<List<Booking>> GetAllBookingsAsync()
    {
        return await _context.Booking
            .Include(b => b.Room)
            .OrderByDescending(b => b.BookingId)
            .ToListAsync();
    }

    private static string NormalizeRoomType(string rawType)
    {
        if (string.IsNullOrWhiteSpace(rawType)) return "Single";
        var lower = rawType.Trim().ToLower();
        if (lower.Contains("suite")) return "Suite";
        if (lower.Contains("đôi") || lower.Contains("doi") || lower.Contains("double")) return "Double";
        if (lower.Contains("đơn") || lower.Contains("don") || lower.Contains("single")) return "Single";
        return rawType.Trim();
    }

    private static bool TryParseFlexibleDate(string? input, out DateOnly date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(input)) return false;

        string[] formats = { "yyyy-MM-dd", "yyyy/MM/dd", "dd/MM/yyyy", "dd-MM-yyyy", "d/M/yyyy", "d-M-yyyy" };
        if (DateOnly.TryParseExact(input.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return true;
        }

        if (DateOnly.TryParse(input.Trim(), out date))
        {
            return true;
        }

        if (DateTime.TryParse(input.Trim(), out var dt))
        {
            date = DateOnly.FromDateTime(dt);
            return true;
        }

        return false;
    }
}

