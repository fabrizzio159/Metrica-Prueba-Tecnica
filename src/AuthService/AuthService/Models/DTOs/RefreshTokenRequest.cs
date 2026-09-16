using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.DTOs;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
