namespace ECommerceApi.DTOs.Auth;

public class GoogleLoginRequest
{
    public string IdToken { get; set; } = null!;   // ID token do Google Identity Services trả về ở FE
}
