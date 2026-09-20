using LibrarySystem.API.DTOs.Loans;
using LibrarySystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly LoanService _loanService;

        public LoansController(LoanService loanService)
        {
            _loanService = loanService;
        }

        [HttpPost]
        public async Task<IActionResult> Borrow(BorrowBookDto borrowBookDto)
        {
            try
            {
                var loan = await _loanService.BorrowAsync(borrowBookDto);

                return Ok(loan);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPut("{id}/return")]
        public async Task<IActionResult> Return(Guid id)
        {
            try
            {
                var success = await _loanService.ReturnAsync(id);

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

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveLoans()
        {
            var loans = await _loanService.GetAllAsync();

            return Ok(loans);
        }

    }
}
