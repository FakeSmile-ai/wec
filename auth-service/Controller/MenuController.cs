using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AuthService.Models.Dtos;
using AuthService.Services.Interfaces;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize] // requiere token por defecto
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _service;

        public MenuController(IMenuService service)
        {
            _service = service;
        }

        // Todos los menús (solo Admin)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var menus = await _service.GetAllAsync();
            return Ok(menus);
        }

        // Menús por rol (solo Admin)
        [HttpGet("role/{roleId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByRoleId(int roleId)
        {
            var menus = await _service.GetByRoleIdAsync(roleId);
            return Ok(menus);
        }

        // Asignar menús a rol (solo Admin)
        [HttpPost("role/{roleId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignToRole(int roleId, [FromBody] AssignMenusDto dto)
        {
            await _service.AssignToRoleAsync(roleId, dto.MenuIds);
            return NoContent();
        }

        // Mis menús (según el token)
        [HttpGet("mine")]
        public async Task<IActionResult> MyMenus()
        {
            var idClaim = User.FindFirstValue("Id");
            if (string.IsNullOrWhiteSpace(idClaim))
                return Unauthorized();

            if (!int.TryParse(idClaim, out var userId))
                return Unauthorized("Claim Id inválida en el token");

            var menus = await _service.GetMyMenusAsync(userId);
            return Ok(menus);
        }
    }
}
