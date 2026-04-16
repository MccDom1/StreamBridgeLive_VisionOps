using Microsoft.AspNetCore.SignalR;
using StreamBridgeLive_VisionOps.Hubs;

namespace StreamBridgeLive_VisionOps.Services
{
    // Phase 2 MVP:
    // This service simulates an AI vision pipeline using generic scenes.
    // It generates:
    // 1) a frame payload with an image + detections
    // 2) telemetry metrics
    // 3) alert messages
    // and broadcasts them to all connected dashboard clients via SignalR.
    public class FrameStreamService
    {
        private readonly IHubContext<StreamHub> _hub;

        public FrameStreamService(IHubContext<StreamHub> hub)
        {
            _hub = hub;
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                var random = new Random();

                var scenes = new List<SceneDefinition>
                {
                    BuildStreetScene(),
                    BuildWarehouseScene(),
                    BuildOfficeScene()
                };

                int frameId = 0;
                int sceneIndex = 0;

                while (true)
                {
                    frameId++;

                    var scene = scenes[sceneIndex];
                    sceneIndex = (sceneIndex + 1) % scenes.Count;

                    int fps = random.Next(24, 42);
                    int latencyMs = random.Next(12, 48);
                    int droppedFrames = random.Next(0, 3);
                    string modelStatus = latencyMs > 40 ? "Degraded" : "Healthy";
                    string streamQuality = droppedFrames > 1 ? "Warning" : "Stable";

                    var framePayload = new
                    {
                        frameId,
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        source = scene.Source,
                        imageUrl = scene.ImageUrl,
                        detections = scene.Detections.Select(d => new
                        {
                            label = d.Label,
                            confidence = d.Confidence,
                            x = d.X,
                            y = d.Y,
                            width = d.Width,
                            height = d.Height
                        }).ToList()
                    };

                    var telemetryPayload = new
                    {
                        fps,
                        latencyMs,
                        droppedFrames,
                        modelStatus,
                        streamQuality,
                        activeDetections = scene.Detections.Count,
                        topClass = scene.Detections
                            .GroupBy(d => d.Label)
                            .OrderByDescending(g => g.Count())
                            .Select(g => g.Key)
                            .FirstOrDefault() ?? "None"
                    };

                    var alertsPayload = BuildAlerts(scene, latencyMs, droppedFrames, modelStatus);
                    Console.WriteLine($"Streaming frame {frameId} | Source: {scene.Source} | FPS: {fps} | Latency: {latencyMs}");

                    await _hub.Clients.All.SendAsync("Frame", framePayload);
                    await _hub.Clients.All.SendAsync("Telemetry", telemetryPayload);
                    await _hub.Clients.All.SendAsync("Alerts", alertsPayload);

                    await Task.Delay(2000);
                }
            });
        }

        private static List<object> BuildAlerts(SceneDefinition scene, int latencyMs, int droppedFrames, string modelStatus)
        {
            var alerts = new List<object>();

            foreach (var detection in scene.Detections)
            {
                if (detection.Confidence >= 0.90)
                {
                    alerts.Add(new
                    {
                        severity = "Info",
                        message = $"High-confidence {detection.Label} detected ({detection.Confidence:P0})."
                    });
                }
            }

            if (latencyMs > 40)
            {
                alerts.Add(new
                {
                    severity = "Warning",
                    message = $"Latency spike detected: {latencyMs} ms."
                });
            }

            if (droppedFrames > 1)
            {
                alerts.Add(new
                {
                    severity = "Critical",
                    message = $"Dropped frames increased to {droppedFrames}."
                });
            }

            alerts.Add(new
            {
                severity = "Status",
                message = $"Model status: {modelStatus} | Scene: {scene.Source}"
            });

            return alerts;
        }

        private static SceneDefinition BuildStreetScene()
        {
            string svg = @"
<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 800 450'>
  <rect width='800' height='450' fill='#9bd0ff'/>
  <rect y='250' width='800' height='200' fill='#6fa86c'/>
  <rect y='290' width='800' height='160' fill='#444b57'/>
  <rect x='60' y='150' width='120' height='140' fill='#d9d9d9'/>
  <rect x='220' y='130' width='160' height='160' fill='#cfcfcf'/>
  <rect x='430' y='120' width='140' height='170' fill='#d7d7d7'/>
  <rect x='620' y='145' width='110' height='145' fill='#cccccc'/>
  <rect x='280' y='315' width='170' height='55' rx='10' fill='#1f3b73'/>
  <rect x='110' y='320' width='36' height='90' fill='#222'/>
  <circle cx='320' cy='370' r='22' fill='#111'/>
  <circle cx='412' cy='370' r='22' fill='#111'/>
  <rect x='560' y='315' width='42' height='92' fill='#303030'/>
  <rect x='640' y='310' width='145' height='65' rx='8' fill='#7b2020'/>
  <circle cx='678' cy='375' r='21' fill='#111'/>
  <circle cx='748' cy='375' r='21' fill='#111'/>
</svg>";
            return new SceneDefinition
            {
                Source = "Street Scene",
                ImageUrl = SvgToDataUrl(svg),
                Detections = new List<Detection>
                {
                    new Detection("Person", 0.94, 14, 71, 5, 20),
                    new Detection("Vehicle", 0.91, 35, 70, 23, 16),
                    new Detection("Person", 0.88, 70, 69, 5, 20),
                    new Detection("Vehicle", 0.93, 80, 69, 18, 16)
                }
            };
        }

        private static SceneDefinition BuildWarehouseScene()
        {
            string svg = @"
<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 800 450'>
  <rect width='800' height='450' fill='#d9d2c3'/>
  <rect y='300' width='800' height='150' fill='#7b6d5d'/>
  <rect x='90' y='140' width='120' height='160' fill='#b07d42'/>
  <rect x='230' y='110' width='130' height='190' fill='#b07d42'/>
  <rect x='390' y='160' width='110' height='140' fill='#b07d42'/>
  <rect x='535' y='125' width='120' height='175' fill='#b07d42'/>
  <rect x='670' y='145' width='85' height='155' fill='#b07d42'/>
  <rect x='455' y='310' width='120' height='70' rx='10' fill='#e5b85c'/>
  <circle cx='485' cy='385' r='18' fill='#111'/>
  <circle cx='545' cy='385' r='18' fill='#111'/>
  <rect x='300' y='315' width='35' height='88' fill='#2f2f2f'/>
</svg>";
            return new SceneDefinition
            {
                Source = "Warehouse Scene",
                ImageUrl = SvgToDataUrl(svg),
                Detections = new List<Detection>
                {
                    new Detection("Package", 0.95, 29, 24, 17, 42),
                    new Detection("Package", 0.89, 67, 28, 15, 39),
                    new Detection("Forklift", 0.92, 57, 69, 16, 18),
                    new Detection("Worker", 0.90, 37, 70, 5, 19)
                }
            };
        }

        private static SceneDefinition BuildOfficeScene()
        {
            string svg = @"
<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 800 450'>
  <rect width='800' height='450' fill='#dfe7f2'/>
  <rect y='315' width='800' height='135' fill='#9f8f81'/>
  <rect x='85' y='95' width='210' height='150' fill='#b8d4ef'/>
  <rect x='345' y='90' width='230' height='155' fill='#b8d4ef'/>
  <rect x='615' y='105' width='115' height='140' fill='#b8d4ef'/>
  <rect x='160' y='285' width='210' height='22' fill='#6f4c3e'/>
  <rect x='155' y='220' width='25' height='65' fill='#222'/>
  <rect x='240' y='225' width='75' height='48' fill='#1f2937'/>
  <rect x='465' y='285' width='170' height='22' fill='#6f4c3e'/>
  <rect x='498' y='238' width='66' height='42' fill='#111827'/>
  <rect x='585' y='240' width='32' height='45' fill='#3b82f6'/>
  <rect x='395' y='235' width='28' height='78' fill='#2f2f2f'/>
</svg>";
            return new SceneDefinition
            {
                Source = "Office Scene",
                ImageUrl = SvgToDataUrl(svg),
                Detections = new List<Detection>
                {
                    new Detection("Laptop", 0.96, 29, 50, 9, 11),
                    new Detection("Person", 0.87, 19, 49, 4, 18),
                    new Detection("Laptop", 0.94, 62, 53, 8, 10),
                    new Detection("Bottle", 0.84, 73, 53, 4, 11),
                    new Detection("Person", 0.89, 49, 52, 4, 18)
                }
            };
        }

        private static string SvgToDataUrl(string svg)
        {
            return "data:image/svg+xml;utf8," + Uri.EscapeDataString(svg);
        }

        private class SceneDefinition
        {
            public string Source { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public List<Detection> Detections { get; set; } = new();
        }

        private class Detection
        {
            public Detection(string label, double confidence, int x, int y, int width, int height)
            {
                Label = label;
                Confidence = confidence;
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }

            public string Label { get; }
            public double Confidence { get; }
            public int X { get; }
            public int Y { get; }
            public int Width { get; }
            public int Height { get; }
        }
    }
}