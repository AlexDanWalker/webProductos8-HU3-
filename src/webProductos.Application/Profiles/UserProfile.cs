using AutoMapper;
using webProductos.Application.DTOs.User;
using webProductos.Domain.Entities;

namespace webProductos.Application.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Mapear User -> UserDto sin RoleName ni CreatedAt
            CreateMap<User, UserDto>();
            CreateMap<User, AuthResponseDto>();

            // Mapear RegisterUserDto -> User
            CreateMap<RegisterUserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => BCrypt.Net.BCrypt.HashPassword(src.Password)))
                .ForMember(dest => dest.RoleId, opt => opt.Ignore()); // RoleId se asigna en el servicio
        }
    }
}