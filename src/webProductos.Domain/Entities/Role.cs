namespace webProductos.Domain.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Relación con User (1:N)
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}