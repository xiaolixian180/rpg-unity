using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace HeroQuest.Net.Go
{
    public sealed class GoWebSocketConnection : IDisposable
    {
        private const int ReceiveBufferSize = 8192;

        private ClientWebSocket webSocket;
        private CancellationTokenSource receiveCancellation;
        private Task receiveTask;

        public bool IsConnected => webSocket != null && webSocket.State == WebSocketState.Open;
        public event Action<GoProtocolFrame> MessageReceived;
        public event Action<Exception> ConnectionError;

        public async Task ConnectAsync(Uri serverUri, CancellationToken cancellationToken)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            throw new PlatformNotSupportedException("ClientWebSocket is not supported on Unity WebGL builds.");
#else
            if (IsConnected)
            {
                return;
            }

            webSocket = new ClientWebSocket();
            await webSocket.ConnectAsync(serverUri, cancellationToken).ConfigureAwait(false);

            receiveCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            receiveTask = ReceiveLoopAsync(receiveCancellation.Token);
#endif
        }

        public Task SendJsonAsync<T>(ushort messageId, T payload, CancellationToken cancellationToken)
        {
            return SendFrameAsync(GoBinaryProtocolCodec.EncodeJson(messageId, payload), cancellationToken);
        }

        public Task SendAsync(ushort messageId, byte[] body, CancellationToken cancellationToken)
        {
            return SendFrameAsync(GoBinaryProtocolCodec.Encode(messageId, body), cancellationToken);
        }

        public async Task DisconnectAsync()
        {
            receiveCancellation?.Cancel();

            if (webSocket != null)
            {
                if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "client disconnect", CancellationToken.None)
                        .ConfigureAwait(false);
                }

                webSocket.Dispose();
                webSocket = null;
            }

            if (receiveTask != null)
            {
                try
                {
                    await receiveTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                }
                catch (ObjectDisposedException)
                {
                }

                receiveTask = null;
            }

            receiveCancellation?.Dispose();
            receiveCancellation = null;
        }

        public void Dispose()
        {
            receiveCancellation?.Cancel();
            webSocket?.Dispose();
            receiveCancellation?.Dispose();
            webSocket = null;
            receiveCancellation = null;
            receiveTask = null;
        }

        private async Task SendFrameAsync(byte[] frame, CancellationToken cancellationToken)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Cannot send before the Go WebSocket connection is established.");
            }

            await webSocket.SendAsync(
                new ArraySegment<byte>(frame),
                WebSocketMessageType.Binary,
                true,
                cancellationToken).ConfigureAwait(false);
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[ReceiveBufferSize];

            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                try
                {
                    using var message = new MemoryStream();
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken)
                            .ConfigureAwait(false);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            return;
                        }

                        message.Write(buffer, 0, result.Count);
                    }
                    while (!result.EndOfMessage);

                    if (result.MessageType != WebSocketMessageType.Binary)
                    {
                        continue;
                    }

                    var data = message.ToArray();
                    if (GoBinaryProtocolCodec.TryDecode(data, out var frame))
                    {
                        MessageReceived?.Invoke(frame);
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    ConnectionError?.Invoke(ex);
                    return;
                }
            }
        }
    }
}
