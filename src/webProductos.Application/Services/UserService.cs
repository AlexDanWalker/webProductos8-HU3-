using webProductos.Application.DTOs.User;
using webProductos.Application.Interfaces;
using webProductos.Domain.Entities;
using AutoMapper;

namespace webProductos.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
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
                RoleId = 3 // default role User
            };

            var createdUser = await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
            return _mapper.Map<UserDto>(createdUser);
        }

        public async Task<AuthResponseDto?> AuthenticateAsync(LoginUserDto loginDto)
        {
            // CAMBIO: usamos GetByEmailAsync
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
    }
}
