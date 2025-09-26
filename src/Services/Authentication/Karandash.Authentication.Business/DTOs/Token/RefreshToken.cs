namespace Karandash.Authentication.Business.DTOs.Token;

public class RefreshToken
{
    public string TokenValue { get; set; }
    public DateTime AddedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}