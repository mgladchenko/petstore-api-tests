namespace PetStoreAPI.Tests.Api.Models;

// Status is a string (not an enum) so tests can send values outside the contract.
public sealed record Pet
{
    public long? Id { get; init; }
    public Category? Category { get; init; }
    public string? Name { get; init; }
    public IReadOnlyList<string> PhotoUrls { get; init; } = [];
    public IReadOnlyList<Tag> Tags { get; init; } = [];
    public string? Status { get; init; }
}

public sealed record Category(long Id, string Name);

public sealed record Tag(long Id, string Name);

// Named "ApiResponse" in the Swagger contract; used for both errors and delete confirmations.
public sealed record ApiMessage(int Code, string? Type, string? Message);
