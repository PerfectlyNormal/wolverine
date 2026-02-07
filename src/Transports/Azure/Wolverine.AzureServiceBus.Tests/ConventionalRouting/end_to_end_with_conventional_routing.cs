using JasperFx.Core;
using Microsoft.Extensions.Hosting;
using JasperFx.Resources;
using Shouldly;
using Wolverine.ComplianceTests;
using Wolverine.Tracking;
using Xunit;
using Wolverine.AzureServiceBus.Tests.Fixtures;

namespace Wolverine.AzureServiceBus.Tests.ConventionalRouting;

[Collection(nameof(AzureServiceBusE2E))]
public class end_to_end_with_conventional_routing(AzureServiceBusE2EFixture fixture) : IAsyncLifetime
{
    private IHost _receiver;
    private IHost _sender;

    public async Task InitializeAsync()
    {
        _sender = await Host.CreateDefaultBuilder()
            .UseWolverine(opts =>
            {
                opts.UseAzureServiceBus(fixture.ConnectionString, managementConnectionString: fixture.ManagementConnectionString)
                    .UseConventionalRouting().AutoProvision().AutoPurgeOnStartup();
                opts.DisableConventionalDiscovery();
                opts.ServiceName = "Sender";

                opts.Services.AddResourceSetupOnStartup();
            }).StartAsync();

        _receiver = await Host.CreateDefaultBuilder()
            .UseWolverine(opts =>
            {
                opts.UseAzureServiceBus(fixture.ConnectionString, managementConnectionString: fixture.ManagementConnectionString)
                    .UseConventionalRouting().AutoProvision().AutoPurgeOnStartup();
                opts.ServiceName = "Receiver";
                
                opts.Services.AddResourceSetupOnStartup();
            }).StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _sender.StopAsync();
        await _receiver.StopAsync();
    }

    [Fact]
    public async Task send_from_one_node_to_another_all_with_conventional_routing()
    {
        var session = await _sender.TrackActivity()
            .AlsoTrack(_receiver)
            .IncludeExternalTransports()
            .Timeout(30.Seconds())
            .SendMessageAndWaitAsync(new RoutedMessage());

        var received = session
            .AllRecordsInOrder()
            .Where(x => x.Envelope.Message?.GetType() == typeof(RoutedMessage))
            .Single(x => x.MessageEventType == MessageEventType.Received);

        received
            .ServiceName.ShouldBe("Receiver");
    }
}