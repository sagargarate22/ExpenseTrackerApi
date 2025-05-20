using ExpenseTrackerApi.Entities;
using System.Security.Claims;

namespace ExpenseTrackerApi.Services.JWTAuthentication
{
    public interface ITokenManager
    { 
        public string GenerateToken(List<Claim> claims);

        public List<Claim> GenerateClaims(Users user, string roleName);
    }
}
