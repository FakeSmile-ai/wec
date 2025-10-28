namespace AuthService.Models.Dtos
{
    public class RoleUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public int UpdatedBy { get; set; }
    }
}
