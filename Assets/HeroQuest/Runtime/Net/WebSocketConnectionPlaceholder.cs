using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HeroQuest.Net
{
    public sealed class WebSocketConnectionPlaceholder : IGameConnection
    {
        public bool IsConnected { get; private set; }
        public event Action<string> MessageReceived;

        public Task ConnectAsync(Uri serverUri, CancellationToken cancellationToken)
        {
            Debug.Log($"WebSocket placeholder connected to {serverUri}");
            IsConnected = true;
            return Task.CompletedTask;
        }

        public Task SendAsync(string payload, CancellationToken cancellationToken)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Cannot send before the connection is established.");
            }

            Debug.Log($"WebSocket placeholder sent: {payload}");
            MessageReceived?.Invoke(payload);
            return Task.CompletedTask;
        }

        public Task DisconnectAsync()
        {
            IsConnected = false;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            IsConnected = false;
        }
    }
}
