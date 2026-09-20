using LibrarySystem.API.Data;
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
    public class UserServiceTests
    {
        private LibraryDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<LibraryDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new LibraryDbContext(options);
        }

        [Fact]
        public async Task RegisterAsync_ShouldCreateUser()
        {
            // Arrange
            using var context = CreateDbContext();
            var service = new UserService(context);

            var dto = new CreateUserDto
            {
                Name = "John Doe",
                Email = "john@example.com"
            };

            // Act
            var result = await service.RegisterAsync(dto);

            // Assert
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal("John Doe", result.Name);
            Assert.Equal("john@example.com", result.Email);
            Assert.NotEqual(default, result.RegisteredDate);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrow_WhenEmailAlreadyExists()
        {
            // Arrange
            using var context = CreateDbContext();
            var service = new UserService(context);

            var dto = new CreateUserDto
            {
                Name = "John Doe",
                Email = "john@example.com"
            };

            await service.RegisterAsync(dto);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RegisterAsync(dto));
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllUsers()
        {
            // Arrange
            using var context = CreateDbContext();
            var service = new UserService(context);

            await service.RegisterAsync(
                new CreateUserDto
                {
                    Name = "John",
                    Email = "john@example.com"
                });

            await service.RegisterAsync(
                new CreateUserDto
                {
                    Name = "Jane",
                    Email = "jane@example.com"
                });

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count);
        }
    }
}
