using System;
using System.Threading;
using System.Threading.Tasks;
using HeroQuest.Domain;
using HeroQuest.Net.Go;

namespace HeroQuest.Systems.Dungeon
{
    /// <summary>
    /// 副本资源服务——真实网络实现。
    /// 采集资源节点通过 NetworkManager 发送 CollectResource 请求。
    /// CollectResult 事件签名为 Action&lt;uint, GoCollectResult&gt;（双参数），
    /// 无法直接使用 NetworkAsyncBridge.RequestAsync，改用手动 TaskCompletionSource。
    /// </summary>
    public sealed class DungeonResourceService : IDungeonResourceService
    {
        private const int DefaultTimeoutMs = 10000;
        private readonly NetworkManager networkManager;

        public DungeonResourceService(NetworkManager networkManager)
        {
            this.networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
        }

        public Task<ServiceResult<DungeonResourceNode>> GetNode(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                return Task.FromResult(ServiceResult<DungeonResourceNode>.Fail(GameErrorCode.InvalidParameter));
            }

            // 无单节点查询协议：资源节点随副本进入响应下发
            return Task.FromResult(ServiceResult<DungeonResourceNode>.Success(new DungeonResourceNode
            {
                nodeId = nodeId,
                isCollected = false
            }));
        }

        public async Task<ServiceResult> Collect(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter);
            }

            if (!ulong.TryParse(nodeId, out var resourceId))
            {
                return ServiceResult.Fail(GameErrorCode.InvalidParameter, $"Invalid nodeId: {nodeId}");
            }

            if (!networkManager.IsConnected)
            {
                return ServiceResult.Fail(GameErrorCode.NetworkDisconnected);
            }

            var tcs = new TaskCompletionSource<uint>(TaskCreationOptions.RunContinuationsAsynchronously);
            void OnCollectResult(uint code, GoCollectResult result) => tcs.TrySetResult(code);

            networkManager.CollectResult += OnCollectResult;
            try
            {
                await networkManager.SendCollectResourceAsync(resourceId, CancellationToken.None);

                var delayTask = Task.Delay(DefaultTimeoutMs);
                var completed = await Task.WhenAny(tcs.Task, delayTask);

                if (completed != tcs.Task)
                {
                    return ServiceResult.Fail(GameErrorCode.NetworkTimeout);
                }

                var code = await tcs.Task;
                if (code != 0)
                {
                    return ServiceResult.Fail(NetworkAsyncBridge.MapServerCode(code));
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail(GameErrorCode.NetworkSendFailed, ex.Message);
            }
            finally
            {
                networkManager.CollectResult -= OnCollectResult;
            }
        }
    }
}
