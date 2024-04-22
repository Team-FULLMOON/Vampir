using FullMoon.Entities.Unit;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;

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
        
        [SerializeField, OverrideLabel("유닛 클래스"), DefinedValues("Main", "Ranged", "Melee", "Shield")]
        private string unitClass = "Ranged";
        public string UnitClass => unitClass;
        
        [Separator]
    
        [SerializeField, OverrideLabel("최대 체력")] private int maxHp = 5;
        public int MaxHp => maxHp;
        
        [SerializeField, OverrideLabel("이동 속도")] private float movementSpeed = 5f;
        public float MovementSpeed => movementSpeed;
        
        [Separator]
    
        [SerializeField, OverrideLabel("회피율 (%)")] private float missRate = 50f;
        public float MissRate => missRate;
        
        [Space(5)]
        
        [SerializeField, ConditionalField(nameof(unitType), false, "Enemy"), OverrideLabel("처치 시 마나 획득량")] 
        private int manaDrop = 7;
        public int ManaDrop => manaDrop;
        
        [SerializeField, ConditionalField(nameof(unitType), false, "Enemy"), OverrideLabel("생성 시 마나 필요량")] 
        private int manaCost = 10;
        public int ManaCost => manaCost;
        
        [SerializeField, ConditionalField(nameof(unitType), false, "Enemy"), OverrideLabel("생성 준비 시간")] 
        private float createPrepareTime = 2.5f;
        public float CreatePrepareTime => createPrepareTime;
        
        [SerializeField, ConditionalField(nameof(unitType), false, "Enemy"), OverrideLabel("인형 소환 시간")] 
        private float summonTime = 3f;
        public float SummonTime => summonTime;
        
        [SerializeField, ConditionalField(nameof(unitType), false, "Enemy"), OverrideLabel("유닛 변환 오브젝트")] 
        private GameObject unitTransformObject;
        public GameObject UnitTransformObject => unitTransformObject;
        
        [SerializeField, ConditionalField(nameof(unitType), false, "Enemy"), OverrideLabel("유닛 리스폰 컨트롤러")] 
        private RespawnController unitRespawnController;
        public RespawnController UnitRespawnController => unitRespawnController;
    
        [Separator]
    
        [SerializeField, OverrideLabel("공격 가능 여부")] private bool attackEnabled = true;
        public bool AttackEnabled => attackEnabled;
    
        [SerializeField, ConditionalField(nameof(attackEnabled)), OverrideLabel("공격 당 데미지")]
        private int attackDamage = 1;
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
    
        [SerializeField, OverrideLabel("전장의 안개 반경")] private int fogOfWarRadius = 10;
        public int FogOfWarRadius => fogOfWarRadius;
    }
}