using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;

namespace PetStoreAPI.Tests.Api;

/// <summary>Immutable snapshot of an HTTP exchange; its <see cref="ToString"/> is used in failure messages.</summary>
public sealed class ApiResponse
{
    public ApiResponse(HttpMethod method, Uri requestUri, string? requestBody, HttpStatusCode statusCode, string? mediaType, string body)
    {
        Method = method;
        RequestUri = requestUri;
        RequestBody = requestBody;
        StatusCode = statusCode;
        MediaType = mediaType;
        Body = body;
    }

    public HttpMethod Method { get; }
    public Uri RequestUri { get; }
    public string? RequestBody { get; }
    public HttpStatusCode StatusCode { get; }
    public string? MediaType { get; }
    public string Body { get; }

    public T ReadAs<T>()
    {
        try
        {
            return JsonSerializer.Deserialize<T>(Body, PetStoreClient.JsonOptions)
                ?? throw new JsonException("Response body is empty or 'null'.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Cannot deserialize response as {typeof(T).Name}: {ex.Message}\n{this}", ex);
        }
    }

    public bool TryReadAs<T>([NotNullWhen(true)] out T? value)
    {
        try
        {
            value = JsonSerializer.Deserialize<T>(Body, PetStoreClient.JsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = default;
            return false;
        }
    }

    /// <summary>Guards Given steps: a failed setup must stop the scenario with a clear reason, not a misleading assertion later.</summary>
    public ApiResponse EnsureStatus(HttpStatusCode expected, string action)
    {
        if (StatusCode != expected)
            throw new InvalidOperationException(
                $"Precondition failed: could not {action}. Expected {(int)expected} {expected}, got {(int)StatusCode} {StatusCode}.\n{this}");

        return this;
    }

    public override string ToString() =>
        $"""
        Request:  {Method} {RequestUri}
        {(RequestBody is null ? string.Empty : $"Payload:  {RequestBody}\n")}Response: {(int)StatusCode} {StatusCode} ({MediaType ?? "no content type"})
        Body:     {(Body.Length == 0 ? "<empty>" : Body)}
        """;
}
