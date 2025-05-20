using ExpenseTrackerApi.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExpenseTrackerApi.Services.JWTAuthentication
{
    public class TokenManager : ITokenManager
    {
        private readonly JwtSettings _jwtSettings;

        public TokenManager(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
        }
        public List<Claim> GenerateClaims(Users user, string roleName)
        {
            List<Claim> userClaim = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserId.ToString()),
                new Claim("Name", user.Username),
                new Claim("Email", user.Email),
                new Claim(ClaimTypes.Role, roleName)
            };

            return userClaim;
        }

        public string GenerateToken(List<Claim> claims)
        {
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenGeneraed = tokenHandler.WriteToken(token);
            return tokenGeneraed;
        }
    }
}
