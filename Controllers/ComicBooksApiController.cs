using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ComicSystem.Models;
using ComicSystem.Services;

namespace ComicSystem.Controllers
{
    [ApiController]
    [Route("api/comicbooks")]
    [Route("api/truyen")]
    public class ComicBooksApiController : ControllerBase
    {
        private readonly IComicBookService _comicBookService;

        public ComicBooksApiController(IComicBookService comicBookService)
        {
            _comicBookService = comicBookService;
        }

        // GET: api/comicbooks or api/truyen
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ComicBook>>> GetComicBooks()
        {
            var books = await _comicBookService.GetAllAsync();
            return Ok(books);
        }

        // GET: api/comicbooks/5 or api/truyen/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ComicBook>> GetComicBook(int id)
        {
            var book = await _comicBookService.GetByIdAsync(id);
            if (book == null)
            {
                return NotFound(new { message = $"Không tìm thấy truyện với ID = {id}" });
            }
            return Ok(book);
        }
    }
}
