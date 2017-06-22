using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;

namespace NUnitWithASB
{
    [TestFixture]
    public class When_using_ASB_with_NUnit
    {
        [Test]
        public async Task Should_not_fail()
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureServiceBus.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            var queuePath = "repro";
            if (! await namespaceManager.QueueExistsAsync(queuePath))
            {
                await namespaceManager.CreateQueueAsync(queuePath);
            }

            var queueClient = QueueClient.CreateFromConnectionString(connectionString, queuePath);

            await queueClient.SendAsync(new BrokeredMessage("message"));

            var taskCompletionSource = new TaskCompletionSource<bool>();

            queueClient.OnMessageAsync(async message =>
            {
                var payload = message.GetBody<string>();
                try
                {
                    Assert.AreEqual(payload, "message");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    taskCompletionSource.SetResult(false);
                }

                await message.CompleteAsync();

                taskCompletionSource.SetResult(true);
            }, new OnMessageOptions
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
