using JasperFx.Core;
using Wolverine.AzureServiceBus.Tests.Fixtures;
using Wolverine.ComplianceTests.Compliance;
using Xunit;

namespace Wolverine.AzureServiceBus.Tests;

public class InlineComplianceFixture : TransportComplianceFixture
{
    public InlineComplianceFixture() : base(new Uri("asb://queue/inline-receiver"), 120)
    {
    }

    public async ValueTask InitializeAsync()
    {
        var queueName = Guid.NewGuid().ToString();
        OutboundAddress = new Uri("asb://queue/" + queueName);

        await SenderIs(opts =>
        {
            opts.UseAzureServiceBus(AzureServiceBusE2EFixture.ServiceBusContainer.GetConnectionString(), managementConnectionString: AzureServiceBusE2EFixture.ServiceBusContainer.GetHttpConnectionString())
                .AutoProvision();
        });

        await ReceiverIs(opts =>
        {
            opts.UseAzureServiceBus(AzureServiceBusE2EFixture.ServiceBusContainer.GetConnectionString(), managementConnectionString: AzureServiceBusE2EFixture.ServiceBusContainer.GetHttpConnectionString())
                .AutoProvision();

            #region sample_using_process_inline

            // Configuring a Wolverine application to listen to
            // an Azure Service Bus queue with the "Inline" mode
            opts.ListenToAzureServiceBusQueue(queueName, q => q.Options.AutoDeleteOnIdle = 5.Minutes()).ProcessInline();

            #endregion
        });
    }
}

public class InlineSendingAndReceivingCompliance : TransportCompliance<InlineComplianceFixture>;