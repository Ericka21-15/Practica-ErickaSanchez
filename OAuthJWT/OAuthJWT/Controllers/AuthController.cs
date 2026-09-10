using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OAuthJWT.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OAuthJWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("Login")]
        public IActionResult Login(LoginRequest loginRequest)
        {
            var adminUser = _configuration["Auth:AdminUser"] ?? "admin";
            var adminPassword = _configuration["Auth:AdminPassword"] ?? "Admin1234!";
            var userName = _configuration["Auth:UserName"] ?? "usuario";
            var userPassword = _configuration["Auth:UserPassword"] ?? "Usuario1234!";

            string rol;

            if (loginRequest.Usuario == adminUser && loginRequest.Password == adminPassword)
            {
                rol = "Administrador";
            }
            else if (loginRequest.Usuario == userName && loginRequest.Password == userPassword)
            {
                rol = "Usuario";
            }
            else
            {
                return Unauthorized("Usuario o contraseña incorrectos");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, loginRequest.Usuario),
                new Claim(ClaimTypes.Role, rol)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
                signingCredentials: credenciales);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                usuario = loginRequest.Usuario,
                rol,
                expiresMinutes = _configuration["Jwt:ExpireMinutes"]
            });
        }
    }
}
