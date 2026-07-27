using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BallastLaneApi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BallastLaneApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _users;
        private readonly IConfiguration _config;

        public AuthService(IUserService users, IConfiguration config)
        {
            _users = users;
            _config = config;
        }

        public async Task<AuthResponse> AuthenticateAsync(string email, string password)
        {
            var user = await _users.AuthenticateAsync(email, password);
            if (user == null) return null;

            var secret = _config["Jwt:Key"] ?? "please-change-this-secret-in-production";
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            return new AuthResponse { Token = jwt, Expires = tokenDescriptor.Expires ?? DateTime.UtcNow.AddHours(8) };
        }
    }
}
