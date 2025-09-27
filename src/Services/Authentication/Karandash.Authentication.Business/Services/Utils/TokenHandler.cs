using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Karandash.Authentication.Business.DTOs.Auth;
using Karandash.Authentication.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Karandash.Authentication.Business.Services.Utils;

public class TokenHandler(IConfiguration configuration, PasswordHasher passwordHasher)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly PasswordHasher _passwordHasher = passwordHasher;

    public string GenerateAccessToken(User user, int expireMinutes = 60)
    {
        List<Claim> claims =
        [
            new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
            new Claim(ClaimTypes.Sid, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.UserRole.ToString())
        ];

        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(_configuration["Jwt:SigningKey"]!));
        SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);
        JwtSecurityToken jwtSecurity = new(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(expireMinutes),
            credentials);

        JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();
        string token = jwtHandler.WriteToken(jwtSecurity);
        return token;
    }

    public string GeneratePasswordResetToken(User user, DateTime expiresDate)
    {
        List<Claim> claims =
        [
            new Claim(ClaimTypes.Sid, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        ];

        SymmetricSecurityKey securityKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SigningKey"]!));
        SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            DateTime.UtcNow,
            expires: expiresDate,
            credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(string accessToken, int minutes)
    {
        return new RefreshToken()
        {
            TokenValue = _passwordHasher.Hash(accessToken, _passwordHasher.GenerateSalt()),
            AddedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(minutes)
        };
    }
}