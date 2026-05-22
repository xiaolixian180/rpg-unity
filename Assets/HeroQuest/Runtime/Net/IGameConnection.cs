using System;
using System.Threading;
using System.Threading.Tasks;

namespace HeroQuest.Net
{
    public interface IGameConnection : IDisposable
    {
        bool IsConnected { get; }
        event Action<string> MessageReceived;
        Task ConnectAsync(Uri serverUri, CancellationToken cancellationToken);
        Task SendAsync(string payload, CancellationToken cancellationToken);
        Task DisconnectAsync();
    }
}
