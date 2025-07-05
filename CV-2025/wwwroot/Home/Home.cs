using CV_2025.wwwroot.Process;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Versioning;
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

    [SupportedOSPlatform("windows")]
    [Route("api/[controller]")]
    [ApiController]
    public sealed class WebsocketController : ControllerBase
    {
        readonly Process process = new();
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
            }
        }
    }
}
