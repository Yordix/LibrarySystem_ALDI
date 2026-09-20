using LibrarySystem.API.Data;
using LibrarySystem.API.DTOs.Books;
using LibrarySystem.API.Models;
using LibrarySystem.API.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem.Tests
{
    public class BookServiceTests
    {
        private LibraryDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<LibraryDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new LibraryDbContext(options);
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateBook()
        {
            using var context = CreateDbContext();
            var service = new BookService(context);

            var dto = new CreateBookDto
            {
                Title = "Test Book",
                Author = "Test Author",
                ISBN = "1234567890",
                PublishedYear = 2023
            };

            var result = await service.CreateAsync(dto);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal("Test Book", result.Title);
            Assert.Equal("Test Author", result.Author);
            Assert.True(result.IsAvailable);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenIsbnAlreadyExists()
        {
            using var context = CreateDbContext();

            var service = new BookService(context);

            var dto = new CreateBookDto
            {
                Title = "Test Book2",
                Author = "Test Author2",
                ISBN = "1234567890",
                PublishedYear = 2024
            };

            await service.CreateAsync(dto);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(dto));
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnBook()
        {
            using var context = CreateDbContext();
            var service = new BookService(context);

            var createdBook = await service.CreateAsync(new CreateBookDto
            {
                Title = "The Hobbit",
                Author = "J.R.R. Tolkien",
                ISBN = "987654321",
                PublishedYear = 1937
            });

            var result = await service.GetByIdAsync(createdBook.Id);

            Assert.NotNull(result);
            Assert.Equal("The Hobbit", result.Title);
        }


        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenBookDoesNotExist()
        {
            using var context = CreateDbContext();
            var service = new BookService(context);

            var result = await service.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ShouldFilterByAvailability()
        {
            using var context = CreateDbContext();
            var service = new BookService(context);

            var book = await service.CreateAsync(
                new CreateBookDto
                {
                    Title = "Book 1",
                    Author = "Author 1",
                    ISBN = "111",
                    PublishedYear = 2020
                });

            await service.CreateAsync(
                new CreateBookDto
                {
                    Title = "Book 2",
                    Author = "Author 2",
                    ISBN = "222",
                    PublishedYear = 2021
                });

            var entity = await context.Books.FindAsync(book.Id);
            entity!.IsAvailable = false;
            await context.SaveChangesAsync();

            var result = await service.GetAllAsync(false, null);

            Assert.Single(result);
            Assert.Equal("Book 1", result[0].Title);
        }

        [Fact]
        public async Task GetAllAsync_ShouldFilterByAuthor()
        {
            using var context = CreateDbContext();
            var service = new BookService(context);

            await service.CreateAsync(
                new CreateBookDto
                {
                    Title = "1984",
                    Author = "George Orwell",
                    ISBN = "111",
                    PublishedYear = 1949
                });

            await service.CreateAsync(
                new CreateBookDto
                {
                    Title = "The Hobbit",
                    Author = "J.R.R. Tolkien",
                    ISBN = "222",
                    PublishedYear = 1937
                });

            var result = await service.GetAllAsync(
                null,
                "orwell");

            Assert.Single(result);
            Assert.Equal("1984", result[0].Title);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteBook()
        {
            using var context = CreateDbContext();
            var service = new BookService(context);

            var book = await service.CreateAsync(
                new CreateBookDto
                {
                    Title = "Book",
                    Author = "Author",
                    ISBN = "123",
                    PublishedYear = 2020
                });

            var result = await service.DeleteAsync(book.Id);

            Assert.True(result);

            var deletedBook = await context.Books.FindAsync(book.Id);

            Assert.Null(deletedBook);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrow_WhenBookHasActiveLoan()
        {
            using var context = CreateDbContext();
            var service = new BookService(context);

            var book = await service.CreateAsync(
                new CreateBookDto
                {
                    Title = "Book",
                    Author = "Author",
                    ISBN = "123",
                    PublishedYear = 2020
                });

            context.Loans.Add(new Loan
            {
                Id = Guid.NewGuid(),
                BookId = book.Id,
                UserId = Guid.NewGuid(),
                LoanDate = DateTime.UtcNow,
                ReturnDate = null
            });

            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.DeleteAsync(book.Id));
        }
    }
}
