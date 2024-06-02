using MyBox;
using UnityEngine;

namespace FullMoon.ScriptableObject
{
    [System.Flags]
    public enum UnitClassFlag
    {
        None = 0,
        Sword = 1 << 0, // 1
        Spear = 1 << 1, // 2
        Crossbow = 1 << 2, // 4
        Main = 1 << 3 // 8
    }
    
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
        
        [SerializeField, OverrideLabel("유닛 클래스"), DefinedValues("Main", "Common", "Hammer", "Sword", "Spear", "Crossbow")]
        private string unitClass = "Main";
        public string UnitClass => unitClass;
        
        [Separator]
    
        [SerializeField, OverrideLabel("최대 체력")] private int maxHp = 100;
        public int MaxHp => maxHp;
        
        [SerializeField, OverrideLabel("이동 속도")] private float movementSpeed = 5f;
        public float MovementSpeed => movementSpeed;
        
        [Separator]
    
        [SerializeField, OverrideLabel("회피율 (%)")] private float missRate = 50f;
        public float MissRate => missRate;
        
        [Space(5)]
        
        [SerializeField, ConditionalField(nameof(unitType), false, "Enemy"), OverrideLabel("유닛 리스폰 오브젝트")] 
        private GameObject respawnUnitObject;
        public GameObject RespawnUnitObject => respawnUnitObject;
    
        [SerializeField, ConditionalField(nameof(unitType), false, "Enemy"), OverrideLabel("자원 유닛 획득량")] 
        private int unitDrop = 5;
        public int UnitDrop => unitDrop;
        
        [Separator]
    
        [SerializeField, OverrideLabel("공격 가능 여부")] private bool attackEnabled = true;
        public bool AttackEnabled => attackEnabled;
    
        [SerializeField, ConditionalField(nameof(attackEnabled)), OverrideLabel("공격 당 데미지")]
        private int attackDamage = 10;
        public int AttackDamage => attackDamage;
        
        [SerializeField, ConditionalField(nameof(attackEnabled)), OverrideLabel("첫 공격 딜레이")]
        private float attackDelay = 1f;
        public float AttackDelay => attackDelay;
    
        [SerializeField, ConditionalField(nameof(attackEnabled)), OverrideLabel("공격 쿨타임")]
        private float attackCoolTime = 1f;
        public float AttackCoolTime => attackCoolTime;
        
        [SerializeField, ConditionalField(nameof(attackEnabled)), OverrideLabel("공격 반경")] 
        private float attackRadius = 10f;
        public float AttackRadius => attackRadius;

        [Separator]
        
        [SerializeField, OverrideLabel("시야 반경")] private float viewRadius = 10f;
        public float ViewRadius => viewRadius;
        
        
        [SerializeField, OverrideLabel("상태 전이 반경")] private float stateTransitionRadius = 10f;
        public float StateTransitionRadius => stateTransitionRadius;

        [Separator]
        
        [SerializeField, OverrideLabel("상성 상 유리한 클래스")]
        private UnitClassFlag unitAdvance = UnitClassFlag.None;
        public UnitClassFlag UnitAdvance => unitAdvance;

        [SerializeField, OverrideLabel("상성 상 불리한 클래스")]
        private UnitClassFlag unitCounter = UnitClassFlag.None;
        public UnitClassFlag UnitCounter => unitCounter;

        [SerializeField, OverrideLabel("카운터에게 입는 데미지(%)")]
        private float counterDamage = 100;
        public float CounterDamage => counterDamage;

        [SerializeField, OverrideLabel("상성에 따른 넉백(m)")]
        private float counterKnockBack = 0;
        public float CounterKnockBack => counterKnockBack;

        [SerializeField, OverrideLabel("상성에 따른 방어 확률(%)")]
        private float counterGuard = 0;
        public float CounterGuard => counterGuard;
    }
}