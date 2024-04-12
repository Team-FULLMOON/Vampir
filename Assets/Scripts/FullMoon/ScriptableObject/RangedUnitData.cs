using MyBox;
using UnityEngine;

namespace FullMoon.ScriptableObject
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "RangedUnit", menuName = "Unit Data/Ranged Unit Data")]
    public class RangedUnitData : BaseUnitData
    {
        [Separator("Ranged Unit Settings")]
    
        [SerializeField, OverrideLabel("총알 속도")] private float bulletSpeed = 70f;
        public float BulletSpeed => bulletSpeed;
    }
}