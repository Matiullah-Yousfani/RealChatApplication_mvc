using System.Net.WebSockets;
using System.Text;

namespace RealChatApplication.Models
{
    public class WebSocketChatHandler
    {
        public static readonly List<WebSocket> _sockets = new List<WebSocket>();

        public async Task HandleAsync(HttpContext context, WebSocket webSocket)
        {

            // Add the new WebSocket to the list of connected sockets
            _sockets.Add(webSocket);

            var buffer = new byte[1024 * 4];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        // Remove the WebSocket from the list and close it
                        _sockets.Remove(webSocket);
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the WebSocket client", CancellationToken.None);
                    }
                    else
                    {
                        var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                        var messageBytes = System.Text.Encoding.UTF8.GetBytes(message);

                        // Broadcast the message to all connected WebSockets
                        foreach (var socket in _sockets)
                        {
                            if (socket.State == WebSocketState.Open)
                            {
                                await socket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                // Ensure the WebSocket is removed if an error occurs
                _sockets.Remove(webSocket);
            }
        }
    }
}
