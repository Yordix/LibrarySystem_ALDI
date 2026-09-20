using LibrarySystem.API.Data;
using LibrarySystem.API.DTOs.Books;
using LibrarySystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.API.Services
{
    public class BookService
    {
        private readonly LibraryDbContext _libraryDbContext;

        public BookService(LibraryDbContext libraryDbContext)
        {
            _libraryDbContext = libraryDbContext;
        }

        public async Task<List<BookResponseDto>> GetAllAsync(
            bool? available,
            string? author)
        {
            var query = _libraryDbContext.Books.AsQueryable();

            if (available.HasValue)
            {
                query = query.Where(book => book.IsAvailable == available.Value);
            }

            if (!string.IsNullOrEmpty(author))
            {
                query = query.Where(book => book.Author.ToLower().Contains(author.ToLower()));
            }

            return await query.Select(book => new BookResponseDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                PublishedYear = book.PublishedYear,
                IsAvailable = book.IsAvailable
            }).ToListAsync();
        }

        public async Task<BookResponseDto?> GetByIdAsync(Guid id)
        {
            var book = await _libraryDbContext.Books.FindAsync(id);

            if (book == null)
            {
                return null;
            }

            return new BookResponseDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                PublishedYear = book.PublishedYear,
                IsAvailable = book.IsAvailable
            };
        }

        public async Task<BookResponseDto> CreateAsync(CreateBookDto createBookDto)
        {
            var existingBook = await _libraryDbContext.Books.FirstOrDefaultAsync(book => book.ISBN == createBookDto.ISBN);

            if (existingBook != null)
            {
                throw new InvalidOperationException($"A book with ISBN '{createBookDto.ISBN}' already exists.");
            }

            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = createBookDto.Title,
                Author = createBookDto.Author,
                ISBN = createBookDto.ISBN,
                PublishedYear = createBookDto.PublishedYear,
                IsAvailable = true
            };

            _libraryDbContext.Books.Add(book);

            await _libraryDbContext.SaveChangesAsync();

            return new BookResponseDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                PublishedYear = book.PublishedYear,
                IsAvailable = book.IsAvailable
            };
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateBookDto updateBookDto)
        {
            var book = await _libraryDbContext.Books.FindAsync(id);

            if (book == null)
            {
                return false;
            }
            
            var duplicateISBN = await _libraryDbContext.Books
                .AnyAsync(b => b.ISBN == updateBookDto.ISBN && b.Id != id);

            if (duplicateISBN)
            {
                throw new InvalidOperationException($"A book with ISBN '{updateBookDto.ISBN}' already exists.");
            }

            book.Title = updateBookDto.Title;
            book.Author = updateBookDto.Author;
            book.ISBN = updateBookDto.ISBN;
            book.PublishedYear = updateBookDto.PublishedYear;

            await _libraryDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var book = await _libraryDbContext.Books.FindAsync(id);

            if (book == null)
            {
                return false;
            }

            var hasActiveLoan = await _libraryDbContext.Loans.AnyAsync(b => b.BookId == id && b.ReturnDate == null);

            if (hasActiveLoan)
            {
                throw new InvalidOperationException("Cannot delete a book that is currently on loan.");
            }
           
            _libraryDbContext.Books.Remove(book);

            await _libraryDbContext.SaveChangesAsync();

            return true;
        }
    }
}
