using Moq;
using webProductos.Application.Interfaces;
using webProductos.Application.DTOs.Product;


public class ProductServiceTests
{
    [Fact]
    public async Task CreateProduct_CallsRepositoryAdd()
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var productDto = new ProductDto { Name = "Test", Price = 100 };

        // Si tu IProductService tuviera un método CreateAsync:
        // var service = new ProductService(mockRepo.Object);

        // Act
        // await service.CreateAsync(productDto);

        // Assert
        // mockRepo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
    }
}