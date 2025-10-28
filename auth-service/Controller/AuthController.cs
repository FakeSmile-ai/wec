using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using AspNet.Security.OAuth.GitHub;
using System.Security.Claims;

using AuthService.Models.Dtos;
using AuthService.Services.Interfaces;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth) => _auth = auth;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var response = await _auth.AuthenticateAsync(request);
            if (response is null) return Unauthorized("Usuario o contraseña inválidos");
            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var result = await _auth.RegisterAsync(request);
            if (result is null) return BadRequest("El usuario ya existe o el rol no es válido");
            return Ok(new { message = result });
        }

        // ---- GitHub OAuth ----

        [HttpGet("github/login")]
        public IActionResult GitHubLogin()
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.ActionLink(nameof(GitHubCallback), "Auth")
            };
            return Challenge(props, GitHubAuthenticationDefaults.AuthenticationScheme);
        }

        [HttpGet("github/callback")]
        public async Task<IActionResult> GitHubCallback()
        {
            var result = await HttpContext.AuthenticateAsync("External");
            if (!result.Succeeded || result.Principal is null)
                return Unauthorized("No se pudo autenticar con GitHub.");

            var p = result.Principal;
            var githubLogin =
                p.FindFirst("urn:github:login")?.Value ??
                p.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                p.Identity?.Name ??
                "github-user";

            var email =
                p.FindFirst("urn:github:email")?.Value ??
                p.FindFirst(ClaimTypes.Email)?.Value;

            var loginResponse = await _auth.AuthenticateWithGitHubAsync(githubLogin, email);

            await HttpContext.SignOutAsync("External");

            // --- Redirect al front con token ---
            var redirect = $"{Request.Scheme}://{Request.Host}";
            var target = HttpContext.RequestServices
                .GetRequiredService<IConfiguration>()["Frontend:OAuthRedirect"]
                ?? "http://localhost:3000/oauth/callback";

            // Si quieres permitir returnUrl dinámico:
            // var returnUrl = Request.Query["returnUrl"].FirstOrDefault();
            // if (!string.IsNullOrWhiteSpace(returnUrl) && Uri.IsWellFormedUriString(returnUrl, UriKind.Absolute))
            //     target = returnUrl;

            var url = $"{target}?token={Uri.EscapeDataString(loginResponse.Token!)}";
            return Redirect(url);
        }

    }
}
