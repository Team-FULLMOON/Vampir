using MyBox;
using UnityEngine;

namespace FullMoon.ScriptableObject
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "MeleeUnit", menuName = "Unit Data/Melee Unit Data")]
    public class MeleeUnitData : BaseUnitData
    {
        [Separator("Melee Unit Settings")]
        
        [SerializeField, OverrideLabel("공격 애니메이션 프레임")] private int hitAnimationFrame = 12;
        public int HitAnimationFrame => hitAnimationFrame;
        
        [SerializeField, OverrideLabel("창 밀치기 프레임"), ConditionalField(nameof(UnitType), false, "Spear")] 
        private int spearPushFrame = 2;
        public int SpearPushFrame => spearPushFrame;
        
        [SerializeField, OverrideLabel("창 밀치기 힘"), ConditionalField(nameof(UnitType), false, "Spear")] 
        private float spearPushForce = 10f;
        public float SpearPushForce => spearPushForce;
    }
}