using AutoMapper;
using webProductos.Application.DTOs.User;
using webProductos.Domain.Entities;

namespace webProductos.Application.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));
            CreateMap<RegisterUserDto, User>();
        }
    }
}