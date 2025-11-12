using webProductos.Application.Interfaces;
using webProductos.Domain.Entities;
using webProductos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace webProductos.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;

        public RefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(r => r.User)
                .ThenInclude(u => u.Role)
                .SingleOrDefaultAsync(r => r.Token == token);
        }
        
        public async Task AddAsync(RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}