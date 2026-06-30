using StreamBridgeLive_VisionOps.Hubs;
using StreamBridgeLive_VisionOps.Services;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddSignalR();
builder.Services.AddSingleton<FrameStreamService>();

var app = builder.Build();

// Allow serving files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// Map SignalR hub endpoint
app.MapHub<StreamHub>("/streamhub");

// Start the telemetry service when app starts
var frameService = app.Services.GetRequiredService<FrameStreamService>();
frameService.Start();

// Start the web server
app.Run()
