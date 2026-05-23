using UnityEngine;

namespace HeroQuest.UI.Core
{
    public abstract class UIView : MonoBehaviour
    {
        public virtual void Show()
        {
            gameObject.SetActive(true);
            OnShown();
        }

        public virtual void Hide()
        {
            OnHidden();
            gameObject.SetActive(false);
        }

        protected virtual void OnShown()
        {
        }

        protected virtual void OnHidden()
        {
        }
    }
}
