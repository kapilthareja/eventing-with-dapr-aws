using System;
using System.Text.Json;
using System.Threading.Tasks;
using Dapr.Client;

// Define a simple message class
public class Message
{
    public string Content { get; set; }
    public int MessageNumber { get; set; }
}

class Program
{
    static async Task Main(string[] args)
    {
        // Name of the pub/sub component and topic
        const string pubsubName = "redis-pubsub";
        const string topicName = "my-redis-topic";

        var client = new DaprClientBuilder().Build();

        int messageCount = 0;
        while (true)
        {
            Thread.Sleep(2000);
            messageCount++;
            var message = new Message
            {
                Content = $"Hello from Producer! Message number {messageCount}",
                MessageNumber = messageCount
            };

            // --- IMPORTANT CHANGE HERE ---
            // Serialize the message object into a byte array.
            // This is necessary to use PublishByteEventAsync.
            var messageBytes = JsonSerializer.SerializeToUtf8Bytes(message);

            // Publish the byte array event to the Dapr topic.
            await client.PublishByteEventAsync(pubsubName, topicName, messageBytes);

            Console.WriteLine($"Published: {message.Content}");

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}





/////////////////BELOW IS REDIS WORKING VERSION ////////////////////////////////////////

//// --- PRODUCER APPLICATION (ProducerApp/Program.cs) ---

//using System.Text.Json;
//using Dapr.Client;

//Console.WriteLine("Dapr .NET Producer Application");

//// Dapr's default gRPC port
//const string DaprHttpPort = "3500";
//const string PubSubComponentName = "redis-pubsub";
//const string TopicName = "my-redis-topic";

//Console.WriteLine($"Publishing messages to Dapr pub/sub component '{PubSubComponentName}' and topic '{TopicName}'");
//Console.WriteLine("Producer will now send messages every second automatically. Press Ctrl+C to stop.");

//using var client = new DaprClientBuilder()
//    .UseHttpEndpoint($"http://localhost:{DaprHttpPort}") // Or DAPR_HTTP_PORT environment variable
//    .Build();



//var messageCount = 0;
//while (true)
//{
//    var message = new MyMessage($"Hello from Producer! Message number {++messageCount}", DateTime.Now);
//    await client.PublishEventAsync(PubSubComponentName, TopicName, message);
//    Console.WriteLine($"Published: {JsonSerializer.Serialize(message)}");

//    await Task.Delay(1000); // Wait for 1 second before sending the next message
//}

//// Console.WriteLine("Producer stopped."); // This line might not be reached if Ctrl+C is used to terminate

//public record MyMessage(string Content, DateTime Timestamp);

///*
//// To run the producer:
//// 1. Ensure Dapr CLI is installed and Redis container is running (dapr init does this by default).
//// 2. Navigate to the ProducerApp directory.
//// 3. Run: dapr run --app-id producerapp --dapr-http-port 3500 --resources-path ..\components -- dotnet run
////    (The --dapr-http-port is optional if DAPR_HTTP_PORT is set)
//*/