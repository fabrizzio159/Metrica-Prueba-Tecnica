using AuthService.Models.Entities;

namespace AuthService.Interfaces;

public interface ITokenService
{
    string GenerateJwtToken(User user);
    string GenerateRefreshToken();
}
