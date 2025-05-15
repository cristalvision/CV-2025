var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

var app = builder.Build();
app.UseStaticFiles();
app.UseWebSockets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");


/*app.Map("/WebSocket", async (context) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
        //await WebSocketHander.ReceiveData(context, webSocket);
        var array = new byte[] { 65, 66, 67, 68, 69 };
        var segment = new ArraySegment<byte>(array, 0, 5);

        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!result.CloseStatus.HasValue)
        {

        }//This loop keeps connection opened

        //await webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
    }

});*/

app.Run();
