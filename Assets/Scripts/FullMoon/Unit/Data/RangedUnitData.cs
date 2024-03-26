using MyBox;
using UnityEngine;

namespace FullMoon.Unit.Data
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "RangedUnit", menuName = "Unit Data/Ranged Unit Data")]
    public class RangedUnitData : BaseUnitData
    {
        [Separator("Ranged Unit Settings")]
    
        [SerializeField, ReadOnly] private float test = 5f;
        public float Test => test;
    }
}