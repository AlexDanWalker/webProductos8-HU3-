namespace webProductos.Application.DTOs.User
{
    public class RegisterUserDto
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? RoleName { get; set; } = "Customer";
    }
}