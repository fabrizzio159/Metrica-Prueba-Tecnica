using AuthService.Data;
using AuthService.Interfaces;
using AuthService.Models.DTOs;
using AuthService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services;

public class AuthenticationService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly AuthDbContext _context;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IUserRepository userRepository,
        ITokenService tokenService,
        AuthDbContext context,
        ILogger<AuthenticationService> logger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _context = context;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Intento de login fallido para {Email}", request.Email);
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        var jwtToken = _tokenService.GenerateJwtToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            FechaExpiracion = DateTime.UtcNow.AddDays(7),
            FechaCreacion = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Login exitoso para {Email}", request.Email);

        return new LoginResponse
        {
            Token = jwtToken,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddHours(2),
            Email = user.Email,
            NombreCompleto = user.NombreCompleto,
            Rol = user.Rol
        };
    }

    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken
                && !rt.Revocado
                && rt.FechaExpiracion > DateTime.UtcNow);

        if (storedToken == null)
        {
            throw new UnauthorizedAccessException("Refresh token inválido o expirado.");
        }

        storedToken.Revocado = true;

        var newJwtToken = _tokenService.GenerateJwtToken(storedToken.User);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = storedToken.UserId,
            Token = newRefreshToken,
            FechaExpiracion = DateTime.UtcNow.AddDays(7),
            FechaCreacion = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(newRefreshTokenEntity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Token refrescado para UserId {UserId}", storedToken.UserId);

        return new LoginResponse
        {
            Token = newJwtToken,
            RefreshToken = newRefreshToken,
            Expiration = DateTime.UtcNow.AddHours(2),
            Email = storedToken.User.Email,
            NombreCompleto = storedToken.User.NombreCompleto,
            Rol = storedToken.User.Rol
        };
    }
}
