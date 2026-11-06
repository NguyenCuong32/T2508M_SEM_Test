using SUNRISEHOTEL.DTOs;
using SUNRISEHOTEL.Models;
using SUNRISEHOTEL.Repositories;

namespace SUNRISEHOTEL.Services
{
    public class BookingService : IBookingService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IBookingRepository _bookingRepository;

        public BookingService(
            IRoomRepository roomRepository,
            IBookingRepository bookingRepository)
        {
            _roomRepository = roomRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<Booking> CreateBookingAsync(
            BookingRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.GuestName))
                throw new ArgumentException("Guest name is required.");

            if (string.IsNullOrWhiteSpace(request.Phone))
                throw new ArgumentException("Phone is required.");

            if (request.CheckOut <= request.CheckIn)
                throw new ArgumentException(
                    "Check-out must be after check-in.");

            if (request.Guests <= 0)
                throw new ArgumentException("Invalid guest count.");

            var validTypes = new[] { "Single", "Double", "Suite" };

            if (!validTypes.Contains(
                request.RoomType,
                StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Invalid room type.");
            }

            request.RoomType = validTypes.First(
                x => x.Equals(
                    request.RoomType,
                    StringComparison.OrdinalIgnoreCase));

            var room = await _roomRepository.GetAvailableRoomAsync(
                request.RoomType,
                request.CheckIn,
                request.CheckOut,
                request.Guests
            );

            if (room == null)
                throw new InvalidOperationException(
                    "No available room found.");

            var nights = request.CheckOut.DayNumber
                         - request.CheckIn.DayNumber;

            var booking = new Booking
            {
                RoomId = room.Id,
                GuestName = request.GuestName,
                Phone = request.Phone,
                CheckIn = request.CheckIn,
                CheckOut = request.CheckOut,
                TotalPrice = nights * room.PricePerNight,
                Status = "Confirmed"
            };

            return await _bookingRepository.CreateAsync(booking);
        }

        public Task<List<Room>> CheckAvailabilityAsync(
            string roomType,
            DateOnly checkIn,
            DateOnly checkOut)
        {
            return _roomRepository.GetAvailableRoomsAsync(
                roomType, checkIn, checkOut);
        }

        public Task<List<Booking>> GetBookingsByPhoneAsync(string phone)
        {
            return _bookingRepository.GetByPhoneAsync(phone);
        }

        public Task<bool> CancelBookingAsync(int bookingId)
        {
            return _bookingRepository.CancelAsync(bookingId);
        }
    }
}
