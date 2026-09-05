using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LiveNow.CRM.API.Services;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace LiveNow.CRM.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ApiControllerBase
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;

    public AuthController(IUserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request, CancellationToken cancellationToken)
    {
        User? user = await _userService.ValidateCredentialsAsync(request.UserOrEmail, request.Password, cancellationToken);
        if (user is null)
        {
            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        var settings = JwtSettings.FromConfiguration(_configuration, required: true);
        DateTime expiresAt = DateTime.UtcNow.AddMinutes(settings.ExpirationMinutes);
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, user.Role)
        ];
        SigningCredentials credentials = new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)), SecurityAlgorithms.HmacSha256);
        string token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(settings.Issuer, settings.Audience, claims, expires: expiresAt, signingCredentials: credentials));

        return Ok(new LoginResponseDto { AccessToken = token, ExpiresAtUtc = expiresAt, UserId = user.Id, Name = user.Name, Username = user.Username, Role = user.Role });
    }
}

public sealed record JwtSettings(string Key, string Issuer, string Audience, int ExpirationMinutes)
{
    public static JwtSettings FromConfiguration(IConfiguration configuration, bool required)
    {
        string? key = configuration["Jwt:Key"];
        string? issuer = configuration["Jwt:Issuer"];
        string? audience = configuration["Jwt:Audience"];
        string? expiration = configuration["Jwt:ExpirationMinutes"];
        if (required && (string.IsNullOrWhiteSpace(key) || key.Length < 32 || string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience) || !int.TryParse(expiration, out int minutes) || minutes <= 0))
        {
            throw new InvalidOperationException("JWT configuration is incomplete. Set Jwt__Key, Jwt__Issuer, Jwt__Audience and Jwt__ExpirationMinutes.");
        }
        return new JwtSettings(key ?? string.Empty, issuer ?? string.Empty, audience ?? string.Empty, int.TryParse(expiration, out int value) && value > 0 ? value : 60);
    }
}
