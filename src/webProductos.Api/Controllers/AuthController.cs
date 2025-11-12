using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using webProductos.Application.DTOs.User;
using webProductos.Application.DTOs.Token;
using webProductos.Application.Interfaces;
using webProductos.Infrastructure.Data;
using webProductos.Infrastructure.Repositories;

namespace webProductos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(
            IUserService userService,
            IRefreshTokenRepository refreshTokenRepository,
            AppDbContext context,
            IConfiguration configuration)
        {
            _userService = userService;
            _refreshTokenRepository = refreshTokenRepository;
            _context = context;
            _configuration = configuration;
        }

        // POST: /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
        {
            var user = await _userService.RegisterAsync(registerDto);
            return CreatedAtAction(null, user);
        }

        // POST: /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto loginDto)
        {
            var authResponse = await _userService.AuthenticateAsync(loginDto);
            if (authResponse == null)
                return Unauthorized("Usuario o contraseña incorrecta.");

            // Generar access token (JWT)
            var jwtToken = GenerateJwtToken(authResponse);

            // Generar refresh token
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var refreshToken = await _userService.GenerateRefreshTokenAsync(authResponse.Id, ipAddress);

            // Asignar el token al response
            authResponse.Token = jwtToken;

            return Ok(new
            {
                Token = jwtToken,
                RefreshToken = refreshToken,
                User = authResponse
            });
        }

        // POST: /api/auth/refresh
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest("El token de actualización es requerido.");

            var user = await _userService.GetUserByRefreshTokenAsync(request.RefreshToken);
            if (user == null)
                return Unauthorized("Token de actualización inválido o expirado.");

            var authResponse = new AuthResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                RoleName = user.Role.Name
            };

            var newJwtToken = GenerateJwtToken(authResponse);

            return Ok(new
            {
                Token = newJwtToken,
                RefreshToken = request.RefreshToken
            });
        }

        // POST: /api/auth/revoke
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RevokeRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest("El token de actualización es requerido.");

            var existing = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
            if (existing == null)
                return NotFound("Token no encontrado o ya revocado.");

            _context.RefreshTokens.Remove(existing);
            await _refreshTokenRepository.SaveChangesAsync();

            return Ok(new { message = "Token revocado correctamente." });
        }

        // 🔐 Método auxiliar para crear JWT
        private string GenerateJwtToken(AuthResponseDto authResponse)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, authResponse.Id.ToString()),
                new Claim(ClaimTypes.Name, authResponse.Username),
                new Claim(ClaimTypes.Role, authResponse.RoleName)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
