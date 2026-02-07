using Azure.Messaging.ServiceBus.Administration;
using Shouldly;
using Wolverine.AzureServiceBus.Tests.Fixtures;
using Wolverine.ComplianceTests.Compliance;
using Wolverine.Tracking;
using Xunit;

namespace Wolverine.AzureServiceBus.Tests;

public class TopicsWithCustomRuleComplianceFixture()
    : AzureServiceBusTransportComplianceFixture(new Uri("asb://topic/topic1"), 120)
{
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

            opts.ListenToAzureServiceBusSubscription(
                    "subscription1",
                    configureSubscriptionRule: rule =>
                    {
                        rule.Filter = new SqlRuleFilter("NOT EXISTS(user.ignore) OR user.ignore NOT LIKE 'true'");
                    })
                .FromTopic("topic1");
        });
    }
}

public class TopicAndSubscriptionWithCustomRuleSendingAndReceivingCompliance : TransportCompliance<TopicsWithCustomRuleComplianceFixture>
{
    [Fact]
    public async Task ignores_message_not_matching_the_filter()
    {
        /*
         * Please note that this test may take a while to run,
         * as it will wait for a message to be processed by the receiver
         * but there should none be incoming because of the subscription
         * filter.
         */

        var session = await theSender.TrackActivity(Fixture.DefaultTimeout)
            .AlsoTrack(theReceiver)
            .DoNotAssertOnExceptionsDetected()
            .ExecuteAndWaitAsync(
                c => c.EndpointFor(theOutboundAddress).SendAsync(
                    new Message1(),
                    new DeliveryOptions()
                        .WithHeader("ignore", "true")));

        var record = session.FindEnvelopesWithMessageType<Message1>(MessageEventType.MessageSucceeded).SingleOrDefault();
        record.ShouldBeNull();
    }
}