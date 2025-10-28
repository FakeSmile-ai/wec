using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.Dtos
{
    public class RoleCreateDto
    {
        [Required] 
        [MaxLength(64)] 
        public string Name { get; set; } = string.Empty;

        public int CreatedBy { get; set; }
    }
}
