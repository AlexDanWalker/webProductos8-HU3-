using webProductos.Application.DTOs.Product;

namespace webProductos.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetByIdAsync(int id);
        Task<ProductDto> CreateAsync(ProductDto productDto, int userId);
        Task<ProductDto> UpdateAsync(ProductDto productDto);
        Task<bool> DeleteAsync(int id);
    }
}