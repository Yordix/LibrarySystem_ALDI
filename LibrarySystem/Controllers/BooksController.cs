using LibrarySystem.API.DTOs.Books;
using LibrarySystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly BookService _bookService;
        
        public BooksController(BookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool? available,
                                                [FromQuery] string? author)
        {
            var books = await _bookService.GetAllAsync(available, author);

            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var book = await _bookService.GetByIdAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);

        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateBookDto createBookDto)
        {
            try
            {
                var book = await _bookService.CreateAsync(createBookDto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = book.Id },
                    book);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            Guid id,
            UpdateBookDto updateBookDto)
        {
            try
            {
                var success = await _bookService.UpdateAsync(id, updateBookDto);

                if (!success)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var success = await _bookService.DeleteAsync(id);

                if (!success)
                {
                    return NotFound();
                }

                return NoContent();

            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}
