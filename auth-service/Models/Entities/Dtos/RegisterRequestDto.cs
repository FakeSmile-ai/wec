using System.ComponentModel.DataAnnotations; // Agrega este using

namespace AuthService.Models.Dtos  // Asegúrate de que el namespace sea "Dtos"
{
    public class RegisterRequestDto
    {
        [Required] 
        [MaxLength(64)] 
        public string Username { get; set; } = string.Empty;

        [Required] 
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "El ID del rol es inválido.")]
        public int RoleId { get; set; }
    }
}
