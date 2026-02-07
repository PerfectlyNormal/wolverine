using Wolverine.AzureServiceBus.Tests.Fixtures;
using Wolverine.ComplianceTests.Compliance;
using Xunit;

namespace Wolverine.AzureServiceBus.Tests;

public class TopicsComplianceFixture : TransportComplianceFixture, IAsyncLifetime
{
    public TopicsComplianceFixture() : base(new Uri("asb://topic/topic1"), 120)
    {
    }

    public async ValueTask InitializeAsync()
    {
        await SenderIs(opts =>
        {
            opts.UseAzureServiceBus(AzureServiceBusE2EFixture.ServiceBusContainer.GetConnectionString(), managementConnectionString: AzureServiceBusE2EFixture.ServiceBusContainer.GetHttpConnectionString())
                .AutoProvision();
        });

        await ReceiverIs(opts =>
        {
            opts.UseAzureServiceBus(AzureServiceBusE2EFixture.ServiceBusContainer.GetConnectionString(), managementConnectionString: AzureServiceBusE2EFixture.ServiceBusContainer.GetHttpConnectionString())
                .AutoProvision();

            opts.ListenToAzureServiceBusSubscription("subscription1").FromTopic("topic1");
        });
    }
}

public class TopicAndSubscriptionSendingAndReceivingCompliance : TransportCompliance<TopicsComplianceFixture>;