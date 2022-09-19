using System;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace CustomerFunctions
{
    // Pre-function work
    // 1) create a managed identity in the Customer subscription to interface with the service bus
    // 2) wire the managed identity in the IAM service bus blade and give it the "Service Bus Data Owner" permissions
    public class ReadDeploymentCommandAndSendDeploymentStatus
    {
        // This customer-side service bus trigger connects to the customer's
        // service bus with a managed identity to read the contents.
        [FunctionName("ReadDeploymentCommandAndSendDeploymentStatus")]

        // ServiceBusConnection resolves to ServiceBusConnection__fullyQualifiedNamespace
        // which is referenced within the local.settings.json file to use the managed identity
        public void Run([ServiceBusTrigger("%DeploymentCommandQueueName%", 
            Connection = "ServiceBusConnection")]string command, ILogger log)
        {
            log.LogInformation($"ReadDeploymentCommandAndSendDeploymentStatus triggered with command: {command}");

            var credential = new DefaultAzureCredential();

            var serviceBusNamespace = Environment.GetEnvironmentVariable("ServiceBusConnection__fullyQualifiedNamespace");
            var deploymentStatusQueueName = Environment.GetEnvironmentVariable("DeploymentStatusQueueName");
            var client = new ServiceBusClient(serviceBusNamespace, credential);

            var sender = client.CreateSender(deploymentStatusQueueName);
            var message = new ServiceBusMessage(command);
            sender.SendMessageAsync(message).GetAwaiter().GetResult();
        }       
    }
}
