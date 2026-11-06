using Microsoft.AspNetCore.Mvc;
using SUNRISEHOTEL.Data;
using SUNRISEHOTEL.DTOs;
using SUNRISEHOTEL.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SUNRISEHOTEL.Controllers
{
    [Route("api/HotelAI")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly IGeminiService _geminiService;

        public AIController(IGeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask(
            [FromBody] AskHotelRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest("Question is required.");

            var answer = await _geminiService.AskAsync(
                request.Question,
                HotelPrompts.Receptionist
            );

            return Ok(new
            {
                success = true,
                answer
            });
        }
    }
}
