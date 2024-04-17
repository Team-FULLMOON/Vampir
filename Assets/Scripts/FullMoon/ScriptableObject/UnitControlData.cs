using MyBox;
using UnityEngine;

namespace FullMoon.ScriptableObject
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "UnitControl", menuName = "Unit Control/Unit Control Data")]
    public class UnitControlData : UnityEngine.ScriptableObject
    {
        [Separator("Unit Control Settings")]
        
        [SerializeField, OverrideLabel("초기 마나량")] private int initMaxMana = 100;
        public int InitMaxMana => initMaxMana;
        
        [SerializeField, OverrideLabel("초기 유닛 제한 수")] private int initUnitLimit = 10;
        public int InitUnitLimit => initUnitLimit;
        
        [Separator]
        
        [SerializeField, OverrideLabel("유닛 제한 확장 필요 마나량")] private int unitLimitExpandCost = 20;
        public int UnitLimitExpandCost => unitLimitExpandCost;
        
        [SerializeField, OverrideLabel("유닛 제한 확장 수")] private int unitLimitExpandValue = 2;
        public int UnitLimitExpandValue => unitLimitExpandValue;
    }
}