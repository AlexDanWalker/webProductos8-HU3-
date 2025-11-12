using webProductos.Domain.Entities;

namespace webProductos.Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task AddAsync(RefreshToken refreshToken);
        Task SaveChangesAsync();
    }
}