namespace webProductos.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        // Relación con Role (1:1)
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        // Relación con Product (1:N)
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}