using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SunriseHotel.Data;

namespace SunriseHotel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController: ControllerBase
    {
        private readonly HotelDBContext _context;
        public RoomsController(HotelDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetRooms(int ID)
        {
            var rooms = await _context.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.roomId == ID);
            if(rooms == null)
            {
                return NotFound(new { message = "Room not found" });
            }
            return Ok(rooms);
        }
    }
}
