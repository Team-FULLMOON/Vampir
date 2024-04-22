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

        public ProgressBar ManaProgressBar { get; private set; }
        public TextElement ManaProgressText { get; private set; }
        
        public Button UnitExpandButton { get; private set; }
        public TextElement UnitLimitText { get; private set; }
        
        public Button RetryButton { get; private set; }

        #region Shortcuts

        public Button ShortcutMoveButton { get; private set; }
        public Button ShortcutStopButton { get; private set; }
        public Button ShortcutAttackMoveButton { get; private set; }
        public Button ShortcutHoldButton { get; private set; }
        public Button ShortcutRespawnButton { get; private set; }
        public Button ShortcutCancelButton { get; private set; }

        #endregion
        
        public int ManaValue => (int)ManaProgressBar.value;
        public int UnitLimitValue { get; private set; }
        public int CurrentUnitValue { get; private set; }
        
        private void Start()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            
            ManaProgressBar = root.Q<ProgressBar>("ManaProgressBar");
            ManaProgressText = root.Q<TextElement>("ManaProgressText");
            SetMaxMana(unitControlData.InitMaxMana);
            
            UnitExpandButton = root.Q<Button>("UnitExpandButton");
            UnitLimitText = root.Q<TextElement>("UnitLimitText");
            UnitExpandButton.RegisterCallback<ClickEvent>(ExpandUnit);
            SetUnitLimit(unitControlData.InitUnitLimit);
            
            RetryButton = root.Q<Button>("RetryButton");
            RetryButton.RegisterCallback<ClickEvent>(Retry);
            AddMana(30);

            ShortcutMoveButton = root.Q<Button>("ShortcutMoveButton");
            ShortcutStopButton = root.Q<Button>("ShortcutStopButton");
            ShortcutAttackMoveButton = root.Q<Button>("ShortcutAttackMoveButton");
            ShortcutHoldButton = root.Q<Button>("ShortcutHoldButton");
            ShortcutRespawnButton = root.Q<Button>("ShortcutRespawnButton");
            ShortcutCancelButton = root.Q<Button>("ShortcutCancelButton");
            
            var canCancel = new ReactiveProperty<bool>(false);

            canCancel.Subscribe(cancel =>
            {
                ShortcutMoveButton.SetEnabled(!cancel);
                ShortcutStopButton.SetEnabled(!cancel);
                ShortcutAttackMoveButton.SetEnabled(!cancel);
                ShortcutHoldButton.SetEnabled(!cancel);
                ShortcutRespawnButton.SetEnabled(!cancel);
                ShortcutCancelButton.SetEnabled(cancel);
            });
    
            ShortcutMoveButton.RegisterCallback<ClickEvent>(evt =>
            {
                canCancel.Value = true;
            });
            ShortcutStopButton.RegisterCallback<ClickEvent>(evt =>
            {
                canCancel.Value = true;
            });
            ShortcutAttackMoveButton.RegisterCallback<ClickEvent>(evt =>
            {
                canCancel.Value = true;
            });
            ShortcutHoldButton.RegisterCallback<ClickEvent>(evt =>
            {
                canCancel.Value = true;
            });
            ShortcutRespawnButton.RegisterCallback<ClickEvent>(evt =>
            {
                canCancel.Value = true;
            });
            ShortcutCancelButton.RegisterCallback<ClickEvent>(evt =>
            {
                canCancel.Value = false;
            });
        }
        
        public void AddMana(int value)
        {
            ManaProgressBar.value = Mathf.Clamp(ManaProgressBar.value + value, 0f, ManaProgressBar.highValue);
            ManaProgressText.text = $"{ManaProgressBar.value} / {ManaProgressBar.highValue}";
        }
        
        public void AddUnit(int value)
        {
            CurrentUnitValue = Mathf.Clamp(CurrentUnitValue + value, 0, Int32.MaxValue);
            UnitLimitText.text = $"{CurrentUnitValue} / {UnitLimitValue}";
            UnitLimitText.style.color = new StyleColor(CurrentUnitValue >= UnitLimitValue ? Color.red : Color.white);
        }
        
        private void SetMaxMana(int value)
        {
            ManaProgressBar.highValue = value;
            ManaProgressText.text = $"{ManaProgressBar.value} / {ManaProgressBar.highValue}";
        }

        private void SetUnitLimit(int value)
        {
            UnitLimitValue = value;
            UnitLimitText.text = $"{CurrentUnitValue} / {UnitLimitValue}";
            UnitLimitText.style.color = new StyleColor(CurrentUnitValue >= UnitLimitValue ? Color.red : Color.white);
        }
        
        private void ExpandUnit(ClickEvent evt)
        {
            if (ManaProgressBar.value < unitControlData.UnitLimitExpandCost)
            {
                return;
            }
            AddMana(-unitControlData.UnitLimitExpandCost);
            SetUnitLimit(UnitLimitValue + unitControlData.UnitLimitExpandValue);
        }
        
        private void Retry(ClickEvent evt)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
