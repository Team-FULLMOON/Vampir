using MyBox;
using UnityEngine;
using FullMoon.FSM;
using FullMoon.Unit.Data;
using FullMoon.Interfaces;

namespace FullMoon.Entities.Unit
{
    public abstract class BaseUnitController
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
            Hp = unitData.MaxHp;
        }

        public virtual void ReceiveDamage(int amount, BaseUnitController attacker)
        {
            if (unitType.Equals(attacker.unitType))
            {
                return;
            }

            Hp = Mathf.Clamp(Hp - amount, 0, System.Int32.MaxValue);
        }
    }
}
