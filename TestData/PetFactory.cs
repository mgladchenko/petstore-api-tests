using PetStoreAPI.Tests.Api.Models;

namespace PetStoreAPI.Tests.TestData;

/// <summary>Builds unique pets so scenarios never collide with each other or with other users of the public API.</summary>
public static class PetFactory
{
    // High range keeps test data away from the small IDs commonly used by other Petstore users.
    private const long MinId = 9_000_000_000;
    private const long MaxId = 9_900_000_000;

    public static long UniqueId() => Random.Shared.NextInt64(MinId, MaxId);

    public static Pet Complete() => new()
    {
        Id = UniqueId(),
        Category = new Category(Random.Shared.Next(1, 1000), "Dogs"),
        Name = UniqueName(),
        PhotoUrls = ["https://example.com/photos/1.png", "https://example.com/photos/2.png"],
        Tags = [new Tag(Random.Shared.Next(1, 1000), "friendly"), new Tag(Random.Shared.Next(1, 1000), "vaccinated")],
        Status = "available",
    };

    public static Pet RequiredFieldsOnly() => new()
    {
        Name = UniqueName(),
        PhotoUrls = ["https://example.com/photos/1.png"],
    };

    private static string UniqueName() => $"qa-pet-{Guid.NewGuid():N}"[..20];
}
