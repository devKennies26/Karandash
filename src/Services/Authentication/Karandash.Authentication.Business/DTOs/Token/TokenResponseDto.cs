namespace Karandash.Authentication.Business.DTOs.Token;

public class TokenResponseDto
{
    public string UserId { get; set; }

    public string FullName { get; set; }
    public byte RoleId { get; set; }

    public string AccessToken { get; set; }
    public DateTime ExpiresDate { get; set; }
    public string RefreshToken { get; set; }
}