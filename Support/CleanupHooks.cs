using System.Net;
using PetStoreAPI.Tests.Api;
using Reqnroll;

namespace PetStoreAPI.Tests.Support;

[Binding]
public sealed class CleanupHooks(CreatedPetsTracker tracker, PetStoreClient client, IReqnrollOutputHelper output)
{
    // Runs even if a step failed; every ID is attempted and a single failure never hides the others.
    [AfterScenario]
    public async Task DeleteCreatedPets()
    {
        foreach (var petId in tracker.Ids)
        {
            try
            {
                var response = await client.DeletePetAsync(petId);
                if (response.StatusCode is not (HttpStatusCode.OK or HttpStatusCode.NotFound))
                    output.WriteLine($"[cleanup] Pet {petId} was not deleted.\n{response}");
            }
            catch (Exception ex)
            {
                output.WriteLine($"[cleanup] Pet {petId} was not deleted: {ex.Message}");
            }
        }
    }
}
