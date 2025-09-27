namespace Karandash.Authentication.Business.DTOs.Auth;

public class RefreshToken
{
    public string TokenValue { get; set; }
    public DateTime AddedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}