using UnityEngine.UIElements;

namespace FullMoon.UI
{
    public static class UIElementsExtensions
    {
        public static void SetVisible(this VisualElement element, bool enabled)
        {
            element.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        public static void SetEnabledRecursive(this VisualElement element, bool enabled)
        {
            element.SetEnabled(enabled);

            foreach (var child in element.Children())
            {
                child.SetEnabledRecursive(enabled);
            }
        }
    }
}