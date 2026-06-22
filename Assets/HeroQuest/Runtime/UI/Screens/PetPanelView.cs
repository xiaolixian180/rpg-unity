using System;
using HeroQuest.Net.Go;
using HeroQuest.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace HeroQuest.UI.Screens
{
    public sealed class PetPanelView : MonoBehaviour
    {
        private static readonly Color Bg = new(0.03f, 0.04f, 0.03f, 0.97f);
        private static readonly Color PanelDark = new(0.06f, 0.08f, 0.06f, 0.95f);
        private static readonly Color Border = new(0.26f, 0.42f, 0.24f, 1f);
        private static readonly Color TextLight = new(0.92f, 0.90f, 0.82f, 1f);
        private static readonly Color TextGold = new(0.78f, 0.58f, 0.22f, 1f);

        private static readonly Color[] QualityColors = new Color[]
        {
            new(0.80f, 0.80f, 0.80f, 1f), // 0=白
            new(0.12f, 1.00f, 0.00f, 1f), // 1=绿
            new(0.00f, 0.44f, 0.87f, 1f), // 2=蓝
            new(0.64f, 0.21f, 0.93f, 1f), // 3=紫
            new(1.00f, 0.50f, 0.00f, 1f), // 4=橙
            new(1.00f, 0.00f, 0.00f, 1f), // 5=红
        };

        private static readonly string[] TypeLabels = { "攻击", "防御", "辅助", "掠夺" };
        private static readonly string[] SlotNames = { "项圈", "护甲", "饰品" };

        // --- Events ---
        public event Action OnCloseRequested;
        public event Action<ulong> OnSummonRequested;
        public event Action<ulong> OnRecallRequested;
        public event Action<ulong> OnLevelUpRequested;
        public event Action<ulong> OnEvolveRequested;
        public event Action<ulong, int> OnExploreRequested;
        public event Action<ulong, int> OnEquipRequested;
        public event Action<ulong, int> OnUnequipRequested;

        // --- Left column: pet list ---
        private Transform listContent;
        private GameObject emptyLabel;

        // --- Right column: detail ---
        private Text detailNameText;
        private Text detailStarsText;
        private Text detailTypeText;
        private Text detailLevelText;
        private Text detailAtkText;
        private Text detailDefText;
        private Text detailHpText;

        private Text[] equipSlotTexts = new Text[3];
        private Button[] equipBtns = new Button[3];
        private Button[] unequipBtns = new Button[3];

        private Button summonBtn;
        private Button recallBtn;
        private Button levelUpBtn;
        private Button evolveBtn;
        private Button exploreBtn;

        // --- State ---
        private GoPetData[] _pets;
        private ulong _activePetUid;
        private ulong _selectedPetUid;

        public static PetPanelView Create(Canvas parent)
        {
            var go = new GameObject("PetPanelView", typeof(RectTransform), typeof(Image));
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
            SetAnchor(panelRect, new Vector2(0.5f, 0.5f), new Vector2(850f, 600f));
            panel.GetComponent<Image>().color = Bg;

            var view = go.AddComponent<PetPanelView>();
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

        public void ApplyPetList(GoPetData[] pets, ulong activePetUid)
        {
            _pets = pets;
            _activePetUid = activePetUid;

            // Clear existing list rows
            for (int i = listContent.childCount - 1; i >= 0; i--)
                Destroy(listContent.GetChild(i).gameObject);

            if (emptyLabel != null)
                emptyLabel.SetActive(pets == null || pets.Length == 0);

            if (pets == null || pets.Length == 0) return;

            for (int i = 0; i < pets.Length; i++)
            {
                var pet = pets[i];
                CreatePetRow(pet, i, activePetUid);
            }

            // Auto-select first pet if nothing selected
            if (_selectedPetUid == 0 && pets.Length > 0)
                SelectPet(pets[0]);
        }

        public void ApplyPetDetail(GoPetData pet, int attack, int defense, int hp)
        {
            if (pet == null) return;

            _selectedPetUid = pet.uid;

            if (detailNameText != null)
            {
                detailNameText.text = pet.name;
                int qi = Mathf.Clamp(pet.quality, 0, QualityColors.Length - 1);
                detailNameText.color = QualityColors[qi];
            }

            if (detailStarsText != null)
            {
                detailStarsText.text = QualityStars(pet.quality);
                int qi = Mathf.Clamp(pet.quality, 0, QualityColors.Length - 1);
                detailStarsText.color = QualityColors[qi];
            }

            if (detailTypeText != null)
            {
                int ti = Mathf.Clamp(pet.type, 0, TypeLabels.Length - 1);
                detailTypeText.text = TypeLabels[ti];
            }

            if (detailLevelText != null)
                detailLevelText.text = $"Lv.{pet.level}";

            if (detailAtkText != null)
                detailAtkText.text = $"攻击: {attack}";
            if (detailDefText != null)
                detailDefText.text = $"防御: {defense}";
            if (detailHpText != null)
                detailHpText.text = $"HP: {hp}";

            // Equipment slots: reset to empty
            for (int s = 0; s < 3; s++)
            {
                if (equipSlotTexts[s] != null)
                {
                    equipSlotTexts[s].text = $"{SlotNames[s]}: 空";
                    equipSlotTexts[s].color = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
                if (equipBtns[s] != null) equipBtns[s].interactable = true;
                if (unequipBtns[s] != null) unequipBtns[s].interactable = false;
            }

            // Action buttons: toggle summon/recall based on active state
            bool isActive = pet.uid == _activePetUid;
            if (summonBtn != null) summonBtn.interactable = !isActive;
            if (recallBtn != null) recallBtn.interactable = isActive;
            if (levelUpBtn != null) levelUpBtn.interactable = true;
            if (evolveBtn != null) evolveBtn.interactable = pet.level >= 10;
            if (exploreBtn != null) exploreBtn.interactable = true;
        }

        public void ApplyNoPets()
        {
            // Clear list
            for (int i = listContent.childCount - 1; i >= 0; i--)
                Destroy(listContent.GetChild(i).gameObject);

            if (emptyLabel != null)
                emptyLabel.SetActive(true);

            // Clear detail
            ClearDetail();
        }

        private void ClearDetail()
        {
            if (detailNameText != null) { detailNameText.text = ""; detailNameText.color = TextLight; }
            if (detailStarsText != null) detailStarsText.text = "";
            if (detailTypeText != null) detailTypeText.text = "";
            if (detailLevelText != null) detailLevelText.text = "";
            if (detailAtkText != null) detailAtkText.text = "";
            if (detailDefText != null) detailDefText.text = "";
            if (detailHpText != null) detailHpText.text = "";

            for (int s = 0; s < 3; s++)
            {
                if (equipSlotTexts[s] != null)
                {
                    equipSlotTexts[s].text = $"{SlotNames[s]}: 空";
                    equipSlotTexts[s].color = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
                if (equipBtns[s] != null) equipBtns[s].interactable = false;
                if (unequipBtns[s] != null) unequipBtns[s].interactable = false;
            }

            if (summonBtn != null) summonBtn.interactable = false;
            if (recallBtn != null) recallBtn.interactable = false;
            if (levelUpBtn != null) levelUpBtn.interactable = false;
            if (evolveBtn != null) evolveBtn.interactable = false;
            if (exploreBtn != null) exploreBtn.interactable = false;
        }

        // --- Build UI ---

        private void Build(Transform root)
        {
            // Title
            CreateText(root, "宠物", 32, new Vector2(0.5f, 0.95f), new Vector2(400f, 48f), TextLight);

            // Close button
            var closeBtn = CreateButton(root, "X", new Vector2(0.96f, 0.95f), new Vector2(44f, 44f));
            closeBtn.onClick.AddListener(() => OnCloseRequested?.Invoke());

            // Left column: pet list (~40% width)
            var leftBg = new GameObject("LeftBg", typeof(RectTransform), typeof(Image));
            leftBg.transform.SetParent(root, false);
            SetAnchor(leftBg.GetComponent<RectTransform>(), new Vector2(0.22f, 0.50f), new Vector2(340f, 520f));
            leftBg.GetComponent<Image>().color = PanelDark;

            var left = leftBg.transform;
            CreateText(left, "— 宠物列表 —", 18, new Vector2(0.5f, 0.95f), new Vector2(300f, 28f), Border);

            // Scrollable pet list
            var scrollGo = new GameObject("PetList", typeof(RectTransform), typeof(ScrollRect));
            scrollGo.transform.SetParent(left, false);
            var scrollRect = scrollGo.GetComponent<RectTransform>();
            SetAnchor(scrollRect, new Vector2(0.5f, 0.46f), new Vector2(320f, 440f));
            scrollGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollGo.transform, false);
            var vpRect = viewport.GetComponent<RectTransform>();
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.offsetMin = Vector2.zero;
            vpRect.offsetMax = Vector2.zero;
            viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 1f);
            contentRect.anchorMax = new Vector2(0.5f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(310f, 440f);

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.content = contentRect;
            scroll.viewport = vpRect;
            scroll.horizontal = false;
            scroll.vertical = true;

            listContent = content.transform;

            // Empty state label
            emptyLabel = new GameObject("EmptyLabel", typeof(RectTransform), typeof(Text));
            emptyLabel.transform.SetParent(left, false);
            var emptyText = emptyLabel.GetComponent<Text>();
            emptyText.text = "暂无宠物";
            emptyText.font = ChineseFontProvider.GetFont();
            emptyText.fontSize = 20;
            emptyText.alignment = TextAnchor.MiddleCenter;
            emptyText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            SetAnchor(emptyLabel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.46f), new Vector2(300f, 40f));
            emptyLabel.SetActive(false);

            // Right column: pet detail (~60% width)
            var rightBg = new GameObject("RightBg", typeof(RectTransform), typeof(Image));
            rightBg.transform.SetParent(root, false);
            SetAnchor(rightBg.GetComponent<RectTransform>(), new Vector2(0.70f, 0.50f), new Vector2(460f, 520f));
            rightBg.GetComponent<Image>().color = PanelDark;

            BuildDetailPanel(rightBg.transform);
        }

        private void BuildDetailPanel(Transform right)
        {
            CreateText(right, "— 宠物详情 —", 18, new Vector2(0.5f, 0.95f), new Vector2(400f, 28f), Border);

            // Pet name + stars + type
            detailNameText = CreateText(right, "", 24, new Vector2(0.5f, 0.88f), new Vector2(300f, 34f), TextGold);
            detailStarsText = CreateText(right, "", 18, new Vector2(0.30f, 0.82f), new Vector2(140f, 26f), TextLight);
            detailTypeText = CreateText(right, "", 18, new Vector2(0.70f, 0.82f), new Vector2(140f, 26f), TextLight);
            detailLevelText = CreateText(right, "", 20, new Vector2(0.5f, 0.76f), new Vector2(300f, 28f), TextLight);

            // Stats
            CreateText(right, "— 属性 —", 16, new Vector2(0.5f, 0.70f), new Vector2(400f, 24f), Border);
            detailAtkText = CreateText(right, "攻击: 0", 18, new Vector2(0.5f, 0.65f), new Vector2(400f, 26f), TextLight);
            detailDefText = CreateText(right, "防御: 0", 18, new Vector2(0.5f, 0.60f), new Vector2(400f, 26f), TextLight);
            detailHpText = CreateText(right, "HP: 0", 18, new Vector2(0.5f, 0.55f), new Vector2(400f, 26f), TextLight);

            // Equipment slots
            CreateText(right, "— 装备 —", 16, new Vector2(0.5f, 0.49f), new Vector2(400f, 24f), Border);

            for (int s = 0; s < 3; s++)
            {
                float y = 0.43f - s * 0.07f;

                equipSlotTexts[s] = CreateText(right, $"{SlotNames[s]}: 空", 16,
                    new Vector2(0.30f, y), new Vector2(200f, 24f), new Color(0.5f, 0.5f, 0.5f, 1f));

                var slotIdx = s;
                equipBtns[s] = CreateButton(right, "装备", new Vector2(0.66f, y), new Vector2(52f, 24f));
                equipBtns[s].onClick.AddListener(() => OnEquipRequested?.Invoke(_selectedPetUid, slotIdx));
                equipBtns[s].interactable = false;

                unequipBtns[s] = CreateButton(right, "卸下", new Vector2(0.82f, y), new Vector2(52f, 24f));
                unequipBtns[s].onClick.AddListener(() => OnUnequipRequested?.Invoke(_selectedPetUid, slotIdx));
                unequipBtns[s].interactable = false;
            }

            // Action buttons
            CreateText(right, "— 操作 —", 16, new Vector2(0.5f, 0.20f), new Vector2(400f, 24f), Border);

            float btnY = 0.14f;
            float btnSpacing = 0.18f;
            float startX = 0.5f - btnSpacing * 2f;

            summonBtn = CreateButton(right, "出战", new Vector2(startX, btnY), new Vector2(72f, 32f));
            summonBtn.onClick.AddListener(() => OnSummonRequested?.Invoke(_selectedPetUid));

            recallBtn = CreateButton(right, "召回", new Vector2(startX + btnSpacing, btnY), new Vector2(72f, 32f));
            recallBtn.onClick.AddListener(() => OnRecallRequested?.Invoke(_selectedPetUid));

            levelUpBtn = CreateButton(right, "升级", new Vector2(startX + btnSpacing * 2f, btnY), new Vector2(72f, 32f));
            levelUpBtn.onClick.AddListener(() => OnLevelUpRequested?.Invoke(_selectedPetUid));

            evolveBtn = CreateButton(right, "进化", new Vector2(startX + btnSpacing * 3f, btnY), new Vector2(72f, 32f));
            evolveBtn.onClick.AddListener(() => OnEvolveRequested?.Invoke(_selectedPetUid));

            exploreBtn = CreateButton(right, "探索", new Vector2(startX + btnSpacing * 4f, btnY), new Vector2(72f, 32f));
            exploreBtn.onClick.AddListener(() => OnExploreRequested?.Invoke(_selectedPetUid, 30));

            // Disable all by default
            summonBtn.interactable = false;
            recallBtn.interactable = false;
            levelUpBtn.interactable = false;
            evolveBtn.interactable = false;
            exploreBtn.interactable = false;
        }

        // --- Pet list row ---

        private void CreatePetRow(GoPetData pet, int index, ulong activePetUid)
        {
            float rowHeight = 60f;
            float rowWidth = 310f;
            float y = -index * rowHeight - rowHeight * 0.5f;

            var row = new GameObject($"PetRow_{pet.uid}", typeof(RectTransform), typeof(Image), typeof(Button));
            row.transform.SetParent(listContent, false);
            var rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.5f, 1f);
            rowRect.anchorMax = new Vector2(0.5f, 1f);
            rowRect.pivot = new Vector2(0.5f, 0.5f);
            rowRect.anchoredPosition = new Vector2(0f, y);
            rowRect.sizeDelta = new Vector2(rowWidth, rowHeight);

            // Highlight if selected or active
            bool isActive = pet.uid == activePetUid;
            row.GetComponent<Image>().color = isActive
                ? new Color(0.20f, 0.35f, 0.20f, 1f)
                : new Color(0.10f, 0.14f, 0.10f, 0.8f);

            int qi = Mathf.Clamp(pet.quality, 0, QualityColors.Length - 1);
            int ti = Mathf.Clamp(pet.type, 0, TypeLabels.Length - 1);

            // Pet name
            var nameText = CreateText(row.transform, pet.name, 17,
                new Vector2(0.30f, 0.70f), new Vector2(160f, 22f), QualityColors[qi]);

            // Stars
            var starsText = CreateText(row.transform, QualityStars(pet.quality), 14,
                new Vector2(0.80f, 0.70f), new Vector2(80f, 18f), QualityColors[qi]);

            // Level + Type
            string levelType = $"Lv.{pet.level}  {TypeLabels[ti]}";
            CreateText(row.transform, levelType, 14,
                new Vector2(0.30f, 0.30f), new Vector2(160f, 18f), TextLight);

            // Active indicator
            if (isActive)
            {
                CreateText(row.transform, "[出战]", 14,
                    new Vector2(0.82f, 0.30f), new Vector2(60f, 18f), TextGold);
            }

            // Click handler
            var capturedPet = pet;
            row.GetComponent<Button>().onClick.AddListener(() => SelectPet(capturedPet));
        }

        private void SelectPet(GoPetData pet)
        {
            _selectedPetUid = pet.uid;
            // Notify via a dummy detail with zero stats; the controller is expected
            // to call ApplyPetDetail with real data in response to the selection.
            // For immediate visual feedback, show basic info here.
            if (detailNameText != null)
            {
                detailNameText.text = pet.name;
                int qi = Mathf.Clamp(pet.quality, 0, QualityColors.Length - 1);
                detailNameText.color = QualityColors[qi];
            }
            if (detailStarsText != null)
            {
                detailStarsText.text = QualityStars(pet.quality);
                int qi = Mathf.Clamp(pet.quality, 0, QualityColors.Length - 1);
                detailStarsText.color = QualityColors[qi];
            }
            if (detailTypeText != null)
            {
                int ti = Mathf.Clamp(pet.type, 0, TypeLabels.Length - 1);
                detailTypeText.text = TypeLabels[ti];
            }
            if (detailLevelText != null)
                detailLevelText.text = $"Lv.{pet.level}";

            bool isActive = pet.uid == _activePetUid;
            if (summonBtn != null) summonBtn.interactable = !isActive;
            if (recallBtn != null) recallBtn.interactable = isActive;
            if (levelUpBtn != null) levelUpBtn.interactable = true;
            if (evolveBtn != null) evolveBtn.interactable = pet.level >= 10;
            if (exploreBtn != null) exploreBtn.interactable = true;

            // Rebuild list to update selection highlight
            if (_pets != null)
                ApplyPetList(_pets, _activePetUid);
        }

        // --- Helpers ---

        private static string QualityStars(int quality)
        {
            int count = Mathf.Clamp(quality + 1, 1, 6);
            return new string('\u2605', count);
        }

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
    }
}
