using webProductos.Application.DTOs.User;
using webProductos.Application.DTOs.Token;
using webProductos.Domain.Entities;

namespace webProductos.Application.Interfaces
{
    public interface IUserService
    {
        // Leer usuarios
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(int id);

        // Crear usuario
        Task<UserDto> RegisterAsync(RegisterUserDto registerDto);

        // Actualizar usuario
        Task<UserDto> UpdateAsync(UserDto userDto);

        // Eliminar usuario
        Task<bool> DeleteAsync(int id);

        // Login
        Task<AuthResponseDto?> AuthenticateAsync(LoginUserDto loginDto);

        // 🔐 Refresh Token
        Task<string> GenerateRefreshTokenAsync(int userId, string ipAddress);
        Task<User?> GetUserByRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token, string ipAddress);
    }
}