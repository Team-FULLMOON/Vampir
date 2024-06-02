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
        public Button RetryButton { get; private set; }
        public Button ExitButton { get; private set; }

        #region Phase
        
        public VisualElement BattlePhase { get; private set; }
        public VisualElement RestPhase { get; private set; }
        public TextElement BattleDetailText { get; private set; }
        public TextElement RestDetailText { get; private set; }

        #endregion

        #region Upper Bar

        public VisualElement BattleIcon { get; private set; }
        public VisualElement RestIcon { get; private set; }
        
        public TextElement DayCountText { get; private set; }
        public TextElement CommonCountText { get; private set; }
        public TextElement WoodCountText { get; private set; }
        public TextElement IronCountText { get; private set; }
        public TextElement EnemyCountText { get; private set; }

        #endregion
        
        public int CommonAmount { get; set; }
        public int WoodAmount { get; set; }
        public int IronAmount { get; set; }
        public int EnemyAmount { get; set; }
        
        private void OnEnable()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            
            RetryButton = root.Q<Button>("RetryButton");
            RetryButton.RegisterCallback<ClickEvent>(Retry);
            ExitButton = root.Q<Button>("ExitButton");
            ExitButton.RegisterCallback<ClickEvent>(Exit);
            
            BattlePhase = root.Q<VisualElement>("BattlePhase");
            RestPhase = root.Q<VisualElement>("RestPhase");
            BattleDetailText = root.Q<TextElement>("BattleDetailText");
            RestDetailText = root.Q<TextElement>("RestDetailText");
            BattlePhase.SetVisible(false);
            RestPhase.SetVisible(false);
            
            BattleIcon = root.Q<VisualElement>("BattleIcon");
            RestIcon = root.Q<VisualElement>("RestIcon");
            BattleIcon.SetVisible(false);
            RestIcon.SetVisible(true);
            
            DayCountText = root.Q<TextElement>("DayCountText");
            CommonCountText = root.Q<TextElement>("CommonCountText");
            WoodCountText = root.Q<TextElement>("WoodCountText");
            IronCountText = root.Q<TextElement>("IronCountText");
            EnemyCountText = root.Q<TextElement>("EnemyCountText");
        }
        
        public void ChangeCommonAmount(int count)
        {
            CommonAmount += count;
            CommonCountText.text = $"{CommonAmount}";
        }
        
        public void ChangeWoodAmount(int count)
        {
            WoodAmount += count;
            WoodCountText.text = $"{WoodAmount}";
        }
        
        public void ChangeIronAmount(int count)
        {
            IronAmount += count;
            IronCountText.text = $"{IronAmount}";
        }
        
        public void ChangeEnemyAmount(int count)
        {
            EnemyAmount += count;
            EnemyCountText.text = $"{EnemyAmount}";
        }
        
        private void Retry(ClickEvent evt)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        private void Exit(ClickEvent evt)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
