using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PetStoreAPI.Tests.Api.Models;
using PetStoreAPI.Tests.Configuration;

namespace PetStoreAPI.Tests.Api;

/// <summary>
/// Thin typed client for the /pet resource. Returns raw responses and never asserts or throws on
/// non-success status codes: deciding what is "expected" belongs to the test, not the client.
/// </summary>
public sealed class PetStoreClient(HttpClient httpClient, PetStoreSettings settings)
{
    private const string PetPath = "pet";

    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public Task<ApiResponse> CreatePetAsync(Pet pet) =>
        CreatePetAsync(JsonSerializer.Serialize(pet, JsonOptions), MediaTypeNames.Application.Json);

    public Task<ApiResponse> CreatePetAsync(string rawBody, string contentType)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, PetPath)
        {
            Content = new StringContent(rawBody, Encoding.UTF8, contentType),
        };
        return SendAsync(request, rawBody);
    }

    public Task<ApiResponse> GetPetAsync(long petId) => GetPetAsync(petId.ToString());

    public Task<ApiResponse> GetPetAsync(string petId) =>
        SendAsync(new HttpRequestMessage(HttpMethod.Get, PetUri(petId)));

    public Task<ApiResponse> DeletePetAsync(long petId) => DeletePetAsync(petId.ToString());

    public Task<ApiResponse> DeletePetAsync(string petId)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, PetUri(petId));
        request.Headers.Add("api_key", settings.ApiKey);
        return SendAsync(request);
    }

    private static string PetUri(string petId) => $"{PetPath}/{Uri.EscapeDataString(petId)}";

    private async Task<ApiResponse> SendAsync(HttpRequestMessage request, string? requestBody = null)
    {
        using (request)
        {
            request.Headers.Accept.ParseAdd(MediaTypeNames.Application.Json);

            using var response = await httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            return new ApiResponse(
                request.Method,
                response.RequestMessage?.RequestUri ?? new Uri(httpClient.BaseAddress!, request.RequestUri!),
                requestBody,
                response.StatusCode,
                response.Content.Headers.ContentType?.MediaType,
                body);
        }
    }
}
