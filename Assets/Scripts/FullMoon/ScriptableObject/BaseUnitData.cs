using MyBox;
using UnityEngine;

namespace FullMoon.ScriptableObject
{
    [System.Serializable]
    public class BaseUnitData : UnityEngine.ScriptableObject
    {
        [Separator("Base Unit Settings")]
        
        [SerializeField, OverrideLabel("유닛 코드")] private string unitCode = "000";
        public string UnitCode => unitCode;
        
        [SerializeField, OverrideLabel("유닛 이름")] private string unitName = "";
        public string UnitName => unitName;
        
        [Separator]
        
        [SerializeField, OverrideLabel("유닛 타입"), DefinedValues("Player", "Enemy")]
        private string unitType = "Player";
        public string UnitType => unitType;
        
        [SerializeField, OverrideLabel("유닛 클래스"), DefinedValues("Ranged", "Melee", "Infantry")]
        private string unitClass = "Ranged";
        public string UnitClass => unitClass;
        
        [Separator]
    
        [SerializeField, OverrideLabel("최대 체력")] private int maxHp = 5;
        public int MaxHp => maxHp;
        
        [SerializeField, OverrideLabel("이동 속도")] private float movementSpeed = 5f;
        public float MovementSpeed => movementSpeed;
        
        [SerializeField, ConditionalField(nameof(unitType), false, "Enemy"), OverrideLabel("처치 시 마나 획득량")] 
        private int manaDrop = 7;
        public int ManaDrop => manaDrop;
        
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