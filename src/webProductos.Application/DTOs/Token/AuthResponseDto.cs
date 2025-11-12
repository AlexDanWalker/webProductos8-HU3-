namespace webProductos.Application.DTOs.Token
{
    public class AuthResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string RoleName { get; set; } = default!;
        public string Token { get; set; } = default!;
    }
}