using System;
using System.Collections.Generic;
using HeroQuest.Net.Go;
using HeroQuest.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace HeroQuest.UI.Screens
{
    /// <summary>
    /// 锻造面板 — 展示锻造配方列表，选择配方可查看产出装备预览、所需材料、金币消耗，点击锻造按钮发起锻造请求。
    /// </summary>
    public sealed class ForgePanelView : MonoBehaviour
    {
        // ================================================================
        // 事件
        // ================================================================

        public Action OnCloseRequested;
        public Action<ulong, ulong[]> OnForgeRequested;

        // ================================================================
        // 锻造配方数据（与服务端 model/shop.go StaticForgeRecipes 保持一致）
        // ================================================================

        public sealed class ForgeRecipeData
        {
            public ulong id;
            public string name;
            public (ulong materialId, int count)[] materials;
            public int resultEquipId;
            public int resultQuality;
            public int requireLevel;
            public long cost;
        }

        public static readonly ForgeRecipeData[] AllRecipes = new ForgeRecipeData[]
        {
            // 武器
            new ForgeRecipeData { id = 1, name = "精钢长剑锻造", materials = new[] { (2001u, 3), (2002u, 2) }, resultEquipId = 3, resultQuality = 1, requireLevel = 10, cost = 500 },
            new ForgeRecipeData { id = 2, name = "暗影之刃锻造", materials = new[] { (2001u, 5), (2003u, 3) }, resultEquipId = 4, resultQuality = 2, requireLevel = 20, cost = 1500 },
            new ForgeRecipeData { id = 3, name = "龙牙剑锻造", materials = new[] { (2001u, 10), (2003u, 5), (2004u, 2) }, resultEquipId = 5, resultQuality = 3, requireLevel = 30, cost = 5000 },
            new ForgeRecipeData { id = 14, name = "天罚圣剑锻造", materials = new[] { (2001u, 20), (2003u, 10), (2004u, 5), (2005u, 1) }, resultEquipId = 6, resultQuality = 4, requireLevel = 45, cost = 15000 },
            // 头盔
            new ForgeRecipeData { id = 4, name = "秘银头盔锻造", materials = new[] { (2001u, 3), (2002u, 2) }, resultEquipId = 12, resultQuality = 1, requireLevel = 10, cost = 400 },
            new ForgeRecipeData { id = 5, name = "暗夜兜帽锻造", materials = new[] { (2001u, 5), (2003u, 3) }, resultEquipId = 13, resultQuality = 2, requireLevel = 20, cost = 1200 },
            new ForgeRecipeData { id = 6, name = "战神之冠锻造", materials = new[] { (2001u, 10), (2003u, 5), (2004u, 2) }, resultEquipId = 14, resultQuality = 3, requireLevel = 30, cost = 4500 },
            // 铠甲
            new ForgeRecipeData { id = 7, name = "精钢战甲锻造", materials = new[] { (2001u, 4), (2002u, 3) }, resultEquipId = 22, resultQuality = 1, requireLevel = 10, cost = 600 },
            new ForgeRecipeData { id = 8, name = "暗影铠甲锻造", materials = new[] { (2001u, 6), (2003u, 4) }, resultEquipId = 23, resultQuality = 2, requireLevel = 20, cost = 1800 },
            new ForgeRecipeData { id = 9, name = "龙鳞铠甲锻造", materials = new[] { (2001u, 12), (2003u, 6), (2004u, 3) }, resultEquipId = 24, resultQuality = 3, requireLevel = 30, cost = 6000 },
            // 手套/靴子/项链/戒指
            new ForgeRecipeData { id = 10, name = "精钢护手锻造", materials = new[] { (2001u, 2), (2002u, 2) }, resultEquipId = 32, resultQuality = 1, requireLevel = 10, cost = 350 },
            new ForgeRecipeData { id = 11, name = "疾风之靴锻造", materials = new[] { (2001u, 3), (2002u, 2) }, resultEquipId = 42, resultQuality = 1, requireLevel = 10, cost = 400 },
            new ForgeRecipeData { id = 12, name = "银项链锻造", materials = new[] { (2001u, 3), (2002u, 2) }, resultEquipId = 51, resultQuality = 1, requireLevel = 10, cost = 450 },
            new ForgeRecipeData { id = 13, name = "银戒指锻造", materials = new[] { (2001u, 2), (2002u, 2) }, resultEquipId = 61, resultQuality = 1, requireLevel = 10, cost = 350 },
        };

        // ================================================================
        // 装备模板数据（与服务端 model/equipment.go EquipTemplates 保持一致）
        // ================================================================

        public sealed class EquipTemplateData
        {
            public int id;
            public string name;
            public int quality;
            public long baseAtk;
            public long baseDef;
            public long baseHp;
            public int requireLevel;
        }

        public static readonly Dictionary<int, EquipTemplateData> EquipTemplates = new Dictionary<int, EquipTemplateData>
        {
            // 武器
            { 1, new EquipTemplateData { id = 1, name = "木剑", quality = 0, baseAtk = 5, baseDef = 0, baseHp = 0, requireLevel = 1 } },
            { 2, new EquipTemplateData { id = 2, name = "铁剑", quality = 0, baseAtk = 12, baseDef = 0, baseHp = 0, requireLevel = 5 } },
            { 3, new EquipTemplateData { id = 3, name = "精钢长剑", quality = 1, baseAtk = 25, baseDef = 2, baseHp = 0, requireLevel = 10 } },
            { 4, new EquipTemplateData { id = 4, name = "暗影之刃", quality = 2, baseAtk = 45, baseDef = 5, baseHp = 0, requireLevel = 20 } },
            { 5, new EquipTemplateData { id = 5, name = "龙牙剑", quality = 3, baseAtk = 80, baseDef = 10, baseHp = 50, requireLevel = 30 } },
            { 6, new EquipTemplateData { id = 6, name = "天罚圣剑", quality = 4, baseAtk = 130, baseDef = 15, baseHp = 100, requireLevel = 45 } },
            // 头盔
            { 10, new EquipTemplateData { id = 10, name = "布帽", quality = 0, baseAtk = 0, baseDef = 3, baseHp = 10, requireLevel = 1 } },
            { 11, new EquipTemplateData { id = 11, name = "铁头盔", quality = 0, baseAtk = 0, baseDef = 8, baseHp = 30, requireLevel = 5 } },
            { 12, new EquipTemplateData { id = 12, name = "秘银头盔", quality = 1, baseAtk = 0, baseDef = 18, baseHp = 60, requireLevel = 10 } },
            { 13, new EquipTemplateData { id = 13, name = "暗夜兜帽", quality = 2, baseAtk = 5, baseDef = 30, baseHp = 100, requireLevel = 20 } },
            { 14, new EquipTemplateData { id = 14, name = "战神之冠", quality = 3, baseAtk = 10, baseDef = 50, baseHp = 180, requireLevel = 30 } },
            // 铠甲
            { 20, new EquipTemplateData { id = 20, name = "布衣", quality = 0, baseAtk = 0, baseDef = 5, baseHp = 20, requireLevel = 1 } },
            { 21, new EquipTemplateData { id = 21, name = "铁甲", quality = 0, baseAtk = 0, baseDef = 15, baseHp = 50, requireLevel = 5 } },
            { 22, new EquipTemplateData { id = 22, name = "精钢战甲", quality = 1, baseAtk = 0, baseDef = 30, baseHp = 100, requireLevel = 10 } },
            { 23, new EquipTemplateData { id = 23, name = "暗影铠甲", quality = 2, baseAtk = 5, baseDef = 30, baseHp = 100, requireLevel = 20 } },
            { 24, new EquipTemplateData { id = 24, name = "龙鳞铠甲", quality = 3, baseAtk = 10, baseDef = 90, baseHp = 300, requireLevel = 30 } },
            // 手套
            { 30, new EquipTemplateData { id = 30, name = "布手套", quality = 0, baseAtk = 2, baseDef = 2, baseHp = 0, requireLevel = 1 } },
            { 31, new EquipTemplateData { id = 31, name = "铁手套", quality = 0, baseAtk = 5, baseDef = 5, baseHp = 0, requireLevel = 5 } },
            { 32, new EquipTemplateData { id = 32, name = "精钢护手", quality = 1, baseAtk = 12, baseDef = 12, baseHp = 20, requireLevel = 10 } },
            // 靴子
            { 40, new EquipTemplateData { id = 40, name = "草鞋", quality = 0, baseAtk = 0, baseDef = 3, baseHp = 5, requireLevel = 1 } },
            { 41, new EquipTemplateData { id = 41, name = "铁靴", quality = 0, baseAtk = 0, baseDef = 8, baseHp = 15, requireLevel = 5 } },
            { 42, new EquipTemplateData { id = 42, name = "疾风之靴", quality = 1, baseAtk = 0, baseDef = 18, baseHp = 30, requireLevel = 10 } },
            // 项链
            { 50, new EquipTemplateData { id = 50, name = "铜项链", quality = 0, baseAtk = 3, baseDef = 0, baseHp = 10, requireLevel = 1 } },
            { 51, new EquipTemplateData { id = 51, name = "银项链", quality = 1, baseAtk = 8, baseDef = 0, baseHp = 30, requireLevel = 10 } },
            // 戒指
            { 60, new EquipTemplateData { id = 60, name = "铜戒指", quality = 0, baseAtk = 2, baseDef = 0, baseHp = 5, requireLevel = 1 } },
            { 61, new EquipTemplateData { id = 61, name = "银戒指", quality = 1, baseAtk = 6, baseDef = 0, baseHp = 20, requireLevel = 10 } },
        };

        // ================================================================
        // 品质颜色
        // ================================================================

        private static readonly Color[] QualityColors = new Color[]
        {
            new Color(0.80f, 0.80f, 0.80f, 1f), // 白
            new Color(0.12f, 1.00f, 0.00f, 1f), // 绿
            new Color(0.00f, 0.44f, 0.87f, 1f), // 蓝
            new Color(0.64f, 0.21f, 0.93f, 1f), // 紫
            new Color(1.00f, 0.50f, 0.00f, 1f), // 橙
            new Color(1.00f, 0.00f, 0.00f, 1f), // 红
        };

        private static readonly string[] QualityNames = { "普通", "优秀", "精良", "史诗", "传说", "神话" };

        // ================================================================
        // 颜色常量
        // ================================================================

        private static readonly Color PanelDark = new Color(0.10f, 0.12f, 0.10f, 0.96f);
        private static readonly Color PanelMid = new Color(0.15f, 0.18f, 0.15f, 0.94f);
        private static readonly Color TextLight = new Color(0.92f, 0.90f, 0.82f, 1f);
        private static readonly Color TextDim = new Color(0.60f, 0.60f, 0.55f, 1f);
        private static readonly Color Border = new Color(0.35f, 0.45f, 0.35f, 1f);
        private static readonly Color BtnGreen = new Color(0.25f, 0.45f, 0.25f, 1f);
        private static readonly Color BtnDisabled = new Color(0.20f, 0.20f, 0.20f, 1f);

        // ================================================================
        // UI 引用
        // ================================================================

        private Text _detailNameText;
        private Text _detailQualityText;
        private Text _detailAtkText;
        private Text _detailDefText;
        private Text _detailHpText;
        private Text _detailLevelText;
        private Text _detailMaterialsText;
        private Text _detailCostText;
        private Text _resultText;
        private Button _forgeBtn;
        private ScrollRect _recipeScroll;
        private Transform _recipeListContent;

        private ForgeRecipeData _selectedRecipe;
        private int _playerLevel;
        private long _playerGold;

        // ================================================================
        // 工厂方法
        // ================================================================

        public static ForgePanelView Create(Transform parent)
        {
            var go = new GameObject("ForgePanel", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var bg = go.GetComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.85f);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var panel = go.AddComponent<ForgePanelView>();
            panel.BuildUI(go.transform);
            panel.Hide();
            return panel;
        }

        // ================================================================
        // 公共方法
        // ================================================================

        public void Show()
        {
            gameObject.SetActive(true);
            PopulateRecipeList();
            if (AllRecipes.Length > 0)
                SelectRecipe(AllRecipes[0]);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetPlayerInfo(int level, long gold)
        {
            _playerLevel = level;
            _playerGold = gold;
            UpdateForgeButtonState();
        }

        public void ApplyForgeResult(uint code, string resultName, int quality)
        {
            if (code == 0)
            {
                Color c = quality >= 0 && quality < QualityColors.Length ? QualityColors[quality] : TextLight;
                string qName = quality >= 0 && quality < QualityNames.Length ? QualityNames[quality] : "";
                _resultText.text = $"锻造成功！获得 <color=#{ColorUtility.ToHtmlStringRGB(c)}>{resultName}</color> ({qName})";
            }
            else
            {
                string reason = code switch
                {
                    400 => "槽位为空",
                    401 => "槽位无效",
                    404 => "锻造图纸不存在",
                    405 => "材料不足",
                    900 => "金币不足",
                    _ => $"code={code}"
                };
                _resultText.text = $"锻造失败: {reason}";
            }
        }

        // ================================================================
        // UI 构建
        // ================================================================

        private void BuildUI(Transform root)
        {
            // 标题
            CreateText(root, "— 锻造 —", 24, new Vector2(0.5f, 0.95f), new Vector2(300f, 36f), TextLight);

            // 关闭按钮
            var closeBtn = CreateButton(root, "X", new Vector2(0.96f, 0.95f), new Vector2(36f, 36f));
            closeBtn.onClick.AddListener(() => OnCloseRequested?.Invoke());

            // 左侧：配方列表背景
            var leftBg = new GameObject("LeftBg", typeof(RectTransform), typeof(Image));
            leftBg.transform.SetParent(root, false);
            leftBg.GetComponent<Image>().color = PanelDark;
            SetAnchor(leftBg.GetComponent<RectTransform>(), new Vector2(0.25f, 0.50f), new Vector2(380f, 620f));

            var left = leftBg.transform;
            CreateText(left, "配方列表", 18, new Vector2(0.5f, 0.95f), new Vector2(200f, 28f), Border);

            // ScrollRect for recipe list
            var scrollGo = new GameObject("RecipeScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(Mask));
            scrollGo.transform.SetParent(left, false);
            scrollGo.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.08f, 1f);
            SetAnchor(scrollGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0.45f), new Vector2(360f, 540f));

            var mask = scrollGo.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            _recipeScroll = scrollGo.GetComponent<ScrollRect>();

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollGo.transform, false);
            viewport.GetComponent<Image>().color = Color.white;
            var vpRt = viewport.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = new Vector2(5, 5);
            vpRt.offsetMax = new Vector2(-5, -5);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.sizeDelta = new Vector2(0, 0);

            var layout = content.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 4;
            layout.padding = new RectOffset(8, 8, 4, 4);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            var fitter = content.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _recipeListContent = content.transform;
            _recipeScroll.content = contentRt;
            _recipeScroll.verticalScrollbar = scrollGo.GetComponent<ScrollRect>().verticalScrollbar;

            // 右侧：详情背景
            var rightBg = new GameObject("RightBg", typeof(RectTransform), typeof(Image));
            rightBg.transform.SetParent(root, false);
            rightBg.GetComponent<Image>().color = PanelMid;
            SetAnchor(rightBg.GetComponent<RectTransform>(), new Vector2(0.68f, 0.50f), new Vector2(440f, 620f));

            var right = rightBg.transform;
            CreateText(right, "产出预览", 18, new Vector2(0.5f, 0.95f), new Vector2(200f, 28f), Border);

            // 详情文本
            _detailNameText = CreateText(right, "", 20, new Vector2(0.5f, 0.85f), new Vector2(380f, 30f), TextLight);
            _detailQualityText = CreateText(right, "", 16, new Vector2(0.5f, 0.78f), new Vector2(380f, 24f), TextDim);
            _detailAtkText = CreateText(right, "", 15, new Vector2(0.5f, 0.70f), new Vector2(380f, 22f), TextLight);
            _detailDefText = CreateText(right, "", 15, new Vector2(0.5f, 0.64f), new Vector2(380f, 22f), TextLight);
            _detailHpText = CreateText(right, "", 15, new Vector2(0.5f, 0.58f), new Vector2(380f, 22f), TextLight);
            _detailLevelText = CreateText(right, "", 15, new Vector2(0.5f, 0.52f), new Vector2(380f, 22f), new Color(1f, 0.8f, 0.3f, 1f));

            CreateText(right, "所需材料", 16, new Vector2(0.5f, 0.43f), new Vector2(200f, 24f), Border);
            _detailMaterialsText = CreateText(right, "", 14, new Vector2(0.5f, 0.33f), new Vector2(380f, 80f), TextDim);

            CreateText(right, "金币消耗", 16, new Vector2(0.5f, 0.22f), new Vector2(200f, 24f), Border);
            _detailCostText = CreateText(right, "", 16, new Vector2(0.5f, 0.16f), new Vector2(380f, 24f), new Color(1f, 0.85f, 0.2f, 1f));

            // 锻造按钮
            _forgeBtn = CreateButton(right, "锻 造", new Vector2(0.5f, 0.06f), new Vector2(160f, 40f));
            _forgeBtn.onClick.AddListener(() =>
            {
                if (_selectedRecipe == null) return;
                var materials = new ulong[_selectedRecipe.materials.Length];
                for (int i = 0; i < _selectedRecipe.materials.Length; i++)
                    materials[i] = _selectedRecipe.materials[i].materialId;
                OnForgeRequested?.Invoke(_selectedRecipe.id, materials);
            });

            // 底部：结果展示
            var resultBg = new GameObject("ResultBg", typeof(RectTransform), typeof(Image));
            resultBg.transform.SetParent(root, false);
            resultBg.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.12f, 0.90f);
            SetAnchor(resultBg.GetComponent<RectTransform>(), new Vector2(0.5f, 0.05f), new Vector2(800f, 50f));

            _resultText = CreateText(resultBg.transform, "", 15, new Vector2(0.5f, 0.5f), new Vector2(760f, 40f), TextLight);
        }

        // ================================================================
        // 配方列表
        // ================================================================

        private void PopulateRecipeList()
        {
            // 清空现有
            for (int i = _recipeListContent.childCount - 1; i >= 0; i--)
                Destroy(_recipeListContent.GetChild(i).gameObject);

            foreach (var recipe in AllRecipes)
            {
                var r = recipe;
                var row = new GameObject($"Recipe_{r.id}", typeof(RectTransform), typeof(Image), typeof(Button));
                row.transform.SetParent(_recipeListContent, false);

                var img = row.GetComponent<Image>();
                img.color = PanelDark;

                var btn = row.GetComponent<Button>();
                btn.targetGraphic = img;
                btn.onClick.AddListener(() => SelectRecipe(r));

                var rowRt = row.GetComponent<RectTransform>();
                rowRt.sizeDelta = new Vector2(0, 40);

                // 品质颜色条
                Color qc = r.resultQuality >= 0 && r.resultQuality < QualityColors.Length
                    ? QualityColors[r.resultQuality] : TextLight;

                var nameText = CreateText(row.transform, r.name, 14, new Vector2(0.4f, 0.5f), new Vector2(240f, 30f), qc);
                nameText.alignment = TextAnchor.MiddleLeft;

                var levelText = CreateText(row.transform, $"Lv.{r.requireLevel}", 12, new Vector2(0.85f, 0.5f), new Vector2(60f, 24f), TextDim);
            }
        }

        // ================================================================
        // 选中配方
        // ================================================================

        private void SelectRecipe(ForgeRecipeData recipe)
        {
            _selectedRecipe = recipe;

            // 查找产出装备模板
            if (EquipTemplates.TryGetValue(recipe.resultEquipId, out var tmpl))
            {
                Color qc = recipe.resultQuality >= 0 && recipe.resultQuality < QualityColors.Length
                    ? QualityColors[recipe.resultQuality] : TextLight;
                string qName = recipe.resultQuality >= 0 && recipe.resultQuality < QualityNames.Length
                    ? QualityNames[recipe.resultQuality] : "";

                _detailNameText.text = tmpl.name;
                _detailNameText.color = qc;
                _detailQualityText.text = $"品质: {qName}";
                _detailQualityText.color = qc;
                _detailAtkText.text = $"攻击力: +{tmpl.baseAtk}";
                _detailDefText.text = $"防御力: +{tmpl.baseDef}";
                _detailHpText.text = $"生命值: +{tmpl.baseHp}";
                _detailLevelText.text = $"需求等级: {tmpl.requireLevel}";
            }
            else
            {
                _detailNameText.text = "未知装备";
                _detailNameText.color = TextLight;
                _detailQualityText.text = "";
                _detailAtkText.text = "";
                _detailDefText.text = "";
                _detailHpText.text = "";
                _detailLevelText.text = "";
            }

            // 材料列表
            var matLines = new System.Text.StringBuilder();
            foreach (var (matId, count) in recipe.materials)
            {
                matLines.AppendLine($"材料 {matId} x {count}");
            }
            _detailMaterialsText.text = matLines.ToString().TrimEnd();

            // 金币消耗
            _detailCostText.text = $"{recipe.cost} 金币";

            UpdateForgeButtonState();
            _resultText.text = "";
        }

        private void UpdateForgeButtonState()
        {
            if (_selectedRecipe == null || _forgeBtn == null) return;
            bool canForge = _playerLevel >= _selectedRecipe.requireLevel && _playerGold >= _selectedRecipe.cost;
            _forgeBtn.interactable = canForge;
            _forgeBtn.GetComponent<Image>().color = canForge ? BtnGreen : BtnDisabled;
        }

        // ================================================================
        // UI 工具方法
        // ================================================================

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
            go.GetComponent<Image>().color = BtnGreen;
            SetAnchor(go.GetComponent<RectTransform>(), anchor, sizeDelta);

            var text = CreateText(go.transform, label, 16, new Vector2(0.5f, 0.5f), sizeDelta, TextLight);
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
