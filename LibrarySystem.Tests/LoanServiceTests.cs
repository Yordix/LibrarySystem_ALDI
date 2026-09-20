using LibrarySystem.API.Data;
using LibrarySystem.API.DTOs.Books;
using LibrarySystem.API.DTOs.Loans;
using LibrarySystem.API.DTOs.Users;
using LibrarySystem.API.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySystem.Tests
{
    public class LoanServiceTests
    {
        private LibraryDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<LibraryDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new LibraryDbContext(options);
        }

        [Fact]
        public async Task BorrowAsync_ShouldCreateLoan()
        {
            // Arrange
            using var context = CreateDbContext();

            var userService = new UserService(context);
            var bookService = new BookService(context);
            var loanService = new LoanService(context);

            var user = await userService.RegisterAsync(
                new CreateUserDto
                {
                    Name = "John",
                    Email = "john@example.com"
                });

            var book = await bookService.CreateAsync(
                new CreateBookDto
                {
                    Title = "1984",
                    Author = "George Orwell",
                    ISBN = "123",
                    PublishedYear = 1949
                });

            // Act
            var loan = await loanService.BorrowAsync(
                new BorrowBookDto
                {
                    UserId = user.Id,
                    BookId = book.Id
                });

            // Assert
            Assert.NotEqual(Guid.Empty, loan.Id);
            Assert.Equal(user.Id, loan.UserId);
            Assert.Equal(book.Id, loan.BookId);
            Assert.Null(loan.ReturnDate);
        }

        [Fact]
        public async Task BorrowAsync_ShouldMakeBookUnavailable()
        {
            // Arrange
            using var context = CreateDbContext();

            var userService = new UserService(context);
            var bookService = new BookService(context);
            var loanService = new LoanService(context);

            var user = await userService.RegisterAsync(
                new CreateUserDto
                {
                    Name = "John",
                    Email = "john@example.com"
                });

            var book = await bookService.CreateAsync(
                new CreateBookDto
                {
                    Title = "1984",
                    Author = "George Orwell",
                    ISBN = "123",
                    PublishedYear = 1949
                });

            // Act
            await loanService.BorrowAsync(
                new BorrowBookDto
                {
                    UserId = user.Id,
                    BookId = book.Id
                });

            // Assert
            var databaseBook = await context.Books.FindAsync(book.Id);

            Assert.False(databaseBook!.IsAvailable);
        }

        [Fact]
        public async Task BorrowAsync_ShouldThrow_WhenBookDoesNotExist()
        {
            // Arrange
            using var context = CreateDbContext();

            var service = new LoanService(context);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.BorrowAsync(
                    new BorrowBookDto
                    {
                        UserId = Guid.NewGuid(),
                        BookId = Guid.NewGuid()
                    }));
        }

        [Fact]
        public async Task BorrowAsync_ShouldThrow_WhenBookIsUnavailable()
        {
            // Arrange
            using var context = CreateDbContext();

            var userService = new UserService(context);
            var bookService = new BookService(context);
            var loanService = new LoanService(context);

            var user1 = await userService.RegisterAsync(
                new CreateUserDto
                {
                    Name = "John",
                    Email = "john@example.com"
                });

            var user2 = await userService.RegisterAsync(
                new CreateUserDto
                {
                    Name = "Jane",
                    Email = "jane@example.com"
                });

            var book = await bookService.CreateAsync(
                new CreateBookDto
                {
                    Title = "1984",
                    Author = "George Orwell",
                    ISBN = "123",
                    PublishedYear = 1949
                });

            await loanService.BorrowAsync(
                new BorrowBookDto
                {
                    UserId = user1.Id,
                    BookId = book.Id
                });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => loanService.BorrowAsync(
                    new BorrowBookDto
                    {
                        UserId = user2.Id,
                        BookId = book.Id
                    }));
        }

        [Fact]
        public async Task ReturnAsync_ShouldMakeBookAvailable()
        {
            // Arrange
            using var context = CreateDbContext();

            var userService = new UserService(context);
            var bookService = new BookService(context);
            var loanService = new LoanService(context);

            var user = await userService.RegisterAsync(
                new CreateUserDto
                {
                    Name = "John",
                    Email = "john@example.com"
                });

            var book = await bookService.CreateAsync(
                new CreateBookDto
                {
                    Title = "1984",
                    Author = "George Orwell",
                    ISBN = "123",
                    PublishedYear = 1949
                });

            var loan = await loanService.BorrowAsync(
                new BorrowBookDto
                {
                    UserId = user.Id,
                    BookId = book.Id
                });

            // Act
            var result = await loanService.ReturnAsync(loan.Id);

            // Assert
            Assert.True(result);

            var databaseBook = await context.Books.FindAsync(book.Id);

            Assert.True(databaseBook!.IsAvailable);
        }

        [Fact]
        public async Task ReturnAsync_ShouldSetReturnDate()
        {
            // Arrange
            using var context = CreateDbContext();

            var userService = new UserService(context);
            var bookService = new BookService(context);
            var loanService = new LoanService(context);

            var user = await userService.RegisterAsync(
                new CreateUserDto
                {
                    Name = "John",
                    Email = "john@example.com"
                });

            var book = await bookService.CreateAsync(
                new CreateBookDto
                {
                    Title = "1984",
                    Author = "George Orwell",
                    ISBN = "123",
                    PublishedYear = 1949
                });

            var loan = await loanService.BorrowAsync(
                new BorrowBookDto
                {
                    UserId = user.Id,
                    BookId = book.Id
                });

            // Act
            await loanService.ReturnAsync(loan.Id);

            // Assert
            var databaseLoan = await context.Loans.FindAsync(loan.Id);

            Assert.NotNull(databaseLoan);
            Assert.NotNull(databaseLoan.ReturnDate);
        }

        [Fact]
        public async Task ReturnAsync_ShouldReturnFalse_WhenLoanDoesNotExist()
        {
            // Arrange
            using var context = CreateDbContext();

            var service = new LoanService(context);

            // Act
            var result = await service.ReturnAsync(Guid.NewGuid());

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetActiveLoansAsync_ShouldReturnOnlyActiveLoans()
        {
            // Arrange
            using var context = CreateDbContext();

            var userService = new UserService(context);
            var bookService = new BookService(context);
            var loanService = new LoanService(context);

            var user = await userService.RegisterAsync(
                new CreateUserDto
                {
                    Name = "John",
                    Email = "john@example.com"
                });

            var book1 = await bookService.CreateAsync(
                new CreateBookDto
                {
                    Title = "Book 1",
                    Author = "Author 1",
                    ISBN = "111",
                    PublishedYear = 2020
                });

            var book2 = await bookService.CreateAsync(
                new CreateBookDto
                {
                    Title = "Book 2",
                    Author = "Author 2",
                    ISBN = "222",
                    PublishedYear = 2021
                });

            var loan1 = await loanService.BorrowAsync(
                new BorrowBookDto
                {
                    UserId = user.Id,
                    BookId = book1.Id
                });

            await loanService.BorrowAsync(
                new BorrowBookDto
                {
                    UserId = user.Id,
                    BookId = book2.Id
                });

            await loanService.ReturnAsync(loan1.Id);

            // Act
            var activeLoans = await loanService.GetAllAsync();

            // Assert
            Assert.Single(activeLoans);
            Assert.Equal(book2.Id, activeLoans[0].BookId);
        }
    }
}
