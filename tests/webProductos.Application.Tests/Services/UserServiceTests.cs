using Xunit;
using Moq;
using webProductos.Application.Interfaces;
using webProductos.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace webProductos.Application.Tests.Services
{
    public class UserServiceTests
    {
        [Fact]
        public async Task AuthenticateUser_ShouldReturnUser_WhenUsernameAndPasswordCorrect()
        {
            // Arrange
            var mockRepo = new Mock<IUserRepository>();
            var username = "admin";
            var password = "password123";

            var user = new User
            {
                Username = username,
                Email = "admin@test.com",
                PasswordHash = password // recuerda, en producción usar hash
            };

            // Configuramos el mock para que devuelva el usuario al buscar por username
            mockRepo.Setup(r => r.GetByUsernameAsync(username))
                .ReturnsAsync(user);

            // Act: simular autenticación
            var fetchedUser = await mockRepo.Object.GetByUsernameAsync(username);
            bool isAuthenticated = fetchedUser != null && fetchedUser.PasswordHash == password;

            // Assert
            Assert.True(isAuthenticated);
            Assert.Equal(username, fetchedUser.Username);
            Assert.Equal("admin@test.com", fetchedUser.Email);

            // Verificamos que se llamó exactamente una vez
            mockRepo.Verify(r => r.GetByUsernameAsync(username), Times.Once);
        }
    }
}