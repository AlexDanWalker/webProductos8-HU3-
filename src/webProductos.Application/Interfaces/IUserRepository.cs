using webProductos.Domain.Entities;

namespace webProductos.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task<User> AddAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(User user);
        Task<User?> GetByUsernameAsync(string username); // útil para login
        Task SaveChangesAsync();
    }
}