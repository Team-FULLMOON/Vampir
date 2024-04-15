
using MyBox;
using UnityEngine;

namespace FullMoon.ScriptableObject
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "MainUnit", menuName = "Unit Data/Main Unit Data")]
    public class MainUnitData : BaseUnitData
    {
        [Separator("Main Unit Settings")]
    
        [SerializeField, OverrideLabel("리스폰 범위")] private float respawnRadius = 3f;
        public float RespawnRadius => respawnRadius;
    }
}