using IntegrationTests;
using Npgsql;
using Weasel.Postgresql;
using Wolverine.AzureServiceBus.Tests.Fixtures;
using Wolverine.ComplianceTests;
using Wolverine.Postgresql;
using Xunit;

namespace Wolverine.AzureServiceBus.Tests;

[Collection(nameof(AzureServiceBusE2E))]
public class leader_election : LeadershipElectionCompliance
{
    private readonly AzureServiceBusE2EFixture fixture;

    public leader_election(AzureServiceBusE2EFixture fixture, ITestOutputHelper output) : base(output)
    {
        this.fixture = fixture;
    }

    protected override async Task beforeBuildingHost()
    {
        await using var conn = new NpgsqlConnection(Servers.PostgresConnectionString);
        await conn.OpenAsync();
        await conn.DropSchemaAsync("registry");
        await conn.CloseAsync();
    }

    protected override void configureNode(WolverineOptions opts)
    {
        opts.UseAzureServiceBus(fixture.ConnectionString, managementConnectionString: fixture.ManagementConnectionString).EnableWolverineControlQueues();
        opts.PersistMessagesWithPostgresql(Servers.PostgresConnectionString, "registry");
    }
}