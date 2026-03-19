using Authorization.Application.Interfaces.Security.JWT;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Authorization.Infrastructure.Security.JWT
{
    public class ServiceJwtProvider : IServiceJwtProvider
    {
        private readonly IConfiguration _configuration;

        public ServiceJwtProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateServiceToken(string serviceName, string[] permissions)
        {
            var claims = new List<Claim>()
            {
                new Claim("token_type", "service"),
                new Claim("service", serviceName),
                new Claim("jti", Guid.NewGuid().ToString())
            };

            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            var jwtKey = _configuration["JwtService:Key"];

            if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
            {
                throw new InvalidOperationException("Критична помилка: JWT Key занадто короткий або відсутній!");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtService:Issuer"],
                audience: _configuration["JwtService:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
