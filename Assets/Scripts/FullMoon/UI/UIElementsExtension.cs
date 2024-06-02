using DG.Tweening;
using UnityEngine.UIElements;

namespace FullMoon.UI
{
    public static class UIElementsExtensions
    {
        public static void SetVisible(this VisualElement element, bool enabled, float duration = 0f)
        {
            if (enabled)
            {
                element.style.display = DisplayStyle.Flex;
                element.FadeIn(duration);
            }
            else
            {
                element.FadeOut(duration, () => element.style.display = DisplayStyle.None);
            }
        }

        public static void SetEnabledRecursive(this VisualElement element, bool enabled)
        {
            element.SetEnabled(enabled);

            foreach (var child in element.Children())
            {
                child.SetEnabledRecursive(enabled);
            }
        }

        private static void FadeIn(this VisualElement element, float duration)
        {
            element.style.opacity = 0;
            DOTween.To(() => element.resolvedStyle.opacity, x => element.style.opacity = x, 1, duration).SetEase(Ease.Linear);
        }

        private static void FadeOut(this VisualElement element, float duration, System.Action onCompleted = null)
        {
            element.style.opacity = 1;
            DOTween.To(() => element.resolvedStyle.opacity, x => element.style.opacity = x, 0, duration)
                .SetEase(Ease.Linear)
                .OnComplete(() => onCompleted?.Invoke());
        }
    }
}