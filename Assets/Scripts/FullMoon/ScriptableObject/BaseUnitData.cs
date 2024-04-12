using MyBox;
using UnityEngine;

namespace FullMoon.ScriptableObject
{
    [System.Serializable]
    public class BaseUnitData : UnityEngine.ScriptableObject
    {
        [Separator("Base Unit Settings")]
    
        [SerializeField, OverrideLabel("최대 체력")] private int maxHp = 5;
        public int MaxHp => maxHp;
        
        [SerializeField, OverrideLabel("이동 속도")] private float movementSpeed = 5f;
        public float MovementSpeed => movementSpeed;
        
        [Separator]
    
        [SerializeField, OverrideLabel("회피율 (%)")] private float missRate = 50f;
        public float MissRate => missRate;
    
        [Separator]
    
        [SerializeField, OverrideLabel("공격 가능 여부")] private bool attackEnabled = true;
        public bool AttackEnabled => attackEnabled;
    
        [ConditionalField(nameof(attackEnabled)), SerializeField, OverrideLabel("공격 당 데미지")]
        private int attackDamage = 1;
        public int AttackDamage => attackDamage;
        
        [ConditionalField(nameof(attackEnabled)), SerializeField, OverrideLabel("첫 공격 딜레이")]
        private float attackDelay = 1f;
        public float AttackDelay => attackDelay;
    
        [ConditionalField(nameof(attackEnabled)), SerializeField, OverrideLabel("공격 속도")]
        private float attackSpeed = 1f;
        public float AttackSpeed => attackSpeed;
        
        [SerializeField, OverrideLabel("공격 반경")] private float attackRadius = 10f;
        public float AttackRadius => attackRadius;
    
        [Separator]
        
        [SerializeField, OverrideLabel("시야 반경")] private float viewRadius = 10f;
        public float ViewRadius => viewRadius;
    
        [SerializeField, OverrideLabel("전장의 안개 반경")] private float fogOfWarRadius = 20f;
        public float FogOfWarRadius => fogOfWarRadius;
    }
}