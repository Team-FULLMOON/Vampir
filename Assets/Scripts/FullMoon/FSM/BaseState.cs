using MyBox;
using UnityEngine;
using FullMoon.Interfaces;
using FullMoon.Unit.Data;

namespace FullMoon.FSM
{
    public abstract class BaseUnitState 
        : MonoBehaviour, IDamageable
    {
        [Foldout("Base Unit Settings"), DefinedValues("Player", "Enemy")]
        public string unitType;
        
        [Foldout("Base Unit Settings"), DisplayInspector] 
        public BaseUnitData unitData;
        
        public readonly StateMachine StateMachine = new();
        
        public Rigidbody Rb { get; private set; }
        
        public int Hp { get; set; }

        protected virtual void Start()
        {
            Rb = GetComponent<Rigidbody>();
        }

        public virtual void ReceiveDamage(int amount, BaseUnitState attacker)
        {
            if (unitType.Equals(attacker.unitType))
            {
                return;
            }

            Hp = Mathf.Clamp(Hp - amount, 0, System.Int32.MaxValue);
        }
    }
}
