using HeroQuest.UI.Core;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace HeroQuest.Editor
{
    /// <summary>
    /// 构建 UGUI Prefab：登录界面、角色选择界面、主 HUD。
    /// 通过菜单 Hero Quest > Build UI Prefabs 一键生成。
    /// </summary>
    public static class UIPrefabBuilder
    {
        private const string PrefabDir = "Assets/Prefabs/UI/";

        [MenuItem("Hero Quest/Build UI Prefabs")]
        public static void BuildAll()
        {
            EnsureDir(PrefabDir);
            BuildLoginPrefab();
            BuildHudPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[UIPrefabBuilder] 所有 UI Prefab 已生成。");
        }

        private static void EnsureDir(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parent = System.IO.Path.GetDirectoryName(path.TrimEnd('/'));
                var name = System.IO.Path.GetFileName(path.TrimEnd('/'));
                if (!AssetDatabase.IsValidFolder(parent)) EnsureDir(parent.Replace('\\', '/') + "/");
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        // === 登录界面 Prefab ===
        private static void BuildLoginPrefab()
        {
            var theme = UIThemeApplier.Current;

            var root = new GameObject("LoginPanel", typeof(RectTransform));
            var canvas = CreateCanvas(root, "LoginCanvas", 100);

            // 全屏半透明背景
            var bg = CreateImage("BG", canvas.transform, theme.panelDark);
            Stretch(bg.rectTransform);

            // 标题
            var title = CreateTMP("Title", bg.transform, "勇者远征", 54, theme.textPrimary);
            SetAnchor(title.rectTransform, new Vector2(0.5f, 0.66f), new Vector2(520, 80));

            // 账号输入
            var accountGO = CreateTMPInput("AccountInput", bg.transform, "账号", theme.slotBackground, theme.textPrimary, theme.textPlaceholder);
            SetAnchor(accountGO.GetComponent<RectTransform>(), new Vector2(0.5f, 0.53f), new Vector2(420, 56));

            // 密码输入
            var passwordGO = CreateTMPInput("PasswordInput", bg.transform, "密码", theme.slotBackground, theme.textPrimary, theme.textPlaceholder, true);
            SetAnchor(passwordGO.GetComponent<RectTransform>(), new Vector2(0.5f, 0.44f), new Vector2(420, 56));

            // 提示
            var hint = CreateTMP("Hint", bg.transform, "测试账号：test / test", 22, theme.textSecondary);
            SetAnchor(hint.rectTransform, new Vector2(0.5f, 0.30f), new Vector2(520, 44));

            // 登录按钮
            var btnGO = CreateButton("LoginButton", bg.transform, "登录", theme.primary, theme.textPrimary, theme.borderAccent);
            SetAnchor(btnGO.GetComponent<RectTransform>(), new Vector2(0.5f, 0.36f), new Vector2(220, 58));

            // 消息文本
            var msg = CreateTMP("MessageText", bg.transform, "", 20, theme.textSecondary);
            SetAnchor(msg.rectTransform, new Vector2(0.5f, 0.22f), new Vector2(520, 30));

            SavePrefab(root, $"{PrefabDir}LoginPanel.prefab");
        }

        // === HUD Prefab ===
        private static void BuildHudPrefab()
        {
            var theme = UIThemeApplier.Current;

            var root = new GameObject("GameplayHUD", typeof(RectTransform));
            var canvas = CreateCanvas(root, "HUDCanvas", 60);

            // 顶部栏
            var topBar = CreatePanel("TopBar", canvas.transform, theme.panelDark, theme.border);
            StretchTop(topBar.rectTransform, 34f);

            var topTitle = CreateTMP("TopTitle", topBar.transform, "勇者远征", 18, theme.textPrimary);
            SetAnchor(topTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(260, 28));

            // 玩家框
            var playerFrame = CreatePanel("PlayerFrame", canvas.transform, theme.panelDark, theme.border);
            SetAnchor(playerFrame.rectTransform, new Vector2(0f, 1f), new Vector2(112, 112), new Vector2(8, -44), new Vector2(0, 1f));

            var portrait = CreateImage("Portrait", playerFrame.transform, theme.slotBackground);
            SetAnchor(portrait.rectTransform, new Vector2(0.5f, 0.66f), new Vector2(78, 64));

            var charName = CreateTMP("CharacterName", playerFrame.transform, "角色", 16, theme.textPrimary);
            SetAnchor(charName.rectTransform, new Vector2(0.5f, 0.31f), new Vector2(100, 20));

            var hpBar = CreateBar("HP", playerFrame.transform, theme.barBackground, theme.hpHigh);
            SetAnchor(hpBar.rectTransform, new Vector2(0.5f, 0.16f), new Vector2(84, 8));

            var mpBar = CreateBar("MP", playerFrame.transform, theme.barBackground, theme.mpFill);
            SetAnchor(mpBar.rectTransform, new Vector2(0.5f, 0.06f), new Vector2(84, 8));

            // 动作条
            var actionBar = CreatePanel("ActionBar", canvas.transform, theme.panelDark, theme.border);
            SetAnchor(actionBar.rectTransform, new Vector2(0.5f, 0f), new Vector2(680, 70), new Vector2(0, 16), new Vector2(0.5f, 0f));

            for (int i = 0; i < 12; i++)
            {
                var slot = CreatePanel($"Slot{i}", actionBar.transform, theme.slotBackground, theme.border);
                SetAnchor(slot.rectTransform, new Vector2(0.03f + i * 0.08f, 0.5f), new Vector2(48, 48), Vector2.zero, new Vector2(0.5f, 0.5f));
                slot.gameObject.AddComponent<Button>();
            }

            SavePrefab(root, $"{PrefabDir}GameplayHUD.prefab");
        }

        // === 工具方法 ===

        private static Canvas CreateCanvas(GameObject root, string canvasName, int sortingOrder)
        {
            var canvasGO = new GameObject(canvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(root.transform, false);
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // EventSystem
            if (!Object.FindFirstObjectByType<EventSystem>())
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }

            return canvas;
        }

        private static Image CreatePanel(string name, Transform parent, Color bg, Color border)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Outline));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = bg;
            var outline = go.GetComponent<Outline>();
            outline.effectColor = border;
            outline.effectDistance = new Vector2(2, -2);
            return img;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            return img;
        }

        private static TMP_Text CreateTMP(string name, Transform parent, string text, float size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            return tmp;
        }

        private static GameObject CreateTMPInput(string name, Transform parent, string placeholder, Color bg, Color textColor, Color phColor, bool password = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = bg;

            var input = go.GetComponent<TMP_InputField>();
            input.contentType = password ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;

            var textGO = new GameObject("Text", typeof(RectTransform));
            textGO.transform.SetParent(go.transform, false);
            var textTMP = textGO.AddComponent<TextMeshProUGUI>();
            textTMP.color = textColor;
            textTMP.fontSize = 24;
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(18, 0); textRect.offsetMax = new Vector2(-18, 0);
            input.textComponent = textTMP;

            var phGO = new GameObject("Placeholder", typeof(RectTransform));
            phGO.transform.SetParent(go.transform, false);
            var phTMP = phGO.AddComponent<TextMeshProUGUI>();
            phTMP.text = placeholder;
            phTMP.color = phColor;
            phTMP.fontSize = 24;
            var phRect = phGO.GetComponent<RectTransform>();
            phRect.anchorMin = Vector2.zero; phRect.anchorMax = Vector2.one;
            phRect.offsetMin = new Vector2(18, 0); phRect.offsetMax = new Vector2(-18, 0);
            input.placeholder = phTMP;

            return go;
        }

        private static GameObject CreateButton(string name, Transform parent, string text, Color bg, Color textColor, Color border)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = bg;
            var outline = go.GetComponent<Outline>();
            outline.effectColor = border;
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            var label = CreateTMP("Label", go.transform, text, 24, textColor);
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero; labelRect.offsetMax = Vector2.zero;

            return go;
        }

        private static Image CreateBar(string name, Transform parent, Color bg, Color fill)
        {
            var bgImg = CreateImage($"{name}Bar", parent, bg);
            var fillGO = new GameObject($"{name}Fill", typeof(RectTransform), typeof(Image));
            fillGO.transform.SetParent(bgImg.transform, false);
            var fillRect = fillGO.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero; fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero; fillRect.offsetMax = Vector2.zero;
            var fillImg = fillGO.GetComponent<Image>();
            fillImg.color = fill;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            return bgImg;
        }

        private static void SetAnchor(RectTransform rect, Vector2 anchor, Vector2 size, Vector2 pos = default, Vector2 pivot = default)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot == default ? new Vector2(0.5f, 0.5f) : pivot;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void StretchTop(RectTransform rect, float height)
        {
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = new Vector2(0, -height);
            rect.offsetMax = Vector2.zero;
        }

        private static void SavePrefab(GameObject root, string path)
        {
            EnsureDir(System.IO.Path.GetDirectoryName(path).Replace('\\', '/') + "/");
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            Debug.Log($"[UIPrefabBuilder] 已保存: {path}");
        }
    }
}
