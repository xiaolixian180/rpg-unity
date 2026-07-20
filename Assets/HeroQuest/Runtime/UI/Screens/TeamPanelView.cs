using System;
using HeroQuest.Net.Go;
using HeroQuest.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace HeroQuest.UI.Screens
{
    public sealed class TeamPanelView : MonoBehaviour
    {
        private static readonly Color Bg = new(0.03f, 0.04f, 0.03f, 0.97f);
        private static readonly Color PanelDark = new(0.06f, 0.08f, 0.06f, 0.95f);
        private static readonly Color Border = new(0.26f, 0.42f, 0.24f, 1f);
        private static readonly Color TextLight = new(0.92f, 0.90f, 0.82f, 1f);
        private static readonly Color TextGold = new(0.78f, 0.58f, 0.22f, 1f);

        private static readonly string[] ClassLabels = { "战士", "法师", "射手", "牧师", "刺客" };
        private const int MaxMembers = 5;

        // --- Events ---
        public event Action OnCloseRequested;
        public event Action OnCreateTeamRequested;
        public event Action<ulong> OnInviteRequested;
        public event Action<ulong> OnKickRequested;
        public event Action OnLeaveTeamRequested;
        public event Action OnDismissTeamRequested;
        public event Action<ulong, bool> OnInviteReplyRequested;

        // --- No-team state ---
        private GameObject noTeamRoot;
        private Button createTeamBtn;

        // --- Has-team state ---
        private GameObject teamRoot;
        private MemberRow[] memberRows = new MemberRow[MaxMembers];
        private Button inviteBtn;
        private Button leaveBtn;
        private Button dismissBtn;

        // --- Invite notification ---
        private GameObject inviteRoot;
        private Text inviteText;
        private Button inviteAcceptBtn;
        private Button inviteRejectBtn;

        // --- Cached state ---
        private ulong currentLeaderId;
        private ulong currentPlayerId;

        public static TeamPanelView Create(Canvas parent)
        {
            var go = new GameObject("TeamPanelView", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent.transform, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(go.transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            SetAnchor(panelRect, new Vector2(0.5f, 0.5f), new Vector2(700f, 500f));
            panel.GetComponent<Image>().color = Bg;

            var view = go.AddComponent<TeamPanelView>();
            view.Build(panel.transform);
            return view;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Show empty state with a "创建队伍" button.
        /// </summary>
        public void ApplyNoTeam()
        {
            if (noTeamRoot != null) noTeamRoot.SetActive(true);
            if (teamRoot != null) teamRoot.SetActive(false);
            if (inviteRoot != null) inviteRoot.SetActive(false);
        }

        /// <summary>
        /// Populate the member list from a GoTeamInfo payload.
        /// </summary>
        public void ApplyTeamInfo(GoTeamInfo team)
        {
            if (team == null || team.members == null || team.members.Length == 0)
            {
                ApplyNoTeam();
                return;
            }

            if (noTeamRoot != null) noTeamRoot.SetActive(false);
            if (teamRoot != null) teamRoot.SetActive(true);
            if (inviteRoot != null) inviteRoot.SetActive(false);

            currentLeaderId = team.leader_id;

            // Determine whether the local player is the leader.
            // currentPlayerId is set externally; fall back to is_leader flag on members.
            bool isLeader = false;
            for (int i = 0; i < team.members.Length; i++)
            {
                if (team.members[i].player_id == currentPlayerId)
                {
                    isLeader = team.members[i].is_leader;
                    break;
                }
                if (team.members[i].is_leader && currentPlayerId == 0)
                {
                    isLeader = team.members[i].player_id == team.leader_id;
                }
            }

            for (int i = 0; i < MaxMembers; i++)
            {
                var row = memberRows[i];
                if (i < team.members.Length)
                {
                    var m = team.members[i];
                    row.root.SetActive(true);

                    string classLabel = (m.@class >= 0 && m.@class < ClassLabels.Length)
                        ? ClassLabels[m.@class]
                        : "未知";

                    row.nameText.text = m.is_leader ? $"[队长] {m.name}" : m.name;
                    row.nameText.color = m.is_leader ? TextGold : TextLight;
                    row.classText.text = classLabel;
                    row.levelText.text = $"Lv.{m.level}";
                    row.hpText.text = $"{m.hp} / {m.max_hp}";
                    row.onlineDot.color = m.online ? new Color(0.2f, 0.8f, 0.2f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f);

                    // Kick button: visible only if current user is leader AND not self.
                    bool canKick = isLeader && m.player_id != currentPlayerId;
                    row.kickBtn.gameObject.SetActive(canKick);
                    var pid = m.player_id;
                    row.kickBtn.onClick.RemoveAllListeners();
                    row.kickBtn.onClick.AddListener(() => OnKickRequested?.Invoke(pid));
                }
                else
                {
                    row.root.SetActive(false);
                }
            }

            // Bottom buttons: invite is available to leader only; dismiss is leader-only.
            if (inviteBtn != null) inviteBtn.interactable = isLeader;
            if (dismissBtn != null) dismissBtn.gameObject.SetActive(isLeader);
        }

        /// <summary>
        /// Show an incoming team invite notification with accept/reject buttons.
        /// </summary>
        public void ApplyInviteNotification(GoTeamInvitePush invite)
        {
            if (inviteRoot == null) return;
            inviteRoot.SetActive(true);

            if (inviteText != null)
                inviteText.text = $"{invite.inviter_name} 邀请你加入队伍 (当前 {invite.member_count} 人)";

            var teamId = invite.team_id;
            if (inviteAcceptBtn != null)
            {
                inviteAcceptBtn.onClick.RemoveAllListeners();
                inviteAcceptBtn.onClick.AddListener(() =>
                {
                    OnInviteReplyRequested?.Invoke(teamId, true);
                    inviteRoot.SetActive(false);
                });
            }
            if (inviteRejectBtn != null)
            {
                inviteRejectBtn.onClick.RemoveAllListeners();
                inviteRejectBtn.onClick.AddListener(() =>
                {
                    OnInviteReplyRequested?.Invoke(teamId, false);
                    inviteRoot.SetActive(false);
                });
            }
        }

        /// <summary>
        /// Set the local player ID so the view can determine leader/kick visibility.
        /// </summary>
        public void SetCurrentPlayerId(ulong playerId)
        {
            currentPlayerId = playerId;
        }

        // -------------------------------------------------------------------
        // Build
        // -------------------------------------------------------------------

        private void Build(Transform root)
        {
            // Title
            CreateText(root, "队伍", 28, new Vector2(0.5f, 0.95f), new Vector2(200f, 40f), TextLight);

            // Close button
            var closeBtn = CreateButton(root, "X", new Vector2(0.96f, 0.95f), new Vector2(40f, 40f));
            closeBtn.onClick.AddListener(() => OnCloseRequested?.Invoke());

            // --- No-team root ---
            noTeamRoot = new GameObject("NoTeam", typeof(RectTransform));
            noTeamRoot.transform.SetParent(root, false);
            SetAnchor(noTeamRoot.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(400f, 80f));

            createTeamBtn = CreateButton(noTeamRoot.transform, "创建队伍", new Vector2(0.5f, 0.5f), new Vector2(180f, 48f));
            createTeamBtn.onClick.AddListener(() => OnCreateTeamRequested?.Invoke());

            // --- Team root ---
            teamRoot = new GameObject("Team", typeof(RectTransform));
            teamRoot.transform.SetParent(root, false);
            var teamRect = teamRoot.GetComponent<RectTransform>();
            teamRect.anchorMin = Vector2.zero;
            teamRect.anchorMax = Vector2.one;
            teamRect.offsetMin = Vector2.zero;
            teamRect.offsetMax = Vector2.zero;
            teamRoot.SetActive(false);

            // Member list background
            var listBg = new GameObject("ListBg", typeof(RectTransform), typeof(Image));
            listBg.transform.SetParent(teamRoot.transform, false);
            SetAnchor(listBg.GetComponent<RectTransform>(), new Vector2(0.5f, 0.55f), new Vector2(620f, 340f));
            listBg.GetComponent<Image>().color = PanelDark;

            // Member rows
            for (int i = 0; i < MaxMembers; i++)
            {
                float y = 0.88f - i * 0.20f;
                memberRows[i] = BuildMemberRow(listBg.transform, y);
                memberRows[i].root.SetActive(false);
            }

            // Bottom buttons
            float btnY = 0.08f;
            inviteBtn = CreateButton(teamRoot.transform, "邀请", new Vector2(0.25f, btnY), new Vector2(100f, 40f));
            inviteBtn.onClick.AddListener(() => OnInviteRequested?.Invoke(currentPlayerId));

            leaveBtn = CreateButton(teamRoot.transform, "离开", new Vector2(0.50f, btnY), new Vector2(100f, 40f));
            leaveBtn.onClick.AddListener(() => OnLeaveTeamRequested?.Invoke());

            dismissBtn = CreateButton(teamRoot.transform, "解散", new Vector2(0.75f, btnY), new Vector2(100f, 40f));
            dismissBtn.onClick.AddListener(() => OnDismissTeamRequested?.Invoke());
            dismissBtn.gameObject.SetActive(false);

            // --- Invite notification root ---
            inviteRoot = new GameObject("Invite", typeof(RectTransform), typeof(Image));
            inviteRoot.transform.SetParent(root, false);
            SetAnchor(inviteRoot.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(420f, 140f));
            inviteRoot.GetComponent<Image>().color = PanelDark;
            inviteRoot.SetActive(false);

            inviteText = CreateText(inviteRoot.transform, "", 18, new Vector2(0.5f, 0.72f), new Vector2(380f, 30f), TextLight);

            inviteAcceptBtn = CreateButton(inviteRoot.transform, "接受", new Vector2(0.30f, 0.28f), new Vector2(90f, 36f));
            inviteRejectBtn = CreateButton(inviteRoot.transform, "拒绝", new Vector2(0.70f, 0.28f), new Vector2(90f, 36f));
        }

        private static MemberRow BuildMemberRow(Transform parent, float anchorY)
        {
            var rowGo = new GameObject("MemberRow", typeof(RectTransform), typeof(Image));
            rowGo.transform.SetParent(parent, false);
            var rowRect = rowGo.GetComponent<RectTransform>();
            SetAnchor(rowRect, new Vector2(0.5f, anchorY), new Vector2(580f, 52f));
            rowGo.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.08f, 0.6f);

            var row = new MemberRow();
            row.root = rowGo;

            // Name
            row.nameText = CreateText(rowGo.transform, "", 17, new Vector2(0.18f, 0.5f), new Vector2(160f, 28f), TextLight);

            // Class
            row.classText = CreateText(rowGo.transform, "", 15, new Vector2(0.38f, 0.5f), new Vector2(70f, 26f), TextLight);

            // Level
            row.levelText = CreateText(rowGo.transform, "", 15, new Vector2(0.50f, 0.5f), new Vector2(60f, 26f), TextLight);

            // HP
            row.hpText = CreateText(rowGo.transform, "", 14, new Vector2(0.65f, 0.5f), new Vector2(100f, 26f), TextLight);

            // Online dot
            var dotGo = new GameObject("OnlineDot", typeof(RectTransform), typeof(Image));
            dotGo.transform.SetParent(rowGo.transform, false);
            SetAnchor(dotGo.GetComponent<RectTransform>(), new Vector2(0.80f, 0.5f), new Vector2(12f, 12f));
            dotGo.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f, 1f);
            row.onlineDot = dotGo.GetComponent<Image>();

            // Kick button
            row.kickBtn = CreateButton(rowGo.transform, "踢出", new Vector2(0.92f, 0.5f), new Vector2(52f, 28f));
            row.kickBtn.gameObject.SetActive(false);

            return row;
        }

        // -------------------------------------------------------------------
        // Helpers (same pattern as CharacterPanelView)
        // -------------------------------------------------------------------

        private static Text CreateText(Transform parent, string content, float size, Vector2 anchor, Vector2 sizeDelta, Color color)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var label = go.GetComponent<Text>();
            label.text = content;
            label.font = ChineseFontProvider.GetFont();
            label.fontSize = Mathf.RoundToInt(size);
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            SetAnchor(label.rectTransform, anchor, sizeDelta);
            return label;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 anchor, Vector2 sizeDelta)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0.30f, 0.42f, 0.30f, 1f);
            SetAnchor(go.GetComponent<RectTransform>(), anchor, sizeDelta);

            var text = CreateText(go.transform, label, 14, new Vector2(0.5f, 0.5f), sizeDelta, TextLight);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            return go.GetComponent<Button>();
        }

        private static void SetAnchor(RectTransform rt, Vector2 anchor, Vector2 sizeDelta)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = sizeDelta;
        }

        // -------------------------------------------------------------------
        // Inner types
        // -------------------------------------------------------------------

        private sealed class MemberRow
        {
            public GameObject root;
            public Text nameText;
            public Text classText;
            public Text levelText;
            public Text hpText;
            public Image onlineDot;
            public Button kickBtn;
        }
    }
}
