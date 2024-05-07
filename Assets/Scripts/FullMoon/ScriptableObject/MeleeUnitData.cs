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
    }
}