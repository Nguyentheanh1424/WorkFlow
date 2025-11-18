using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WorkFlow.Application.Common.Interfaces.Services;
using WorkFlow.Domain.Enums;

namespace WorkFlow.Infrastructure.Services
{
    public class JwtTokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public JwtTokenService(
            IConfiguration config)
        {
            _config = config;
        }

        public TokenInfoDto ParseAccessToken(string accessToken)
        {
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(accessToken))
                throw new Exception("Token không hợp lệ");

            var token = handler.ReadJwtToken(accessToken);

            var provider = token.Claims.FirstOrDefault(c => c.Type == "provider")?.Value
                           ?? "unknown";

            var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            if (userIdClaim == null)
                throw new Exception("Token không chứa userId");

            var userId = Guid.Parse(userIdClaim);

            var isExpired = token.ValidTo < DateTime.UtcNow;

            return new TokenInfoDto
            {
                UserId = userId,
                Provider = provider,
                IsExpired = isExpired
            };
        }

        public Task<(string accessToken, string refreshToken)> IssueAsync(Guid userId, AccountProvider provider)
        {
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var signingKey = _config["Jwt:SigningKey"];
            var accessMinutes = int.TryParse(_config["Jwt:AccessTokenMinutes"], out int value) ? value : 30;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim("provider", provider.ToString().ToLowerInvariant()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(accessMinutes),
                signingCredentials: creds
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

            var refreshToken = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64)
            );

            return Task.FromResult((accessToken, refreshToken));
        }
    }
}
