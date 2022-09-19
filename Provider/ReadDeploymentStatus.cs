using System;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ProviderFunctions
{
    public class ReadDeploymentStatus
    {
        // A function for the Provider to try reading from the Customer's deployment status
        // queue every 10 seconds
        [FunctionName("ReadDeploymentStatus")]
        public void Run([TimerTrigger("*/10 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            var customerTenantId = Environment.GetEnvironmentVariable("CustomerTenantId");
            var providerClientId = Environment.GetEnvironmentVariable("ClientId");
            var providerClientSecret = Environment.GetEnvironmentVariable("ClientSecret");
            var credential = new ClientSecretCredential(customerTenantId, providerClientId, providerClientSecret);

            var serviceBusNamespace = Environment.GetEnvironmentVariable("ServiceBusNamespace");
            var deploymentStatusQueueName = Environment.GetEnvironmentVariable("DeploymentStatusQueueName");
            var client = new ServiceBusClient(serviceBusNamespace, credential);
            
            var receiver = client.CreateReceiver(deploymentStatusQueueName);

            var receivedMessage = receiver.ReceiveMessageAsync().GetAwaiter().GetResult();
            if (receivedMessage == null)
            {   
                log.LogInformation($">> No message read -- {DateTime.Now}");
                return;
            }
            
            var body = receivedMessage.Body.ToString();
            log.LogInformation($">> MESSAGE RECEIVED: {body} -- {DateTime.Now}");

            // Mark the message for deletion on the queue
            receiver.CompleteMessageAsync(receivedMessage).GetAwaiter().GetResult();
        }
    }
}
