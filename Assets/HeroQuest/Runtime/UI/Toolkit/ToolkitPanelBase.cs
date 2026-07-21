using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace HeroQuest.UI.Toolkit
{
    /// <summary>
    /// UI Toolkit 面板基类，管理 UIDocument 的加载、显示和隐藏。
    /// 子类通过 OnPanelLoaded 注册事件回调。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public abstract class ToolkitPanelBase : MonoBehaviour
    {
        protected UIDocument Document { get; private set; }
        protected VisualElement Root { get; private set; }

        protected virtual void Awake()
        {
            Document = GetComponent<UIDocument>();
            if (Document == null)
            {
                Document = gameObject.AddComponent<UIDocument>();
            }
        }

        protected virtual void OnEnable()
        {
            Root = Document.rootVisualElement;
            if (Root == null)
            {
                Debug.LogError($"[{GetType().Name}] UIDocument rootVisualElement 为空，请检查 PanelSettings 和 UXML 配置。");
                return;
            }

            OnPanelLoaded(Root);
        }

        protected virtual void OnDisable()
        {
            OnPanelUnloaded(Root);
        }

        protected abstract void OnPanelLoaded(VisualElement root);

        protected virtual void OnPanelUnloaded(VisualElement root) { }

        protected static T Q<T>(VisualElement root, string name) where T : VisualElement
        {
            return root.Q<T>(name);
        }

        protected static void RegisterClick(VisualElement root, string name, Action callback)
        {
            var btn = root.Q<Button>(name);
            if (btn != null)
            {
                btn.clicked += callback;
            }
            else
            {
                Debug.LogWarning($"[ToolkitPanelBase] 未找到按钮: {name}");
            }
        }
    }
}
