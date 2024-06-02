using MyBox;
using UnityEngine;

namespace FullMoon.ScriptableObject
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "MainUnit", menuName = "Unit Data/Main Unit Data")]
    public class MainUnitData : BaseUnitData
    {
        [Separator("Main Unit Settings")] 
        
        [SerializeField, OverrideLabel("그로기 시간")] private float groggyTime = 5f;
        public float GroggyTime => groggyTime;
        
        [Separator]
        
        [SerializeField, OverrideLabel("공격 애니메이션 프레임")] private int hitAnimationFrame = 12;
        public int HitAnimationFrame => hitAnimationFrame;
    }
}