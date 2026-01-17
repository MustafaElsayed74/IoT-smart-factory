using MQTTnet;
using MQTTnet.Client;
using System.Text.Json;
using System.Text;
using IoTBackend.Models;
using IoTBackend.Data;
using IoTBackend.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace IoTBackend.Services
{
    public class MqttService
    {
        private readonly IMqttClient _mqttClient;
        private readonly IHubContext<FactoryHub> _hubContext;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<MqttService> _logger;

        public MqttService(IMqttClient mqttClient, IHubContext<FactoryHub> hubContext, AppDbContext dbContext, ILogger<MqttService> logger)
        {
            _mqttClient = mqttClient;
            _hubContext = hubContext;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task StartAsync()
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("localhost", 1883)
                .Build();

            _mqttClient.DisconnectedAsync += async e =>
            {
                _logger.LogWarning("MQTT disconnected");
                await Task.Delay(5000);
                await _mqttClient.ConnectAsync(options);
            };

            _mqttClient.ApplicationMessageReceivedAsync += HandleMessageAsync;

            await _mqttClient.ConnectAsync(options);

            var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter("factory/temperature")
                .WithTopicFilter("factory/smoke")
                .WithTopicFilter("factory/weight")
                .WithTopicFilter("factory/product")
                .Build();

            await _mqttClient.SubscribeAsync(subscribeOptions);
            _logger.LogInformation("MQTT subscribed to factory topics");
        }

        private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                string topic = e.ApplicationMessage.Topic;

                var data = new Dictionary<string, object>();

                if (topic == "factory/temperature")
                {
                    var json = JsonDocument.Parse(payload);
                    decimal temperature = json.RootElement.GetProperty("value").GetDecimal();
                    await _hubContext.Clients.All.SendAsync("ReceiveTemperatureUpdate", temperature);
                }
                else if (topic == "factory/smoke")
                {
                    var json = JsonDocument.Parse(payload);
                    bool smoke = json.RootElement.GetProperty("value").GetBoolean();
                    await _hubContext.Clients.All.SendAsync("ReceiveSmokeUpdate", smoke);
                }
                else if (topic == "factory/weight")
                {
                    var json = JsonDocument.Parse(payload);
                    decimal weight = json.RootElement.GetProperty("value").GetDecimal();
                    await _hubContext.Clients.All.SendAsync("ReceiveWeightUpdate", weight);

                    var reading = new Reading { Weight = weight, Timestamp = DateTime.UtcNow };
                    _dbContext.Readings.Add(reading);
                    await _dbContext.SaveChangesAsync();
                }
                else if (topic == "factory/product")
                {
                    var json = JsonDocument.Parse(payload);
                    string productNumber = json.RootElement.GetProperty("value").GetString();
                    await _hubContext.Clients.All.SendAsync("ReceiveProductNumberUpdate", productNumber);

                    var reading = new Reading { ProductNumber = productNumber, Timestamp = DateTime.UtcNow };
                    _dbContext.Readings.Add(reading);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing MQTT message: {ex.Message}");
            }
        }
    }
}
