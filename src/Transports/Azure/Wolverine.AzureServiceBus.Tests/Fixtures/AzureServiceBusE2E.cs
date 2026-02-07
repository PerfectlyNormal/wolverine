using Testcontainers.ServiceBus;
using Wolverine.ComplianceTests.Compliance;
using Xunit;

namespace Wolverine.AzureServiceBus.Tests.Fixtures;

[CollectionDefinition("AzureServiceBusE2E")]
public class AzureServiceBusE2E : ICollectionFixture<AzureServiceBusE2EFixture>
{
}

public class AzureServiceBusE2EFixture : IAsyncLifetime
{
    private ServiceBusContainer _serviceBusContainer;

    public virtual async Task InitializeAsync()
    {
        _serviceBusContainer = new ServiceBusBuilder("mcr.microsoft.com/azure-messaging/servicebus-emulator:latest")
            .WithAcceptLicenseAgreement(true)
            .Build();

        await _serviceBusContainer.StartAsync();
    }

    public ServiceBusContainer ServiceBus => _serviceBusContainer;

    public string ConnectionString => _serviceBusContainer.GetConnectionString();
    public string ManagementConnectionString => _serviceBusContainer.GetHttpConnectionString();

    public virtual async Task DisposeAsync()
    {
        await _serviceBusContainer.StopAsync();
        await _serviceBusContainer.DisposeAsync();
    }
}

public class AzureServiceBusTransportComplianceFixture(Uri destination, int defaultTimeInSeconds = 5) : TransportComplianceFixture(destination, defaultTimeInSeconds), IAsyncLifetime
{
    private ServiceBusContainer _serviceBusContainer;

    public virtual async Task InitializeAsync()
    {
        _serviceBusContainer = new ServiceBusBuilder("mcr.microsoft.com/azure-messaging/servicebus-emulator:latest")
            .WithAcceptLicenseAgreement(true)
            .Build();

        await _serviceBusContainer.StartAsync();
    }

    public ServiceBusContainer ServiceBus => _serviceBusContainer;

    public string ConnectionString => _serviceBusContainer.GetConnectionString();
    public string ManagementConnectionString => _serviceBusContainer.GetHttpConnectionString();

    public virtual async Task DisposeAsync()
    {
        await _serviceBusContainer.StopAsync();
        await _serviceBusContainer.DisposeAsync();
    }
}

public static class ServiceBusContainerExtensions
{
    /// <summary>
    /// Gets the Service Bus HTTP connection string.
    /// </summary>
    /// <remarks>
    /// This connection string is intended for use with the ServiceBusAdministrationClient.
    /// </remarks>
    /// <returns>The Service Bus HTTP connection string.</returns>
    [Obsolete("Coming in a later Testcontainers release")]
    public static string GetHttpConnectionString(this ServiceBusContainer sb)
    {
        var properties = new Dictionary<string, string>
        {
            { "Endpoint", new UriBuilder("sb", sb.Hostname, sb.GetMappedPublicPort(ServiceBusBuilder.ServiceBusHttpPort)).ToString() },
            { "SharedAccessKeyName", "RootManageSharedAccessKey" },
            { "SharedAccessKey", "SAS_KEY_VALUE" },
            { "UseDevelopmentEmulator", "true" }
        };
        return string.Join(";", properties.Select(property => string.Join("=", property.Key, property.Value)));
    }
}
