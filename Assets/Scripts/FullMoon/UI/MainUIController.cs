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
        private Button _retryButton;

        private void Start()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            _manaProgressBar = root.Q<ProgressBar>("ManaProgressBar");
            _manaProgressText = root.Q<TextElement>("ManaProgressText");
            _manaExpandButton = root.Q<Button>("ManaExpandButton");
            _retryButton = root.Q<Button>("RetryButton");

            _manaProgressText.text = $"{_manaProgressBar.value} / {_manaProgressBar.highValue}";
            _manaExpandButton.RegisterCallback<ClickEvent>(ExpandMana);
            _retryButton.RegisterCallback<ClickEvent>(Retry);
            
            AddMana(30);
        }
        
        public void AddMana(int value)
        {
            _manaProgressBar.value = Mathf.Clamp(_manaProgressBar.value + value, 0f, _manaProgressBar.highValue);
            _manaProgressText.text = $"{_manaProgressBar.value} / {_manaProgressBar.highValue}";
        }
        
        private void ExpandMana(ClickEvent evt)
        {
            if (_manaProgressBar.value < 20)
            {
                return;
            }
            _manaProgressBar.highValue += 10;
            AddMana(-20);
        }
        
        private void Retry(ClickEvent evt)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
