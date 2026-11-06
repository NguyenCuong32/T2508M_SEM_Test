using Microsoft.AspNetCore.Mvc;

namespace SunriseHotel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                Message = "SunriseHotel API is running!",
                status = "Success",
                hotel = "Sunrise Hotel"
            });
        }
    }
}
