using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace HospitalOrders.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Genera un token JWT para autenticarse en la API.
    /// </summary>
    /// <remarks>
    /// Credenciales de prueba: usuario = **admin**, contraseña = **Admin123!**
    /// </remarks>
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetToken([FromBody] LoginRequest request)
    {
        if (!IsValidCredentials(request.Username, request.Password))
        {
            _logger.LogWarning("Intento de login fallido para usuario: {Username}", request.Username);
            return Unauthorized(new { error = "Credenciales inválidas." });
        }

        var token = GenerateJwtToken(request.Username);
        _logger.LogInformation("Token generado para usuario: {Username}", request.Username);

        return Ok(new TokenResponse { Token = token, ExpiresIn = 3600 });
    }

    private bool IsValidCredentials(string username, string password)
    {
        var validUser = _configuration["Jwt:TestUser"];
        var validPassword = _configuration["Jwt:TestPassword"];
        return username == validUser && password == validPassword;
    }

    private string GenerateJwtToken(string username)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "ApiUser"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record LoginRequest(string Username, string Password);
public record TokenResponse
{
    public string Token { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}
