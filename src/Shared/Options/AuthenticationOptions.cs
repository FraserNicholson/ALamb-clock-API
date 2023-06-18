namespace Shared.Options;

public class AuthenticationOptions
{
    public string ApiKeyHeaderName { get; set; } = default!;
    public string ApiKeyValue { get; set; } = default!;
}