namespace AuthService.Models.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime FechaExpiracion { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public bool Revocado { get; set; }
    public User User { get; set; } = null!;
}
