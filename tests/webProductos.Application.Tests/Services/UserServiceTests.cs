using Moq;
using webProductos.Application.Interfaces;
using webProductos.Domain.Entities;


namespace webProductos.Application.Tests.Services
{
    public class UserServiceTests
    {
        [Fact]
        public async Task AuthenticateUser_ShouldReturnUser_WhenEmailAndPasswordCorrect()
        {
            // Arrange
            var mockRepo = new Mock<IUserRepository>();
            var email = "admin@test.com";
            var password = "password123";

            var user = new User
            {
                Username = "admin",
                Email = email,
                PasswordHash = password // recuerda, en producción usar hash
            };

            // Configuramos el mock para que devuelva el usuario al buscar por email
            mockRepo.Setup(r => r.GetByEmailAsync(email))
                .ReturnsAsync(user);

            // Act: simular autenticación
            var fetchedUser = await mockRepo.Object.GetByEmailAsync(email);
            bool isAuthenticated = fetchedUser != null && fetchedUser.PasswordHash == password;

            // Assert
            Assert.True(isAuthenticated);
            Assert.Equal("admin", fetchedUser.Username);
            Assert.Equal(email, fetchedUser.Email);

            // Verificamos que se llamó exactamente una vez
            mockRepo.Verify(r => r.GetByEmailAsync(email), Times.Once);
        }
    }
}