using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeroQuest.UI.Core
{
    public sealed class UIManager : MonoBehaviour
    {
        private readonly Dictionary<Type, UIView> views = new();

        private void Awake()
        {
            RegisterViewsInChildren();
        }

        public void RegisterViewsInChildren()
        {
            views.Clear();
            foreach (var view in GetComponentsInChildren<UIView>(true))
            {
                views[view.GetType()] = view;
            }
        }

        public TView Show<TView>() where TView : UIView
        {
            var view = Get<TView>();
            view.Show();
            return view;
        }

        public void Hide<TView>() where TView : UIView
        {
            Get<TView>().Hide();
        }

        public TView Get<TView>() where TView : UIView
        {
            if (views.TryGetValue(typeof(TView), out var view))
            {
                return (TView)view;
            }

            throw new InvalidOperationException($"UI view is not registered: {typeof(TView).Name}");
        }
    }
}
