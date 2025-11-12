using webProductos.Application.DTOs.User;
using webProductos.Application.DTOs.Token;
using webProductos.Application.Interfaces;
using webProductos.Domain.Entities;
using AutoMapper;
using System.Security.Cryptography;

namespace webProductos.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> RegisterAsync(RegisterUserDto registerDto)
        {
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                RoleId = 3
            };

            var createdUser = await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
            return _mapper.Map<UserDto>(createdUser);
        }

        public async Task<AuthResponseDto?> AuthenticateAsync(LoginUserDto loginDto)
        {
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);
            if (user == null) return null;

            bool isValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
            return isValid ? _mapper.Map<AuthResponseDto>(user) : null;
        }

        public async Task<UserDto> UpdateAsync(UserDto userDto)
        {
            var user = await _userRepository.GetByIdAsync(userDto.Id);
            if (user == null) throw new Exception("Usuario no encontrado");

            user.Username = userDto.Username;
            user.Email = userDto.Email;

            var updatedUser = await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
            return _mapper.Map<UserDto>(updatedUser);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _userRepository.DeleteAsync(id);
            await _userRepository.SaveChangesAsync();
            return result;
        }

        public async Task<string> GenerateRefreshTokenAsync(int userId, string ipAddress)
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var refreshToken = Convert.ToBase64String(randomBytes);

            var tokenEntity = new RefreshToken
            {
                Token = refreshToken,
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                UserId = userId
            };

            await _refreshTokenRepository.AddAsync(tokenEntity);
            await _refreshTokenRepository.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<User?> GetUserByRefreshTokenAsync(string token)
        {
            var rt = await _refreshTokenRepository.GetByTokenAsync(token);
            if (rt == null || rt.User == null || !rt.IsActive) return null;
            return rt.User;
        }

        public async Task RevokeRefreshTokenAsync(string token, string ipAddress)
        {
            var rt = await _refreshTokenRepository.GetByTokenAsync(token);
            if (rt == null) return;

            rt.Revoked = DateTime.UtcNow;
            rt.RevokedByIp = ipAddress;
            await _refreshTokenRepository.SaveChangesAsync();
        }
    }
}
