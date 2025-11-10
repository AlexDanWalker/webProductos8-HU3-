using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webProductos.Application.DTOs.User;
using webProductos.Application.Interfaces;

namespace webProductos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET /api/users → solo Admin
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        // GET /api/users/{id} → Admin, Vendedor, User
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Seller,User")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // PUT /api/users/{id} → Admin
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, UserDto userDto)
        {
            if (id != userDto.Id) return BadRequest();
            var updated = await _userService.UpdateAsync(userDto);
            return Ok(updated);
        }

        // DELETE /api/users/{id} → Admin
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _userService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
