using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required] public required string Username { get; set; }
        [Required] public required string Password { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int UpdatedBy { get; set; }

        // FK
        public int RoleId { get; set; }
        public Role? Role { get; set; }
    }
}