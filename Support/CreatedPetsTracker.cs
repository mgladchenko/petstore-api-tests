namespace PetStoreAPI.Tests.Support;

/// <summary>IDs of pets created during the current scenario; drained by <see cref="CleanupHooks"/>.</summary>
public sealed class CreatedPetsTracker
{
    private readonly HashSet<long> _ids = [];

    public IReadOnlyCollection<long> Ids => _ids;

    public void Track(long petId) => _ids.Add(petId);
}
