using Microsoft.AspNetCore.Mvc;
using SUNRISEHOTEL.DTOs;
using SUNRISEHOTEL.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SUNRISEHOTEL.Controllers
{
    [ApiController]
    [Route("api/assistant")]
    public class AssistantController : ControllerBase
    {
        private readonly IHotelAssistantService _assistantService;

        public AssistantController(
            IHotelAssistantService assistantService)
        {
            _assistantService = assistantService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat(
            [FromBody] AssistantChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Message is required.");

            var answer = await _assistantService.ChatAsync(
                request.Message
            );

            return Ok(new
            {
                success = true,
                answer
            });
        }
    }
}
