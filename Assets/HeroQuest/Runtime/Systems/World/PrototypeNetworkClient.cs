using System;
using System.Threading;
using HeroQuest.Net;
using HeroQuest.Net.Protocol;
using UnityEngine;

namespace HeroQuest.Systems.World
{
    public sealed class PrototypeNetworkClient : MonoBehaviour
    {
        [SerializeField] private string playerId = "local-player";
        [SerializeField] private string serverUrl = "ws://127.0.0.1:8080/ws";
        [SerializeField] private float moveSyncInterval = 0.25f;
        [SerializeField] private TopDownPlayerController playerController;

        private IGameConnection connection;
        private float syncTimer;

        public void SetPlayerController(TopDownPlayerController controller)
        {
            playerController = controller;
        }

        private async void Start()
        {
            if (playerController == null)
            {
                playerController = FindObjectOfType<TopDownPlayerController>();
            }

            connection = new WebSocketConnectionPlaceholder();
            connection.MessageReceived += OnMessageReceived;
            await connection.ConnectAsync(new Uri(serverUrl), CancellationToken.None);
            await connection.SendAsync(GameplayProtocolCodec.EncodeHello(playerId), CancellationToken.None);
        }

        private async void Update()
        {
            if (connection == null || !connection.IsConnected || playerController == null)
            {
                return;
            }

            syncTimer -= Time.deltaTime;
            if (syncTimer > 0f)
            {
                return;
            }

            syncTimer = moveSyncInterval;
            await connection.SendAsync(
                GameplayProtocolCodec.EncodeMove(playerId, playerController.transform.position, playerController.MoveInput),
                CancellationToken.None);
        }

        private void OnDestroy()
        {
            if (connection == null)
            {
                return;
            }

            connection.MessageReceived -= OnMessageReceived;
            connection.Dispose();
            connection = null;
        }

        private void OnMessageReceived(string payload)
        {
            if (payload.Contains("\"spawn_monster\"", StringComparison.Ordinal))
            {
                var message = GameplayProtocolCodec.DecodeMonsterSpawn(payload);
                Debug.Log($"Monster spawn from server: {message.monsterId} at ({message.positionX}, {message.positionY})");
            }
        }
    }
}
