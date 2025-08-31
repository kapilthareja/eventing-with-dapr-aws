// --- CONSUMER APPLICATION (ConsumerApp/Program.cs) ---

using Dapr; // Required for [Topic] attribute
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddDapr(); // Important: Add .AddDapr()
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCloudEvents(); // Required for Dapr Pub/Sub
app.UseAuthorization();
app.MapControllers();
app.MapSubscribeHandler(); // Exposes the /dapr/subscribe endpoint for Dapr to discover subscriptions

app.Run();

//public class MessageController : ControllerBase
//{
//    private readonly ILogger<MessageController> _logger;

//    public MessageController(ILogger<MessageController> logger)
//    {
//        _logger = logger;
//    }

//    [HttpPost("/messages")] // Endpoint for Dapr to send messages
//    [Topic("redis-pubsub", "my-redis-topic")] // *** CHANGED to Redis component name and topic ***
//    public ActionResult<MyMessage> ProcessMessage(MyMessage message)
//    {
//        _logger.LogInformation($"Received message:" + message.Content);
//        _logger.LogInformation($"Received message: {JsonSerializer.Serialize(message)}");
//        // Process the message here (e.g., save to database, perform business logic)
//        return Ok(message); // Acknowledge message processing
//    }
//}

[ApiController]
[Route("[controller]")]
public class MessageController : ControllerBase
{
    private readonly ILogger<MessageController> _logger;

    public MessageController(ILogger<MessageController> logger)
    {
        _logger = logger;
    }

    // This endpoint handles messages from the "my-redis-topic" topic on the "redis-pubsub" component.
    // The [Topic] attribute tells Dapr which topic and pubsub component to subscribe to.
    // Dapr automatically handles the HTTP POST request and deserializes the body into the 'message' parameter.
    [Topic("redis-pubsub", "my-redis-topic")]
    [HttpPost("message")]
    public IActionResult PostMessage([FromBody] Message message)
    {
        _logger.LogInformation("Received message from my-redis-topic: Content={Content}, Timestamp={Timestamp}", message.Content, message.Timestamp);

        // A successful response (HTTP 200 OK) tells Dapr the message was processed.
        return Ok();
    }
}

public class Message
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

public record MyMessage(string Content, DateTime Timestamp);

/*
// To run the consumer:
// 1. Ensure Dapr CLI is installed and Redis container is running (dapr init does this by default).
// 2. Navigate to the ConsumerApp directory.
// 3. Run: dapr run --app-id consumerapp --app-port 5000 --dapr-http-port 3501 --resources-path ..\components -- dotnet run
//    (--dapr-http-port must be different from producer's if both run locally)
//    (Replace 5000 with your actual ASP.NET Core port if different)
*/