using AutoMapper;
using webProductos.Application.DTOs.Product;
using webProductos.Domain.Entities;

namespace webProductos.Application.Profiles
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<CreateProductDto, Product>();
        }
    }
}