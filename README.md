# Petstore API — BDD tests

Automated API tests for the `Pet` entity of the public [Swagger Petstore](https://petstore.swagger.io/) API:
`POST /pet`, `GET /pet/{petId}`, `DELETE /pet/{petId}`.

**Stack:** .NET 10, Reqnroll 3, xUnit v3, `HttpClient`, Microsoft.Extensions.DependencyInjection / Configuration.

## Running

```bash
dotnet test
```

Run a subset by tag/category (Reqnroll tags become xUnit traits):

```bash
dotnet test --filter "Category=delete"
```

Point the suite to another environment without editing files:

```bash
PetStore__BaseUrl=https://my-petstore.local/v2/ dotnet test
```

## Structure

```
.
├── Features/              # Gherkin: one feature per endpoint
├── StepDefinitions/       # PetSteps: Given = arrange + verify, When = call API, Then = assert
├── Api/                   # PetStoreClient, ApiResponse (HTTP exchange snapshot), DTOs
├── TestData/              # PetFactory: unique, self-contained test data
├── Support/               # DI setup, created-pets tracker, cleanup hook
├── Configuration/         # Typed, validated settings
├── appsettings.json
└── PetStoreAPI.Tests.csproj
```

## Implementation

- API requests are grouped in a small typed client, with shared request and response models.
- Scenarios create their own test data and attempt to remove created pets afterward.
- Gherkin steps separate test setup, API actions, and assertions.

## Coverage

| Endpoint | Scenarios |
|---|---|
| `POST /pet` | complete pet is returned and persisted; server-generated ID; malformed JSON → 400 |
| `GET /pet/{petId}` | existing pet is returned; deleted pet → 404; confirmed-unused ID → 404 |
| `DELETE /pet/{petId}` | delete is confirmed and the pet is gone; another pet remains unaffected; repeated delete → 404 |
