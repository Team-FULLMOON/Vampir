using FullMoon.Util;
using UnityEngine;
using UnityEngine.UIElements;

namespace FullMoon.UI
{
    public class MainUIController : ComponentSingleton<MainUIController>
    {
        private ProgressBar _manaProgressBar;

        private void Start()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            _manaProgressBar = root.Q<ProgressBar>("ManaProgressBar");
        }
        
        public void AddMana(int value)
        {
            _manaProgressBar.value = Mathf.Clamp(_manaProgressBar.value + value, 0f, _manaProgressBar.highValue);
        }
    }
}
