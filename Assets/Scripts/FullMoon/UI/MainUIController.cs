using UniRx;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using FullMoon.Util;
using FullMoon.ScriptableObject;

namespace FullMoon.UI
{
    [DefaultExecutionOrder(-1)]
    public class MainUIController : ComponentSingleton<MainUIController>
    {
        [SerializeField] private UnitControlData unitControlData;
        
        public Button RetryButton { get; private set; }
        
        public VisualElement BattlePhase { get; private set; }
        public VisualElement RestPhase { get; private set; }
        public TextElement BattleDetailText { get; private set; }
        public TextElement RestDetailText { get; private set; }
        
        private void OnEnable()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            
            RetryButton = root.Q<Button>("RetryButton");
            RetryButton.RegisterCallback<ClickEvent>(Retry);
            
            BattlePhase = root.Q<VisualElement>("BattlePhase");
            RestPhase = root.Q<VisualElement>("RestPhase");
            
            BattleDetailText = root.Q<TextElement>("BattleDetailText");
            RestDetailText = root.Q<TextElement>("RestDetailText");
            
            BattlePhase.SetVisible(false);
            RestPhase.SetVisible(false);
        }
        
        private void Retry(ClickEvent evt)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
