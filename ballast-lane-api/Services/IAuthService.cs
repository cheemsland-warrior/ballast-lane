public interface IAuthService
{
    Task<AuthResponse> AuthenticateAsync(string email, string password);
}

public class AuthResponse
{
    public string Token { get; set; }
    public DateTime Expires { get; set; }
}