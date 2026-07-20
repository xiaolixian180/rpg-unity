using System.Threading;
using HeroQuest.Core;
using HeroQuest.Net.Go;
using UnityEngine;

namespace HeroQuest.Systems.World
{
    public sealed class PrototypeNetworkClient : MonoBehaviour
    {
        [SerializeField] private float moveSyncInterval = 0.25f;
        [SerializeField] private TopDownPlayerController playerController;

        private NetworkManager network;
        private float syncTimer;
        private double lastSentX;
        private double lastSentY;
        private const double MoveThreshold = 0.01;

        public void SetPlayerController(TopDownPlayerController controller)
        {
            playerController = controller;
        }

        private void Start()
        {
            if (playerController == null)
            {
                playerController = FindFirstObjectByType<TopDownPlayerController>();
            }

            if (ServiceRegistry.TryResolve<NetworkManager>(out var nm))
            {
                network = nm;
                network.PlayerMove += OnPlayerMove;
            }
        }

        private async void Update()
        {
            if (network == null || !network.IsConnected || playerController == null)
            {
                return;
            }

            syncTimer -= Time.deltaTime;
            if (syncTimer > 0f)
            {
                return;
            }

            syncTimer = moveSyncInterval;

            var pos = playerController.transform.position;
            var dx = (double)pos.x - lastSentX;
            var dy = (double)pos.y - lastSentY;
            if (dx * dx + dy * dy < MoveThreshold * MoveThreshold)
            {
                return;
            }

            lastSentX = (double)pos.x;
            lastSentY = (double)pos.y;
            await network.SendMoveAsync(lastSentX, lastSentY, CancellationToken.None);
        }

        private void OnDestroy()
        {
            if (network != null)
            {
                network.PlayerMove -= OnPlayerMove;
            }
        }

        private void OnPlayerMove(ulong pid, double x, double y)
        {
            // TODO: update other player positions in the world
            // For now, log movement of other players
            Debug.Log($"[Net] Player {pid} moved to ({x:F2}, {y:F2})");
        }
    }
}
