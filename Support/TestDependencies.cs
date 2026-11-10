using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PetStoreAPI.Tests.Api;
using PetStoreAPI.Tests.Configuration;
using Reqnroll.Microsoft.Extensions.DependencyInjection;

namespace PetStoreAPI.Tests.Support;

public static class TestDependencies
{
    [ScenarioDependencies]
    public static IServiceCollection CreateServices()
    {
        var settings = LoadSettings();
        var services = new ServiceCollection();

        services.AddSingleton(settings);
        services.AddHttpClient<PetStoreClient>(client =>
        {
            client.BaseAddress = settings.BaseUrl;
            client.Timeout = TimeSpan.FromSeconds(settings.RequestTimeoutSeconds);
        });

        // Scoped == one instance per scenario.
        services.AddScoped<CreatedPetsTracker>();

        return services;
    }

    private static PetStoreSettings LoadSettings()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var settings = configuration.GetSection(PetStoreSettings.SectionName).Get<PetStoreSettings>()
            ?? throw new InvalidOperationException($"Configuration section '{PetStoreSettings.SectionName}' is missing.");

        settings.Validate();
        return settings;
    }
}
