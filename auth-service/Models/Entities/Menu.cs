namespace AuthService.Models.Entities
{
    public class Menu
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

        public ICollection<RoleMenu> RoleMenus { get; set; } = new List<RoleMenu>();
    }
}
