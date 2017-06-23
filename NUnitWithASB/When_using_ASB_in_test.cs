using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using NUnit.Framework;

namespace NUnitWithASB
{
    [TestFixture]
    public class When_using_ASB_with_NUnit
    {
        [Test]
        public async Task Should_not_fail()
        {
            // Ensure queue exists... because ASB management is a PITA now.

            var connectionString = Environment.GetEnvironmentVariable("AzureServiceBus.ConnectionString");
            var queuePath = "repro";

            var connectionStringBuilder = new ServiceBusConnectionStringBuilder(connectionString);
            connectionStringBuilder.EntityPath = queuePath;
            var queueClient = new QueueClient(connectionStringBuilder);

            await queueClient.SendAsync(new Message(Encoding.UTF8.GetBytes("message")));

            var taskCompletionSource = new TaskCompletionSource<bool>();

            queueClient.RegisterMessageHandler(async (message, token) =>
            {
                var payload = Encoding.UTF8.GetString(message.Body);
                try
                {
                    Assert.AreEqual(payload, "message");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    taskCompletionSource.SetResult(false);
                }

                await queueClient.CompleteAsync(message.SystemProperties.LockToken);

                taskCompletionSource.SetResult(true);
            }, new MessageHandlerOptions
            {
                AutoComplete = false,
                MaxConcurrentCalls = 1
            });

            var result = await taskCompletionSource.Task;
            Assert.IsTrue(result, "NUnit threw an exception on assertion while executing under Test method");

            await queueClient.CloseAsync();
        }
    }
}
