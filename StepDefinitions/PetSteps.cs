using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Nodes;
using PetStoreAPI.Tests.Api;
using PetStoreAPI.Tests.Api.Models;
using PetStoreAPI.Tests.Support;
using PetStoreAPI.Tests.TestData;
using Reqnroll;

namespace PetStoreAPI.Tests.StepDefinitions;

/// <summary>
/// Reqnroll creates a new instance of this class per scenario, so the fields below are the scenario state.
/// Given steps prepare and verify data, When steps only call the API, Then steps only assert.
/// </summary>
[Binding]
public sealed class PetSteps(PetStoreClient client, CreatedPetsTracker tracker)
{
    private Pet? _pet;
    private Pet? _otherPet;
    private ApiResponse? _response;

    private Pet Pet => _pet
        ?? throw new InvalidOperationException("No pet defined. The scenario must start with a Given step that defines a pet.");

    private long PetId => Pet.Id
        ?? throw new InvalidOperationException("The pet has no ID. It must be created or assigned an ID first.");

    private ApiResponse Response => _response
        ?? throw new InvalidOperationException("No API response captured. The scenario must call the API in a When step first.");

    #region Given

    [Given("a new pet with all fields populated")]
    public void GivenNewPetWithAllFields() => _pet = PetFactory.Complete();

    [Given("a new pet with only the required fields and no ID")]
    public void GivenNewPetWithRequiredFieldsOnly() => _pet = PetFactory.RequiredFieldsOnly();

    [Given("an existing pet in the store")]
    public async Task GivenExistingPet() => _pet = await CreatePetInStoreAsync();

    [Given("another existing pet in the store")]
    public async Task GivenAnotherExistingPet() => _otherPet = await CreatePetInStoreAsync();

    [Given("a pet that has been deleted from the store")]
    public async Task GivenDeletedPet()
    {
        var pet = await CreatePetInStoreAsync();

        (await client.DeletePetAsync(pet.Id!.Value)).EnsureStatus(HttpStatusCode.OK, "delete the pet");
        (await client.GetPetAsync(pet.Id.Value)).EnsureStatus(HttpStatusCode.NotFound, "confirm the pet is gone");

        _pet = pet;
    }

