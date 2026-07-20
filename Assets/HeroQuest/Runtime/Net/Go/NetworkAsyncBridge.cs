using System;
using System.Threading;
using System.Threading.Tasks;
using HeroQuest.Domain;
using UnityEngine;

namespace HeroQuest.Net.Go
{
    /// <summary>
    /// 异步桥接器：将 NetworkManager 的事件驱动模式桥接为 async/await 模式。
    /// 每个 Service 实现类持有一个桥接器实例，通过 RequestAsync 发送请求并等待对应事件响应。
    /// </summary>
    /// <remarks>
    /// 限制：同一时间同一事件类型只支持一个未完成请求（协议无 request_id 关联）。
    /// 若需并发同类型请求，需协议层增加 request_id 字段。
    /// 超时默认 10 秒，超时后返回 NetworkTimeout。
    /// </remarks>
    internal sealed class NetworkAsyncBridge
    {
        private const int DefaultTimeoutMs = 10000;

        private readonly NetworkManager networkManager;

        public NetworkAsyncBridge(NetworkManager networkManager)
        {
            this.networkManager = networkManager ?? throw new ArgumentNullException(nameof(networkManager));
        }

        public bool IsConnected => networkManager.IsConnected;

        /// <summary>
        /// 发送网络请求并等待对应的事件响应，返回包裹了 Go 协议响应体的 ServiceResult。
        /// 调用方负责将 ServiceResult.Value（Go 响应体）映射为业务域模型。
        /// </summary>
        /// <typeparam name="TResponse">Go 协议响应体类型（如 GoRankingListResponse）</typeparam>
        /// <param name="sendFunc">发送网络请求的委托</param>
        /// <param name="subscribe">订阅响应事件的委托</param>
        /// <param name="unsubscribe">取消订阅响应事件的委托</param>
        /// <param name="ct">取消令牌</param>
        /// <returns>成功时 Value 为反序列化后的响应体；失败时 ErrorCode 指示原因</returns>
        public async Task<ServiceResult<TResponse>> RequestAsync<TResponse>(
            Func<CancellationToken, Task> sendFunc,
            Action<Action<TResponse>> subscribe,
            Action<Action<TResponse>> unsubscribe,
            CancellationToken ct = default) where TResponse : class
        {
            if (!networkManager.IsConnected)
            {
                return ServiceResult<TResponse>.Fail(GameErrorCode.NetworkDisconnected);
            }

            var tcs = new TaskCompletionSource<TResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
            void OnResponse(TResponse response) => tcs.TrySetResult(response);

            subscribe(OnResponse);
            try
            {
                await sendFunc(ct);

                var delayTask = Task.Delay(DefaultTimeoutMs, ct);
                var completed = await Task.WhenAny(tcs.Task, delayTask);

                if (completed != tcs.Task)
                {
                    // 超时或取消
                    return ServiceResult<TResponse>.Fail(GameErrorCode.NetworkTimeout);
                }

                return ServiceResult<TResponse>.Success(await tcs.Task);
            }
            catch (OperationCanceledException)
            {
                return ServiceResult<TResponse>.Fail(GameErrorCode.NetworkTimeout);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkAsyncBridge] RequestAsync<{typeof(TResponse).Name}> failed: {ex}");
                return ServiceResult<TResponse>.Fail(GameErrorCode.NetworkSendFailed, ex.Message);
            }
            finally
            {
                unsubscribe(OnResponse);
            }
        }

        /// <summary>
        /// 将服务端返回的 code (uint32) 映射为 GameErrorCode。
        /// code == 0 → Success；已知码 → 直接转换；未知码 → InternalError 兜底。
        /// </summary>
        public static GameErrorCode MapServerCode(uint code)
        {
            if (code == 0)
            {
                return GameErrorCode.Success;
            }

            var errorCode = (GameErrorCode)code;
            return Enum.IsDefined(typeof(GameErrorCode), errorCode)
                ? errorCode
                : GameErrorCode.InternalError;
        }
    }
}
