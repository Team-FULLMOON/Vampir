using MyBox;
using UnityEngine;

namespace FullMoon.ScriptableObject
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "MainUnit", menuName = "Unit Data/Main Unit Data")]
    public class MainUnitData : BaseUnitData
    {
        [Separator("Main Unit Settings")] 
        
        [SerializeField, OverrideLabel("공격 애니메이션 프레임")] private int hitAnimationFrame = 12;
        public int HitAnimationFrame => hitAnimationFrame;
        
        [SerializeField, OverrideLabel("유닛 컨트롤 데이터")]
        private UnitControlData unitControlData;
        public UnitControlData UnitControlData => unitControlData;
        
        [Separator]
    
        [SerializeField, OverrideLabel("리스폰 범위")] private float respawnRadius = 3f;
        public float RespawnRadius => respawnRadius;
    }
}