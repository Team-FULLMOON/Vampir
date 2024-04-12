using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using FullMoon.Util;

namespace FullMoon.UI
{
    public class MainUIController : ComponentSingleton<MainUIController>
    {
        private ProgressBar _manaProgressBar;
        private TextElement _manaProgressText;
        private Button _manaExpandButton;

        private void Start()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            _manaProgressBar = root.Q<ProgressBar>("ManaProgressBar");
            _manaProgressText = root.Q<TextElement>("ManaProgressText");
            _manaExpandButton = root.Q<Button>("ManaExpandButton");

            _manaProgressText.text = $"{_manaProgressBar.value} / {_manaProgressBar.highValue}";
            _manaExpandButton.RegisterCallback<ClickEvent>(ExpandMana);
        }
        
        public void AddMana(int value)
        {
            _manaProgressBar.value = Mathf.Clamp(_manaProgressBar.value + value, 0f, _manaProgressBar.highValue);
            _manaProgressText.text = $"{_manaProgressBar.value} / {_manaProgressBar.highValue}";
        }
        
        private void ExpandMana(ClickEvent evt)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
