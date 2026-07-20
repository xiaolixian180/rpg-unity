using System;
using System.Threading;
using System.Threading.Tasks;
using HeroQuest.Domain;
using HeroQuest.Net.Go;
using UnityEngine;

namespace HeroQuest.Net.Auth
{
    public sealed class LocalTestAuthService : IAuthService
    {
        public const string TestAccount = "test";
        public const string TestPassword = "test";
        private const ulong TestPlayerId = 100001;
        private const int ResponseTimeoutMs = 10000;

        private readonly NetworkManager network;

        public LocalTestAuthService(NetworkManager network)
        {
            this.network = network;
        }

        public async Task<AuthResult> LoginAsync(string account, string password, CancellationToken cancellationToken)
        {
            if (account != TestAccount || password != TestPassword)
            {
                return new AuthResult(false, string.Empty, "账号或密码错误");
            }

            try
            {
                if (!network.IsConnected)
                {
                    await network.ConnectAsync(cancellationToken);
                }

                var token = JwtHelper.GenerateToken(TestPlayerId);

                var tcs = new TaskCompletionSource<(uint code, GoPlayerData player)>();
                Action<uint, GoPlayerData> onResult = null;
                onResult = (code, player) =>
                {
                    network.LoginResult -= onResult;
                    tcs.TrySetResult((code, player));
                };
                network.LoginResult += onResult;

                await network.SendLoginAsync(token, cancellationToken);

                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(ResponseTimeoutMs);
                var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(-1, timeoutCts.Token));

                if (completedTask != tcs.Task)
                {
                    network.LoginResult -= onResult;
                    return new AuthResult(false, string.Empty, "登录超时，请检查服务器连接");
                }

                var result = await tcs.Task;
                if (result.code != 0)
                {
                    // Player doesn't exist, create one
                    var createTcs = new TaskCompletionSource<(uint code, GoPlayerData player)>();
                    Action<uint, GoPlayerData> onCreateResult = null;
                    onCreateResult = (code, player) =>
                    {
                        network.CreatePlayerResult -= onCreateResult;
                        createTcs.TrySetResult((code, player));
                    };
                    network.CreatePlayerResult += onCreateResult;

                    await network.SendCreatePlayerAsync(token, TestAccount, (int)CharacterClass.Warrior, cancellationToken);

                    var createCompleted = await Task.WhenAny(createTcs.Task, Task.Delay(-1, timeoutCts.Token));
                    if (createCompleted != createTcs.Task)
                    {
                        network.CreatePlayerResult -= onCreateResult;
                        return new AuthResult(false, string.Empty, "创建角色超时");
                    }

                    var createResult = await createTcs.Task;
                    if (createResult.code != 0)
                    {
                        return new AuthResult(false, string.Empty, $"创建角色失败: code={createResult.code}");
                    }

                    return new AuthResult(true, createResult.player.id.ToString(), "登录成功，角色已创建");
                }

                return new AuthResult(true, result.player.id.ToString(), "登录成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LocalTestAuthService] Login error: {ex}");
                return new AuthResult(false, string.Empty, $"连接失败: {ex.Message}");
            }
        }
    }
}
