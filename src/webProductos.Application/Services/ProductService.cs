using webProductos.Application.DTOs.Product;
using webProductos.Application.Interfaces;
using webProductos.Domain.Entities;
using AutoMapper;

namespace webProductos.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product == null ? null : _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> CreateAsync(ProductDto productDto, int userId)
        {
            var product = _mapper.Map<Product>(productDto);
            product.UserId = userId; // 🔹 Asociamos al usuario creador

            var createdProduct = await _productRepository.AddAsync(product);
            return _mapper.Map<ProductDto>(createdProduct);
        }

        public async Task<ProductDto> UpdateAsync(ProductDto productDto)
        {
            //  Primero obtenemos el producto existente para no romper la FK
            var existingProduct = await _productRepository.GetByIdAsync(productDto.Id);
            if (existingProduct == null) throw new Exception("Product not found");

            existingProduct.Name = productDto.Name;
            existingProduct.Description = productDto.Description;
            existingProduct.Price = productDto.Price;
            existingProduct.Stock = productDto.Stock;
            //  NO modificamos UserId

            var updatedProduct = await _productRepository.UpdateAsync(existingProduct);
            return _mapper.Map<ProductDto>(updatedProduct);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _productRepository.DeleteAsync(id);
        }
    }
}
