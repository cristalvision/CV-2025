using CV_2025.wwwroot.Process;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace CV_2025.Home
{
    public class Home : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public sealed class WebsocketController : ControllerBase
    {
        Process process = new Process();
        public async Task SendReceiveAsyncData()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                byte[] buffer = new byte[1024];
                process.webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                process.result = await process.webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                while (!process.result.CloseStatus.HasValue)
                {
                    string JSONString = Encoding.UTF8.GetString(buffer, 0, process.result.Count);
                    JsonElement[] request = JsonSerializer.Deserialize<JsonElement[]>(JSONString);

                    try
                    {
                        ExecuteAction(request);
                    }
                    catch (Exception ex)
                    {
                        await process.UpdateProgress("Exception", ex.Message);
                    }

                    process.result = await process.webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }//This loop keeps connection opened | Because function is async breaks point here for send/receive

                await process.webSocket.CloseAsync(process.result.CloseStatus.Value, process.result.CloseStatusDescription, CancellationToken.None);
            }
        }

        private async Task ExecuteAction(JsonElement[] request)
        {
            string? action = request[0].GetString();

            switch (action)
            {
                case "Start":
                    await process.Start(request[1].ToString());
                    break;
                case "DisplayUnwnownChar":
                    await process.DisplayUnwnownChar();
                    break;
                case "UpdateDatabase":
                    JsonElement jsonElement = request[1];
                    bool style = request[1].GetBoolean();
                    char value = request[2].ToString()[0];
                    int DBWidth = request[3].GetInt32();

                    await process.UpdateDatabase(style, value, DBWidth);
                    break;
            }
        }
    }
}
