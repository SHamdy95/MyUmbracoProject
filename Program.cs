
using Microsoft.AspNetCore.SignalR;
using MyUmbracoProject.Hubs;
using System.Collections.Concurrent;
using System.Text.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// 1. Configure Umbraco
builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddComposers()
    .AddDeliveryApi()
    .Build();

// 2. Configure CORS for Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("angular", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// 3. Register SignalR
builder.Services.AddSignalR();

// 4. Build application
WebApplication app = builder.Build();

// 5. Start Umbraco
await app.BootUmbracoAsync();

// 6. Configure CORS
app.UseCors("angular");

// 7. Configure Umbraco middleware and endpoints
app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

// 8. Store recent events in memory
var recentEvents = new ConcurrentQueue<object>();

// 9. Get recent webhook events
app.MapGet("/api/webhooks/events", (int? take) =>
{
    int count = Math.Clamp(take ?? 20, 1, 20);

    return Results.Json(
        recentEvents
            .Reverse()
            .Take(count)
            .ToArray()
    );
});

// 10. Receive Umbraco webhooks
app.MapPost("/api/webhooks/umbraco", async (
    HttpRequest request,
    IHubContext<WebhookHub> hubContext) =>
{
    Console.WriteLine("========== WEBHOOK RECEIVED ==========");
    Console.WriteLine($"Time: {DateTimeOffset.UtcNow:O}");

    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();

    Console.WriteLine($"Body: {body}");

    var eventName = request.Headers["umb-webhook-event"].ToString();

    JsonElement payload;

    try
    {
        using var document = JsonDocument.Parse(body);
        payload = document.RootElement.Clone();
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"Invalid JSON: {ex.Message}");

        return Results.BadRequest(new
        {
            error = "Request body must contain valid JSON."
        });
    }

    var webhookEvent = new
    {
        receivedAt = DateTimeOffset.UtcNow,
        eventName = string.IsNullOrWhiteSpace(eventName)
            ? "Unknown"
            : eventName,
        payload
    };

    // Save event
    recentEvents.Enqueue(webhookEvent);

    while (recentEvents.Count > 20)
    {
        recentEvents.TryDequeue(out _);
    }

    // Notify connected Angular clients
    await hubContext.Clients.All.SendAsync(
        "WebhookEventReceived",
        webhookEvent
    );

    Console.WriteLine(
        $"Event sent to SignalR: {webhookEvent.eventName}"
    );

    return Results.Ok(new { received = true });
});

// 11. Register SignalR Hub
app.MapHub<WebhookHub>("/hubs/webhook");

// 12. Run application
await app.RunAsync();
