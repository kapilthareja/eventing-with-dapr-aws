using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Dapr;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Dapr.Client;
using Microsoft.Extensions.Logging;

// Define the same message class as the producer
public class Message
{
    public string Content { get; set; }
    public int MessageNumber { get; set; }
}

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders().AddConsole();
        builder.Services.AddControllers().AddDapr();
        builder.Services.AddEndpointsApiExplorer();

        var app = builder.Build();

        app.UseRouting();
        app.UseCloudEvents();
        app.UseEndpoints(endpoints =>
        {
            // Configure the endpoint to listen for messages from the "my-redis-topic"
            endpoints.MapSubscribeHandler();
            endpoints.MapPost("/my-redis-topic", async (HttpContext httpContext) =>
            {
                // --- IMPORTANT CHANGE HERE ---
                // Manually read the raw bytes from the incoming request body.
                await using var memoryStream = new MemoryStream();
                await httpContext.Request.Body.CopyToAsync(memoryStream);
                var messageBytes = memoryStream.ToArray();

                // Deserialize the byte array back into a Message object.
                var message = JsonSerializer.Deserialize<Message>(messageBytes);

                Console.WriteLine($"Received message: {message.Content}");
            }).WithTopic("redis-pubsub", "my-redis-topic");
        });

        app.Run();
    }
}


/////////////////BELOW IS REDIS WORKING VERSION ////////////////////////////////////////

//// --- CONSUMER APPLICATION (ConsumerApp/Program.cs) ---

//using Dapr; // Required for [Topic] attribute
//using Dapr.Client;
//using Microsoft.AspNetCore.Mvc;
//using System.Text.Json;
//using System.Text.Json.Serialization;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddControllers().AddDapr(); // Important: Add .AddDapr()
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseCloudEvents(); // Required for Dapr Pub/Sub
//app.UseAuthorization();
//app.MapControllers();
//app.MapSubscribeHandler(); // Exposes the /dapr/subscribe endpoint for Dapr to discover subscriptions

//app.Run();

//[ApiController]
//[Route("[controller]")]
//public class MessageController : ControllerBase
//{
//    private readonly ILogger<MessageController> _logger;

//    public MessageController(ILogger<MessageController> logger)
//    {
//        _logger = logger;
//    }

//    // This endpoint handles messages from the "my-redis-topic" topic on the "redis-pubsub" component.
//    // The [Topic] attribute tells Dapr which topic and pubsub component to subscribe to.
//    // Dapr automatically handles the HTTP POST request and deserializes the body into the 'message' parameter.
//    [Topic("redis-pubsub", "my-redis-topic")]
//    [HttpPost("message")]
//    public IActionResult PostMessage([FromBody] Message message)
//    {
//        _logger.LogInformation("Received message from my-redis-topic: Content={Content}, Timestamp={Timestamp}", message.Content, message.Timestamp);

//        // A successful response (HTTP 200 OK) tells Dapr the message was processed.
//        return Ok();
//    }
//}

//public class Message
//{
//    [JsonPropertyName("content")]
//    public string? Content { get; set; }

//    [JsonPropertyName("timestamp")]
//    public DateTime Timestamp { get; set; }
//}

//public record MyMessage(string Content, DateTime Timestamp);

///*
//// To run the consumer:
//// 1. Ensure Dapr CLI is installed and Redis container is running (dapr init does this by default).
//// 2. Navigate to the ConsumerApp directory.
//// 3. Run: dapr run --app-id consumerapp --app-port 5000 --dapr-http-port 3501 --resources-path ..\components -- dotnet run
////    (--dapr-http-port must be different from producer's if both run locally)
////    (Replace 5000 with your actual ASP.NET Core port if different)
//*/