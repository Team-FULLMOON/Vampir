using MyBox;
using UnityEngine;

namespace FullMoon.ScriptableObject
{
    [System.Serializable]
    public class BaseBuildingData : UnityEngine.ScriptableObject
    {
        [Separator("Base Building Settings")]

        [SerializeField, OverrideLabel("건물 코드")] private string buildingCode = "000";
        public string BuildingCode => buildingCode;

        [SerializeField, OverrideLabel("건물 이름")] private string buildingName = "";
        public string BuildingName => buildingName;

        [SerializeField, OverrideLabel("건물 타입"), DefinedValues("LumberMill", "SwordArmy", "SpearArmy", "CrossbowArmy")] 
        private string buildingType = "";
        public string BuildingType => buildingType;
        
        [Separator]
        
        [SerializeField, OverrideLabel("건물 생성 시간")] private float buildTime = 5f;
        public float BuildTime => buildTime;

        [Separator]

        [SerializeField, OverrideLabel("건물 최대 체력")] private int maxHp = 1000;
        public int MaxHp => maxHp;

        [SerializeField, OverrideLabel("건물 방어력")] private int defAmount = 0;
        public int DefAmount => defAmount;
    }
}
