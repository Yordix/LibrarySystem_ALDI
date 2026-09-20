using LibrarySystem.API.Data;
using LibrarySystem.API.DTOs.Loans;
using LibrarySystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.API.Services
{
    public class LoanService
    {
        private readonly LibraryDbContext _libraryDbContext;

        public LoanService(LibraryDbContext libraryDbContext)
        {
            _libraryDbContext = libraryDbContext;
        }

        public async Task<LoanResponseDto> BorrowAsync(BorrowBookDto borrowBookDto)
        {
            var user = await _libraryDbContext.Users.FindAsync(borrowBookDto.UserId);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {borrowBookDto.UserId} not found.");
            }

            var book = await _libraryDbContext.Books.FindAsync(borrowBookDto.BookId);

            if (book == null)
            {
                throw new KeyNotFoundException($"Book with ID {borrowBookDto.BookId} not found.");
            }

            if (!book.IsAvailable)
            {
                throw new InvalidOperationException($"Book with ID {borrowBookDto.BookId} is not available for borrowing.");
            }

            var loan = new Loan
            {
                Id = Guid.NewGuid(),
                UserId = borrowBookDto.UserId,
                BookId = borrowBookDto.BookId,
                LoanDate = DateTime.UtcNow,
                ReturnDate = null
            };

            book.IsAvailable = false;

            _libraryDbContext.Loans.Add(loan);

            await _libraryDbContext.SaveChangesAsync();

            return new LoanResponseDto
            {
                Id = loan.Id,
                UserId = loan.UserId,
                BookId = loan.BookId,
                LoanDate = loan.LoanDate,
                ReturnDate = loan.ReturnDate
            };
        }

        public async Task<bool> ReturnAsync(Guid loanId)
        {
            var loan = await _libraryDbContext.Loans.Include(l => l.Book).FirstOrDefaultAsync(l => l.Id == loanId);

            if (loan == null)
            {
                return false;
            }

            if (loan.ReturnDate != null)
            {
                throw new InvalidOperationException($"Loan with ID {loanId} has already been returned.");
            }

            loan.ReturnDate = DateTime.UtcNow;

            loan.Book.IsAvailable = true;

            await _libraryDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<List<LoanResponseDto>> GetAllAsync()
        {
            return await _libraryDbContext.Loans
                .Where(l => l.ReturnDate == null)
                .Select(l => new LoanResponseDto
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    BookId = l.BookId,
                    LoanDate = l.LoanDate,
                    ReturnDate = l.ReturnDate
                })
                .ToListAsync();
        }
    }   
}
