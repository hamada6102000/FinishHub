namespace test.Services;

public class ExternalAuthPayload
{
    public string Subject { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
}

public interface IFirebaseAuthValidator
{
    Task<ExternalAuthPayload> ValidateAsync(string idToken);
}
