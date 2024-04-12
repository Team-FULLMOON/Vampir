using FullMoon.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace FullMoon.UI
{
    public class MainUIController : ComponentSingleton<MainUIController>
    {
        private ProgressBar _manaProgressBar;
        private Button _manaExpandBtn;

        private void Start()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            _manaProgressBar = root.Q<ProgressBar>("ManaProgressBar");
            _manaExpandBtn = root.Q<Button>("ManaExpandButton");
            
            _manaExpandBtn.RegisterCallback<ClickEvent>(ExpandMana);
        }
        
        public void AddMana(int value)
        {
            _manaProgressBar.value = Mathf.Clamp(_manaProgressBar.value + value, 0f, _manaProgressBar.highValue);
        }
        
        private void ExpandMana(ClickEvent evt)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
