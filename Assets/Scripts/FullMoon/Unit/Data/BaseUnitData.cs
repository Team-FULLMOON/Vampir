using MyBox;
using UnityEngine;

namespace FullMoon.Unit.Data
{
    [System.Serializable]
    public class BaseUnitData : ScriptableObject
    {
        [Separator("Base Unit Settings")]
    
        [SerializeField] private int maxHp = 5;
        public int MaxHp => maxHp;
        
        [SerializeField] private int defenseStrength = 5;
        public int DefenceStrength => defenseStrength;
    
        [SerializeField] private float movementSpeed = 5f;
        public float MovementSpeed => movementSpeed;
    
        [Separator]
    
        [SerializeField] private bool attackEnabled = true;
        public bool AttackEnabled => attackEnabled;
    
        [ConditionalField(nameof(attackEnabled)), SerializeField]
        private int attackDamage = 1;
        public int AttackDamage => attackDamage; 
        
        [ConditionalField(nameof(attackEnabled)), SerializeField]
        private float attackDelay = 1f;
        public float AttackDelay => attackDelay;
    
        [ConditionalField(nameof(attackEnabled)), SerializeField]
        private float attackSpeed = 1f;
        public float AttackSpeed => attackSpeed;
    
        [Separator]
    
        [SerializeField] private float fogOfWarRadius = 20f;
        public float FogOfWarRadius => fogOfWarRadius;
    }
}