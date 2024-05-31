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
        
        public VisualElement PhaseElement { get; private set; }
        public TextElement PhaseText { get; private set; }
        public TextElement PhaseDetailText { get; private set; }
        
        private void OnEnable()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            
            RetryButton = root.Q<Button>("RetryButton");
            RetryButton.RegisterCallback<ClickEvent>(Retry);
            
            PhaseElement = root.Q<VisualElement>("Phase");
            PhaseText = root.Q<TextElement>("PhaseText");
            PhaseDetailText = root.Q<TextElement>("PhaseDetailText");
            PhaseElement.SetVisible(false);
        }
        
        private void Retry(ClickEvent evt)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
