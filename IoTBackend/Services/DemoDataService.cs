using IoTBackend.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace IoTBackend.Services
{
    public class DemoDataService
    {
        private readonly IHubContext<FactoryHub> _hubContext;
        private readonly ILogger<DemoDataService> _logger;
        private CancellationTokenSource _cancellationTokenSource;

        private decimal _temperature = 20m;
        private bool _smoke = false;
        private decimal _weight = 0m;
        private string _productNumber = "";

        public DemoDataService(IHubContext<FactoryHub> hubContext, ILogger<DemoDataService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task StartDemoAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            _ = Task.Run(async () =>
            {
                var random = new Random();
                int cycle = 0;

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        cycle++;

                        // Simulate temperature fluctuations
                        _temperature += (decimal)(random.NextDouble() - 0.5) * 2;
                        _temperature = Math.Max(15, Math.Min(45, _temperature));
                        await _hubContext.Clients.All.SendAsync("ReceiveTemperatureUpdate", Math.Round(_temperature, 1), cancellationToken);

                        await Task.Delay(3000, cancellationToken);

                        // Simulate smoke detection (random spike)
                        _smoke = random.Next(0, 20) == 1;
                        await _hubContext.Clients.All.SendAsync("ReceiveSmokeUpdate", _smoke, cancellationToken);

                        await Task.Delay(2000, cancellationToken);

                        // Simulate weight changes
                        if (cycle % 5 == 0)
                        {
                            _weight = random.Next(5, 50) + (decimal)random.NextDouble();
                        }
                        await _hubContext.Clients.All.SendAsync("ReceiveWeightUpdate", Math.Round(_weight, 2), cancellationToken);

                        await Task.Delay(4000, cancellationToken);

                        // Simulate product numbers
                        if (cycle % 8 == 0)
                        {
                            _productNumber = $"PROD-{random.Next(1000, 9999)}";
                            await _hubContext.Clients.All.SendAsync("ReceiveProductNumberUpdate", _productNumber, cancellationToken);
                        }

                        await Task.Delay(5000, cancellationToken);

                        _logger.LogInformation($"Demo Cycle {cycle}: Temp={_temperature:F1}°C, Smoke={_smoke}, Weight={_weight:F2}kg, Product={_productNumber}");
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error in demo data stream: {ex.Message}");
                        await Task.Delay(5000, cancellationToken);
                    }
                }
            }, cancellationToken);

            await Task.CompletedTask;
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
