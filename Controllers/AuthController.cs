using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; // <-- FALTABA ESTE
using SamuBarber.Api.Data;

namespace SamuBarber.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            // Busca en la tabla USUARIO mediante Entity Framework
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (usuario == null)
            {
                return Unauthorized(new { mensaje = "El correo electrónico no está registrado." });
            }

            // Verificación sencilla o con BCrypt según lo tengas guardado
            bool passwordValida = BCrypt.Net.BCrypt.Verify(model.Password, usuario.Password) 
                                 || usuario.Password == model.Password;

            if (!passwordValida)
            {
                return Unauthorized(new { mensaje = "Contraseña incorrecta." });
            }

            // Generar Token JWT con el Rol asignado en BD
            var token = GenerarTokenJwt(usuario.Email, usuario.Nombre, usuario.Rol, usuario.IdUsuario);

            return Ok(new
            {
                token = token,
                idUsuario = usuario.IdUsuario,
                nombre = usuario.Nombre,
                rol = usuario.Rol,
                email = usuario.Email
            });
        }

        private string GenerarTokenJwt(string email, string nombre, string rol, int idUsuario)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, idUsuario.ToString()),
                new Claim(ClaimTypes.Name, nombre),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, rol) // 'Administrador' o 'Barbero'
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(double.Parse(_config["Jwt:ExpireHours"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}