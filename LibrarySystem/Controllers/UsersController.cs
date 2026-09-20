using LibrarySystem.API.DTOs.Users;
using LibrarySystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibrarySystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService) {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Register(CreateUserDto createUserDto) 
        {
            try 
            {
                var user = await _userService.RegisterAsync(createUserDto);

                return CreatedAtAction(
                    nameof(GetAll), 
                    new { id = user.Id }, 
                    user
                );

            } catch (InvalidOperationException ex) {
                return Conflict(ex.Message);
            }
        }

        [HttpGet]
        public async  Task<IActionResult> GetAll() 
        {
            var users = await _userService.GetAllAsync();

            return Ok(users);
        }
    }
}
