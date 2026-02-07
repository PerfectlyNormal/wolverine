using Testcontainers.ServiceBus;
using Xunit;

namespace Wolverine.AzureServiceBus.Tests.Fixtures;

public class AzureServiceBusE2EFixture : IAsyncLifetime
{
    //private static readonly ServiceBusContainer _serviceBusContainer =
    //    new ServiceBusBuilder("mcr.microsoft.com/azure-messaging/servicebus-emulator:latest")
    //    .WithAcceptLicenseAgreement(true)
    //    .Build();

    private static readonly DockerHostedServiceBus _serviceBusContainer = new();

    public virtual ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
        //await _serviceBusContainer.StartAsync();
    }

    public static DockerHostedServiceBus ServiceBusContainer => _serviceBusContainer;
    public string ConnectionString => _serviceBusContainer.GetConnectionString();
    public string ManagementConnectionString => _serviceBusContainer.GetHttpConnectionString();

    public virtual ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
        //throw new Exception("Disposing already!?");
        //await _serviceBusContainer.StopAsync();
        //await _serviceBusContainer.DisposeAsync();
    }
}

public class DockerHostedServiceBus
{
    public string GetConnectionString() => "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";
    public string GetHttpConnectionString() => "Endpoint=sb://localhost:5300;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";
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
    [Obsolete("Coming in a later Testcontainers release, so use that when available")]
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
