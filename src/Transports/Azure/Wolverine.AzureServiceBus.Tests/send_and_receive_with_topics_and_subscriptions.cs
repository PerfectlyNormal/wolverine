using Wolverine.AzureServiceBus.Tests.Fixtures;
using Wolverine.ComplianceTests.Compliance;
using Xunit;

namespace Wolverine.AzureServiceBus.Tests;

public class TopicsComplianceFixture : AzureServiceBusTransportComplianceFixture, IAsyncLifetime
{
    public TopicsComplianceFixture() : base(new Uri("asb://topic/topic1"), 120)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await SenderIs(opts =>
        {
            opts.UseAzureServiceBus(ConnectionString, managementConnectionString: ManagementConnectionString)
                .AutoProvision();
        });

        await ReceiverIs(opts =>
        {
            opts.UseAzureServiceBus(ConnectionString, managementConnectionString: ManagementConnectionString)
                .AutoProvision();

            opts.ListenToAzureServiceBusSubscription("subscription1").FromTopic("topic1");
        });
    }
}

public class TopicAndSubscriptionSendingAndReceivingCompliance : TransportCompliance<TopicsComplianceFixture>;