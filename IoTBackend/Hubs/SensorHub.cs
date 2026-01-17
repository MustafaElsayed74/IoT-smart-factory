using Microsoft.AspNetCore.SignalR;

namespace IoTBackend.Hubs
{
    public class SensorHub : Hub
    {
        public async Task SendSensorDataAsync(Dictionary<string, object> data)
        {
            await Clients.All.SendAsync("ReceiveSensorData", data);
        }
    }
}
