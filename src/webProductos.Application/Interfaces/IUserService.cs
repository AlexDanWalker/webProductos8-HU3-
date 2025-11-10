using webProductos.Application.DTOs.User;

namespace webProductos.Application.Interfaces
{
    public interface IUserService
    {
        // Obtener todos los usuarios
        Task<IEnumerable<UserDto>> GetAllAsync();

        // Obtener un usuario por Id
        Task<UserDto?> GetByIdAsync(int id);

        // Crear un nuevo usuario
        Task<UserDto> CreateAsync(UserDto userDto);

        // Actualizar un usuario existente
        Task<UserDto> UpdateAsync(UserDto userDto);

        // Eliminar un usuario por Id
        Task<bool> DeleteAsync(int id);

        // Opcional: autenticar usuario (si planeas login)
        Task<UserDto?> AuthenticateAsync(string username, string password);
    }
}