    [Given("a pet ID not present in the store")]
    public async Task GivenUnusedPetId()
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = PetFactory.Complete();
            var response = await client.GetPetAsync(candidate.Id!.Value);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _pet = candidate;
                return;
            }

            response.EnsureStatus(HttpStatusCode.OK, "confirm that a pet ID is unused");
        }

        throw new InvalidOperationException("Could not find an unused pet ID after 5 attempts.");
    }

    #endregion

    #region When

    [When("I create the pet")]
    public async Task WhenICreateThePet()
    {
        if (Pet.Id is { } id)
            tracker.Track(id);

        _response = await client.CreatePetAsync(Pet);
        TrackPetFromResponse(_response);
    }

    [When("I send a create pet request with content type {string} and body:")]
    public async Task WhenISendRawCreateRequest(string contentType, string body)
    {
        _response = await client.CreatePetAsync(body, contentType);
        TrackPetFromResponse(_response);
    }

    [When("I create the pet without the required field {string}")]
    public async Task WhenICreatePetWithoutRequiredField(string fieldName)
    {
        if (fieldName is not ("name" or "photoUrls"))
            throw new ArgumentException("Expected a required pet field: name or photoUrls.", nameof(fieldName));

        var body = JsonSerializer.SerializeToNode(Pet, PetStoreClient.JsonOptions)!.AsObject();
        if (!body.Remove(fieldName))
            throw new InvalidOperationException($"The pet payload does not contain '{fieldName}'.");

        tracker.Track(PetId);
        _response = await client.CreatePetAsync(body.ToJsonString(), MediaTypeNames.Application.Json);
        TrackPetFromResponse(_response);
    }

    [When("I create the pet with field {string} set to JSON value {string}")]
    public async Task WhenICreatePetWithFieldValue(string fieldName, string jsonValue)
    {
        var body = JsonSerializer.SerializeToNode(Pet, PetStoreClient.JsonOptions)!.AsObject();
        if (!body.ContainsKey(fieldName))
            throw new ArgumentException($"The pet payload does not contain '{fieldName}'.", nameof(fieldName));

        body[fieldName] = JsonNode.Parse(jsonValue);
        if (fieldName != "id")
            tracker.Track(PetId);

        _response = await client.CreatePetAsync(body.ToJsonString(), MediaTypeNames.Application.Json);
        TrackPetFromResponse(_response);
    }

    [When("I request the pet by its ID")]
    public async Task WhenIRequestThePet() => _response = await client.GetPetAsync(PetId);

    [When("I request a pet with ID {string}")]
    public async Task WhenIRequestPetWithId(string petId) => _response = await client.GetPetAsync(petId);

    [When("I delete the pet")]
    public async Task WhenIDeleteThePet() => _response = await client.DeletePetAsync(PetId);

    [When("I delete a pet with ID {string}")]
    public async Task WhenIDeletePetWithId(string petId) => _response = await client.DeletePetAsync(petId);

    #endregion

    #region Then

    [Then("the response status code is {int}")]
    public void ThenStatusCodeIs(int expected) =>
        Assert.True(
            (int)Response.StatusCode == expected,
            $"Expected status {expected}, got {(int)Response.StatusCode}.\n{Response}");

    [Then("the response is in JSON format")]
    public void ThenResponseIsJson() =>
        Assert.Equal(MediaTypeNames.Application.Json, Response.MediaType);

    [Then("the response contains the pet")]
    public void ThenResponseContainsThePet() =>
        Assert.Equivalent(Pet, Response.ReadAs<Pet>(), strict: true);

    [Then("the store assigns an ID to the pet")]
    public void ThenStoreAssignsId()
    {
        var created = Response.ReadAs<Pet>();

        Assert.True(created.Id is > 0, $"Expected a positive generated ID.\n{Response}");

        _pet = Pet with { Id = created.Id };
        Assert.Equivalent(Pet, created, strict: true);
    }

    [Then("the pet can be retrieved from the store")]
    public async Task ThenPetCanBeRetrieved() => await AssertPetIsStoredAsync(Pet);

    [Then("the other pet is still in the store")]
    public async Task ThenOtherPetIsStillStored() =>
        await AssertPetIsStoredAsync(_otherPet ?? throw new InvalidOperationException("No other pet was created in this scenario."));

    [Then("the pet is no longer in the store")]
    public async Task ThenPetIsNoLongerStored()
    {
        var response = await client.GetPetAsync(PetId);
        Assert.True(response.StatusCode == HttpStatusCode.NotFound, $"Expected the pet to be deleted.\n{response}");
    }

    [Then("the error message is {string}")]
    public void ThenErrorMessageIs(string expected) =>
        Assert.Equal(expected, Response.ReadAs<ApiMessage>().Message);

    [Then("the response confirms deletion of the pet")]
    public void ThenResponseConfirmsDeletion() =>
        Assert.Equal(PetId.ToString(), Response.ReadAs<ApiMessage>().Message);

    #endregion

    private async Task<Pet> CreatePetInStoreAsync()
    {
        var pet = PetFactory.Complete();
        tracker.Track(pet.Id!.Value);

        (await client.CreatePetAsync(pet)).EnsureStatus(HttpStatusCode.OK, "create a pet");
        (await client.GetPetAsync(pet.Id.Value)).EnsureStatus(HttpStatusCode.OK, "read back the created pet");

        return pet;
    }

    private async Task AssertPetIsStoredAsync(Pet expected)
    {
        var response = await client.GetPetAsync(expected.Id!.Value);

        Assert.True(response.StatusCode == HttpStatusCode.OK, $"Expected the pet to be stored.\n{response}");
        Assert.Equivalent(expected, response.ReadAs<Pet>(), strict: true);
    }

    private void TrackPetFromResponse(ApiResponse response)
    {
        if (response.StatusCode == HttpStatusCode.OK && response.TryReadAs<Pet>(out var created) && created.Id is { } id)
            tracker.Track(id);
    }
}
