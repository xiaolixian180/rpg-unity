using System;
using System.Collections.Generic;
using System.Linq;
using HeroQuest.Net.Go;
using HeroQuest.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace HeroQuest.UI.Screens
{
    public sealed class ChatPanelView : MonoBehaviour
    {
        public const int ChannelWorld = 1;
        public const int ChannelPrivate = 2;
        public const int ChannelTeam = 3;

        private const int MaxMessages = 100;
        private const float RowHeight = 28f;
        private static readonly Vector2 PanelSize = new Vector2(600f, 450f);

        private static readonly Color Bg = new Color(0.03f, 0.04f, 0.03f, 0.97f);
        private static readonly Color PanelDark = new Color(0.06f, 0.08f, 0.06f, 0.95f);
        private static readonly Color Border = new Color(0.26f, 0.42f, 0.24f, 1f);
        private static readonly Color TextLight = new Color(0.92f, 0.90f, 0.82f, 1f);
        private static readonly Color TextGold = new Color(0.78f, 0.58f, 0.22f, 1f);

        private static readonly Color TagWorld = Color.white;
        private static readonly Color TagTeam = new Color(0.2f, 0.8f, 0.2f, 1f);
        private static readonly Color TagPrivate = new Color(0.9f, 0.4f, 0.6f, 1f);

        private static readonly string[] ChannelNames = { "", "世界", "私聊", "队伍" };
        private static readonly string[] ChannelTags = { "", "世界", "私", "队伍" };

        // --- Events ---
        public event Action OnCloseRequested;
        public event Action<int, ulong, string> OnSendRequested;
        public event Action<int, int> OnHistoryRequested;
        public event Action<ulong, string> OnPrivateChatRequested;

        // --- State ---
        private ulong currentPlayerID;
        private int currentChannel = ChannelWorld;
        private ulong privateTargetID;
        private string privateTargetName;
        private readonly List<ChatEntry> messages = new List<ChatEntry>();

        // --- UI references ---
        private readonly Button[] tabBtns = new Button[4];
        private readonly Image[] tabImgs = new Image[4];
        private Text channelIndicator;
        private Text messageText;
        private ScrollRect scrollRect;
        private RectTransform contentRect;
        private InputField inputField;

        // --- Data ---
        private struct ChatEntry
        {
            public int channel;
            public ulong senderID;
            public string senderName;
            public ulong targetID;
            public string content;
            public long timestamp;
        }

        // ================================================================
        // Factory
        // ================================================================

        public static ChatPanelView Create(Canvas parent)
        {
            // Full-screen overlay
            var overlayGO = new GameObject("ChatPanelView",
                typeof(RectTransform), typeof(Image));
            overlayGO.transform.SetParent(parent.transform, false);
            var overlayRT = overlayGO.GetComponent<RectTransform>();
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = Vector2.zero;
            overlayRT.offsetMax = Vector2.zero;
            overlayGO.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

            // Panel – anchored bottom-left
            var panelGO = new GameObject("Panel",
                typeof(RectTransform), typeof(Image));
            panelGO.transform.SetParent(overlayGO.transform, false);
            var panelRT = panelGO.GetComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.zero;
            panelRT.pivot = new Vector2(0f, 0f);
            panelRT.anchoredPosition = new Vector2(10f, 10f);
            panelRT.sizeDelta = PanelSize;
            panelGO.GetComponent<Image>().color = Bg;

            var view = overlayGO.AddComponent<ChatPanelView>();
            view.Build(panelGO.transform);
            return view;
        }

        // ================================================================
        // Build UI
        // ================================================================

        private void Build(Transform root)
        {
            // ---- Title bar ----
            var titleBg = CreatePanel(root, new Vector2(0f, 1f),
                new Vector2(10f, -20f), new Vector2(580f, 40f));

            CreateText(titleBg.transform, "聊天", 22,
                new Vector2(0.5f, 0.5f), Vector2.zero,
                TextGold, true, true);

            var closeBtn = CreateButton(root, "X",
                new Vector2(1f, 1f), new Vector2(-25f, -20f),
                new Vector2(36f, 36f));
            closeBtn.onClick.AddListener(() => OnCloseRequested?.Invoke());

            // ---- Channel tabs (世界 / 私聊 / 队伍) ----
            var tabs = new GameObject("Tabs",
                typeof(RectTransform), typeof(Image),
                typeof(HorizontalLayoutGroup));
            tabs.transform.SetParent(root, false);
            var tabsRT = tabs.GetComponent<RectTransform>();
            tabsRT.anchorMin = new Vector2(0f, 1f);
            tabsRT.anchorMax = new Vector2(0f, 1f);
            tabsRT.pivot = new Vector2(0.5f, 0.5f);
            tabsRT.anchoredPosition = new Vector2(10f, -60f);
            tabsRT.sizeDelta = new Vector2(580f, 36f);
            tabs.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            var hlg = tabs.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            for (int ch = 1; ch <= 3; ch++)
            {
                var tabGO = new GameObject("Tab_" + ChannelNames[ch],
                    typeof(RectTransform), typeof(Image), typeof(Button));
                tabGO.transform.SetParent(tabs.transform, false);
                var tabImg = tabGO.GetComponent<Image>();
                tabImg.color = ch == currentChannel ? Border : PanelDark;
                tabImgs[ch] = tabImg;

                CreateText(tabGO.transform, ChannelNames[ch], 16,
                    new Vector2(0.5f, 0.5f), Vector2.zero,
                    TextLight, true, true);

                tabBtns[ch] = tabGO.GetComponent<Button>();
                int captured = ch;
                tabBtns[ch].onClick.AddListener(() => SwitchChannel(captured));
            }

            // ---- Message area (scroll view) ----
            var msgBg = CreatePanel(root, new Vector2(0f, 1f),
                new Vector2(10f, -230f), new Vector2(580f, 320f));

            // Scroll-view root
            var scrollGO = new GameObject("ScrollView",
                typeof(RectTransform), typeof(ScrollRect));
            scrollGO.transform.SetParent(msgBg.transform, false);
            var svRT = scrollGO.GetComponent<RectTransform>();
            svRT.anchorMin = Vector2.zero;
            svRT.anchorMax = Vector2.one;
            svRT.offsetMin = Vector2.zero;
            svRT.offsetMax = Vector2.zero;

            // Viewport (masked)
            var viewportGO = new GameObject("Viewport",
                typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportGO.transform.SetParent(scrollGO.transform, false);
            var vpRT = viewportGO.GetComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = Vector2.zero;
            vpRT.offsetMax = Vector2.zero;
            viewportGO.GetComponent<Image>().color = new Color(0, 0, 0, 0.01f);
            viewportGO.GetComponent<Mask>().showMaskGraphic = false;

            // Content
            var contentGO = new GameObject("Content",
                typeof(RectTransform), typeof(VerticalLayoutGroup),
                typeof(ContentSizeFitter));
            contentGO.transform.SetParent(viewportGO.transform, false);
            contentRect = contentGO.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 0f);

            var vlg = contentGO.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 2;
            vlg.padding = new RectOffset(4, 4, 4, 4);
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            contentGO.GetComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            scrollRect = scrollGO.GetComponent<ScrollRect>();
            scrollRect.content = contentRect;
            scrollRect.viewport = vpRT;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            // Placeholder text for empty channels
            messageText = CreateText(msgBg.transform, "暂无消息", 16,
                new Vector2(0.5f, 0.5f), new Vector2(400f, 30f),
                new Color(0.5f, 0.5f, 0.5f, 1f));

            // ---- Input area ----
            var inputBg = CreatePanel(root, new Vector2(0f, 1f),
                new Vector2(10f, -425f), new Vector2(580f, 44f));

            // Channel indicator label
            channelIndicator = CreateText(inputBg.transform, "世界", 14,
                new Vector2(0f, 0.5f), new Vector2(55f, 30f), TextGold);

            // InputField
            var fieldGO = new GameObject("InputField",
                typeof(RectTransform), typeof(Image), typeof(InputField));
            fieldGO.transform.SetParent(inputBg.transform, false);
            var fieldRT = fieldGO.GetComponent<RectTransform>();
            fieldRT.anchorMin = new Vector2(0.5f, 0.5f);
            fieldRT.anchorMax = new Vector2(0.5f, 0.5f);
            fieldRT.pivot = new Vector2(0.5f, 0.5f);
            fieldRT.anchoredPosition = new Vector2(15f, 0f);
            fieldRT.sizeDelta = new Vector2(430f, 34f);
            fieldGO.GetComponent<Image>().color = PanelDark;

            var fieldTextGO = new GameObject("Text",
                typeof(RectTransform), typeof(Text));
            fieldTextGO.transform.SetParent(fieldGO.transform, false);
            var ft = fieldTextGO.GetComponent<Text>();
            ft.font = ChineseFontProvider.GetFont();
            ft.fontSize = 14;
            ft.color = TextLight;
            ft.alignment = TextAnchor.MiddleLeft;
            ft.horizontalOverflow = HorizontalWrapMode.Overflow;
            ft.supportRichText = false;
            var ftRT = fieldTextGO.GetComponent<RectTransform>();
            ftRT.anchorMin = Vector2.zero;
            ftRT.anchorMax = Vector2.one;
            ftRT.offsetMin = new Vector2(8f, 0f);
            ftRT.offsetMax = new Vector2(-4f, 0f);

            var placeholderGO = new GameObject("Placeholder",
                typeof(RectTransform), typeof(Text));
            placeholderGO.transform.SetParent(fieldGO.transform, false);
            var ph = placeholderGO.GetComponent<Text>();
            ph.font = ChineseFontProvider.GetFont();
            ph.fontSize = 14;
            ph.fontStyle = FontStyle.Italic;
            ph.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            ph.alignment = TextAnchor.MiddleLeft;
            ph.horizontalOverflow = HorizontalWrapMode.Overflow;
            ph.text = "输入消息...";
            var phRT = placeholderGO.GetComponent<RectTransform>();
            phRT.anchorMin = Vector2.zero;
            phRT.anchorMax = Vector2.one;
            phRT.offsetMin = new Vector2(8f, 0f);
            phRT.offsetMax = new Vector2(-4f, 0f);

            inputField = fieldGO.GetComponent<InputField>();
            inputField.textComponent = ft;
            inputField.placeholder = ph;
            inputField.text = "";

            // Send button
            var sendBtn = CreateButton(inputBg.transform, "发送",
                new Vector2(1f, 0.5f), new Vector2(-40f, 0f),
                new Vector2(60f, 34f));
            sendBtn.onClick.AddListener(HandleSend);

            // Enter key submits via onEndEdit
            inputField.onEndEdit.AddListener(_ => HandleSend());
        }

        // ================================================================
        // Send
        // ================================================================

        private void HandleSend()
        {
            if (inputField == null) return;
            var content = inputField.text?.Trim();
            if (string.IsNullOrEmpty(content)) return;

            OnSendRequested?.Invoke(currentChannel, privateTargetID, content);
            inputField.text = "";
            inputField.ActivateInputField();
        }

        // ================================================================
        // Public API
        // ================================================================

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SwitchChannel(int channel)
        {
            if (channel < 1 || channel > 3) return;
            currentChannel = channel;

            for (int ch = 1; ch <= 3; ch++)
            {
                if (tabImgs[ch] != null)
                    tabImgs[ch].color = ch == channel ? Border : PanelDark;
            }

            if (channel != ChannelPrivate)
            {
                privateTargetID = 0;
                privateTargetName = null;
            }

            if (channelIndicator != null)
            {
                channelIndicator.text = channel == ChannelPrivate && !string.IsNullOrEmpty(privateTargetName)
                    ? privateTargetName
                    : ChannelNames[channel];
            }

            RenderMessages();
        }

        public void AddMessage(int channel, string senderName,
            ulong senderID, string content, long timestamp, ulong targetID = 0)
        {
            messages.Add(new ChatEntry
            {
                channel = channel,
                senderID = senderID,
                senderName = senderName ?? "",
                targetID = targetID,
                content = content ?? "",
                timestamp = timestamp
            });

            while (messages.Count > MaxMessages)
                messages.RemoveAt(0);

            RenderMessages();
        }

        public void LoadHistory(GoChatMessage[] historyMessages)
        {
            if (historyMessages == null) return;

            foreach (var m in historyMessages.OrderBy(x => x.timestamp))
            {
                messages.Add(new ChatEntry
                {
                    channel = m.channel,
                    senderID = m.sender_id,
                    senderName = m.sender_name ?? "",
                    targetID = m.target_id,
                    content = m.content ?? "",
                    timestamp = m.timestamp
                });
            }

            while (messages.Count > MaxMessages)
                messages.RemoveAt(0);

            RenderMessages();
        }

        public void SetCurrentPlayer(ulong playerID)
        {
            currentPlayerID = playerID;
        }

        // ================================================================
        // Rendering
        // ================================================================

        private void RenderMessages()
        {
            if (contentRect == null) return;

            for (int i = contentRect.childCount - 1; i >= 0; i--)
                Destroy(contentRect.GetChild(i).gameObject);

            var filtered = new List<ChatEntry>(messages.Count);
            for (int i = 0; i < messages.Count; i++)
            {
                if (IsVisible(messages[i]))
                    filtered.Add(messages[i]);
            }

            bool hasMessages = filtered.Count > 0;
            if (messageText != null)
                messageText.gameObject.SetActive(!hasMessages);

            if (!hasMessages) return;

            for (int i = 0; i < filtered.Count; i++)
                BuildMessageRow(filtered[i]);

            ScrollToBottom();
        }

        private bool IsVisible(ChatEntry entry)
        {
            if (currentChannel == ChannelPrivate)
            {
                if (privateTargetID == 0) return false;
                return (entry.channel == ChannelPrivate) &&
                       ((entry.senderID == privateTargetID && entry.targetID == currentPlayerID) ||
                        (entry.senderID == currentPlayerID && entry.targetID == privateTargetID));
            }

            return entry.channel == currentChannel;
        }

        private void BuildMessageRow(ChatEntry entry)
        {
            bool isSelf = entry.senderID == currentPlayerID;
            string tagLabel = entry.channel >= 1 && entry.channel <= 3
                ? ChannelTags[entry.channel] : "";
            Color tagColor = entry.channel == ChannelTeam ? TagTeam
                : entry.channel == ChannelPrivate ? TagPrivate
                : TagWorld;
            string timeStr = FormatTimestamp(entry.timestamp);

            // Row container
            var rowGO = new GameObject("MsgRow",
                typeof(RectTransform), typeof(LayoutElement));
            rowGO.transform.SetParent(contentRect, false);
            var rowRT = rowGO.GetComponent<RectTransform>();
            rowRT.anchorMin = new Vector2(0f, 1f);
            rowRT.anchorMax = new Vector2(1f, 1f);
            rowRT.pivot = new Vector2(0.5f, 1f);
            rowRT.sizeDelta = new Vector2(0f, RowHeight);
            rowGO.GetComponent<LayoutElement>().preferredHeight = RowHeight;

            float x = 0f;

            // [channel tag]
            var tagGO = new GameObject("Tag", typeof(RectTransform), typeof(Text));
            tagGO.transform.SetParent(rowGO.transform, false);
            var tagRT = tagGO.GetComponent<RectTransform>();
            tagRT.anchorMin = new Vector2(0f, 1f);
            tagRT.anchorMax = new Vector2(0f, 1f);
            tagRT.pivot = new Vector2(0f, 0.5f);
            tagRT.anchoredPosition = new Vector2(x, -RowHeight * 0.5f);
            tagRT.sizeDelta = new Vector2(36f, RowHeight);
            var tagTxt = tagGO.GetComponent<Text>();
            tagTxt.text = "[" + tagLabel + "]";
            tagTxt.font = ChineseFontProvider.GetFont();
            tagTxt.fontSize = 12;
            tagTxt.color = tagColor;
            tagTxt.alignment = TextAnchor.MiddleLeft;
            x += 38f;

            // sender name button (clickable for private chat)
            var senderGO = new GameObject("Sender",
                typeof(RectTransform), typeof(Image), typeof(Button));
            senderGO.transform.SetParent(rowGO.transform, false);
            var senderRT = senderGO.GetComponent<RectTransform>();
            senderRT.anchorMin = new Vector2(0f, 1f);
            senderRT.anchorMax = new Vector2(0f, 1f);
            senderRT.pivot = new Vector2(0f, 0.5f);
            senderRT.anchoredPosition = new Vector2(x, -RowHeight * 0.5f);
            senderRT.sizeDelta = new Vector2(76f, RowHeight);
            senderGO.GetComponent<Image>().color = new Color(0, 0, 0, 0);

            var senderTxtGO = new GameObject("Text",
                typeof(RectTransform), typeof(Text));
            senderTxtGO.transform.SetParent(senderGO.transform, false);
            var stRT = senderTxtGO.GetComponent<RectTransform>();
            stRT.anchorMin = Vector2.zero;
            stRT.anchorMax = Vector2.one;
            stRT.offsetMin = Vector2.zero;
            stRT.offsetMax = Vector2.zero;
            var stTxt = senderTxtGO.GetComponent<Text>();
            stTxt.text = entry.senderName;
            stTxt.font = ChineseFontProvider.GetFont();
            stTxt.fontSize = 13;
            stTxt.color = isSelf ? TextGold : TextLight;
            stTxt.alignment = TextAnchor.MiddleLeft;

            ulong sid = entry.senderID;
            string sname = entry.senderName;
            senderGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (sid != currentPlayerID)
                    OnPrivateChatRequested?.Invoke(sid, sname);
            });
            x += 80f;

            // message content
            var msgGO = new GameObject("Content",
                typeof(RectTransform), typeof(Text));
            msgGO.transform.SetParent(rowGO.transform, false);
            var msgRT = msgGO.GetComponent<RectTransform>();
            msgRT.anchorMin = new Vector2(0f, 1f);
            msgRT.anchorMax = new Vector2(0f, 1f);
            msgRT.pivot = new Vector2(0f, 0.5f);
            msgRT.anchoredPosition = new Vector2(x, -RowHeight * 0.5f);
            msgRT.sizeDelta = new Vector2(420f, RowHeight);
            var msgTxt = msgGO.GetComponent<Text>();
            msgTxt.text = entry.content;
            msgTxt.font = ChineseFontProvider.GetFont();
            msgTxt.fontSize = 13;
            msgTxt.color = isSelf ? TextGold : TextLight;
            msgTxt.alignment = TextAnchor.MiddleLeft;
            msgTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            x += 424f;

            // timestamp
            var timeGO = new GameObject("Time",
                typeof(RectTransform), typeof(Text));
            timeGO.transform.SetParent(rowGO.transform, false);
            var timeRT = timeGO.GetComponent<RectTransform>();
            timeRT.anchorMin = new Vector2(0f, 1f);
            timeRT.anchorMax = new Vector2(0f, 1f);
            timeRT.pivot = new Vector2(0f, 0.5f);
            timeRT.anchoredPosition = new Vector2(x, -RowHeight * 0.5f);
            timeRT.sizeDelta = new Vector2(44f, RowHeight);
            var timeTxt = timeGO.GetComponent<Text>();
            timeTxt.text = timeStr;
            timeTxt.font = ChineseFontProvider.GetFont();
            timeTxt.fontSize = 11;
            timeTxt.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            timeTxt.alignment = TextAnchor.MiddleRight;
        }

        private void ScrollToBottom()
        {
            if (scrollRect == null) return;
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases();
        }

        // ================================================================
        // Helpers
        // ================================================================

        private static string FormatTimestamp(long unixMs)
        {
            if (unixMs <= 0) return "";
            var dt = DateTimeOffset.FromUnixTimeMilliseconds(unixMs)
                .ToLocalTime().DateTime;
            return dt.ToString("HH:mm");
        }

        private static GameObject CreatePanel(Transform parent,
            Vector2 anchor, Vector2 offset, Vector2 size)
        {
            var go = new GameObject("Panel",
                typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = offset;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = PanelDark;
            return go;
        }

        private static Text CreateText(Transform parent, string content,
            float size, Vector2 anchor, Vector2 sizeDelta, Color color,
            bool fillParent = false, bool centerAlign = false)
        {
            var go = new GameObject("Text",
                typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var label = go.GetComponent<Text>();
            label.text = content;
            label.font = ChineseFontProvider.GetFont();
            label.fontSize = Mathf.RoundToInt(size);
            label.alignment = centerAlign
                ? TextAnchor.MiddleCenter
                : TextAnchor.MiddleLeft;
            label.color = color;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;

            var rt = label.rectTransform;
            if (fillParent)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            else
            {
                SetAnchor(rt, anchor, sizeDelta);
            }
            return label;
        }

        private static Button CreateButton(Transform parent, string label,
            Vector2 anchor, Vector2 offset, Vector2 sizeDelta)
        {
            var go = new GameObject(label,
                typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color =
                new Color(0.30f, 0.42f, 0.30f, 1f);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = offset;
            rt.sizeDelta = sizeDelta;

            CreateText(go.transform, label, 14,
                new Vector2(0.5f, 0.5f), sizeDelta, TextLight,
                true, true);

            return go.GetComponent<Button>();
        }

        private static void SetAnchor(RectTransform rt, Vector2 anchor,
            Vector2 sizeDelta)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = sizeDelta;
        }
    }
}
