using LibrarySystem.API.Data;
using LibrarySystem.API.DTOs.Users;
using LibrarySystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.API.Services
{
    public class UserService
    {
        private readonly LibraryDbContext _libraryDbContext;

        public UserService(LibraryDbContext libraryDbContext)
        {
            _libraryDbContext = libraryDbContext;
        }

        public async Task<UserResponseDto> RegisterAsync(CreateUserDto createUserDto)
        {
            var existingUser = await _libraryDbContext.Users
                .FirstOrDefaultAsync(u => u.Email == createUserDto.Email);

            if (existingUser != null)
            {
                throw new InvalidOperationException("A user with the same email already exists.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = createUserDto.Name,
                Email = createUserDto.Email,
                RegisteredDate = DateTime.UtcNow
            };

            _libraryDbContext.Users.Add(user);

            await _libraryDbContext.SaveChangesAsync();

            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                RegisteredDate = user.RegisteredDate
            };
        }

        public async Task<List<UserResponseDto>> GetAllAsync()
        {
            return await _libraryDbContext.Users
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    RegisteredDate = u.RegisteredDate
                })
                .ToListAsync();
        }
    }
}
