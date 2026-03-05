// Import the SignalR namespace so this service can communicate
// with the SignalR Hub and push messages to connected clients
using Microsoft.AspNetCore.SignalR;

// Import our Hub namespace so we can reference StreamHub
using StreamBridgeLive.Hubs;

namespace StreamBridgeLive.Services
{
    // This service simulates a real-time data stream.
    // In real systems this could be:
    // • AI camera detections
    // • sensor telemetry
    // • robot telemetry
    // • cloud monitoring metrics
    public class FrameStreamService
    {
        // SignalR hub context allows this background service
        // to send messages to all connected clients
        private readonly IHubContext<StreamHub> _hub;

        // Constructor injection
        // ASP.NET automatically passes the Hub context
        // when this service is created.
        public FrameStreamService(IHubContext<StreamHub> hub)
        {
            _hub = hub;
        }

        // Start() launches the telemetry loop
        // This simulates a continuous stream of data.
        public void Start()
        {
            // Run this in a background thread so it does not
            // block the web server.
            Task.Run(async () =>
            {
                // Random number generator used to simulate
                // telemetry values
                var random = new Random();

                // Infinite loop — represents a live system
                while (true)
                {
                    
                    // SIMULATED FRAME DATA
                    
                    // This object represents a "frame"
                    // produced by a camera or AI system
                    var frame = new
                    {
                        // Unique frame identifier
                        frameId = random.Next(1000, 9999),

                        // Timestamp when frame was produced
                        timestamp = DateTime.Now,

                        // Simulated number of objects detected
                        // by an AI model (YOLO, ResNet, etc)
                        objectsDetected = random.Next(0, 5)
                    };

                    
                    // TELEMETRY METRICS
                    
                    // These represent system performance metrics
                    var telemetry = new
                    {
                        // Frames per second processed
                        fps = random.Next(25, 60),

                        // Network / processing latency
                        latencyMs = random.Next(5, 40),

                        // Frames dropped due to overload
                        droppedFrames = random.Next(0, 3)
                    };

                    
                    // SEND DATA TO CONNECTED CLIENTS
                    
                    // SignalR broadcasts the data to
                    // every browser connected to the hub.

                    // Send the frame object
                    await _hub.Clients.All.SendAsync("Frame", frame);

                    // Send the telemetry metrics
                    await _hub.Clients.All.SendAsync("Telemetry", telemetry);

                    // Wait 1 second before generating
                    // the next frame update
                    await Task.Delay(1000);
                }
            });
        }
    }
}