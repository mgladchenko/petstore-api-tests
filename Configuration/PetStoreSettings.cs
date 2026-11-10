namespace PetStoreAPI.Tests.Configuration;

public sealed class PetStoreSettings
{
    public const string SectionName = "PetStore";

    public Uri BaseUrl { get; init; } = null!;
    public string ApiKey { get; init; } = string.Empty;
    public int RequestTimeoutSeconds { get; init; } = 30;

    public void Validate()
    {
        if (BaseUrl is null || !BaseUrl.IsAbsoluteUri)
            throw new InvalidOperationException($"'{SectionName}:BaseUrl' must be an absolute URL.");

        // HttpClient drops the last path segment of BaseAddress (e.g. "/v2") without a trailing slash.
        if (!BaseUrl.AbsoluteUri.EndsWith('/'))
            throw new InvalidOperationException($"'{SectionName}:BaseUrl' must end with '/'. Actual: '{BaseUrl}'.");

        if (RequestTimeoutSeconds <= 0)
            throw new InvalidOperationException($"'{SectionName}:RequestTimeoutSeconds' must be positive.");
    }
}
