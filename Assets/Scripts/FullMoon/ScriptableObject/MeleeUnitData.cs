using MyBox;
using UnityEngine;

namespace FullMoon.ScriptableObject
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "MeleeUnit", menuName = "Unit Data/Melee Unit Data")]
    public class MeleeUnitData : BaseUnitData
    {
        // [Separator("Melee Unit Settings")]
        //
        // [SerializeField] private float bulletSpeed = 70f;
        // public float BulletSpeed => bulletSpeed;
    }
}