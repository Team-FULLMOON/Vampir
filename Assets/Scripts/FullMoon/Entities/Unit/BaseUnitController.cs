using MyBox;
using UnityEngine;
using UnityEngine.AI;
using FullMoon.FSM;
using FullMoon.Unit.Data;
using FullMoon.Interfaces;

namespace FullMoon.Entities.Unit
{
    public abstract class BaseUnitController
        : MonoBehaviour, IDamageable, ISelectable, INavigation
    {
        [Foldout("Base Unit Settings"), DefinedValues("Player", "Enemy")]
        public string unitType;
        
        [Foldout("Base Unit Settings"), DisplayInspector] 
        public BaseUnitData unitData;
        
        [Foldout("Base Unit Settings")] 
        public GameObject unitMarker;
        
        [Foldout("Base Unit Settings")] 
        public SphereCollider viewRange;
        
        public readonly StateMachine StateMachine = new();
        
        public Rigidbody Rb { get; private set; }
        public NavMeshAgent Agent { get; set; }
        public int Hp { get; set; }

        protected virtual void Start()
        {
            Rb = GetComponent<Rigidbody>();
            Agent = GetComponent<NavMeshAgent>();
            Hp = unitData.MaxHp;
            unitMarker.SetActive(false);
        }

        public virtual void ReceiveDamage(int amount, BaseUnitController attacker)
        {
            if (unitType.Equals(attacker.unitType))
            {
                return;
            }

            Hp = Mathf.Clamp(Hp - amount, 0, System.Int32.MaxValue);
        }
        
        public void Select()
        {
            unitMarker.SetActive(true);
        }
        
        public void Deselect()
        {
            unitMarker.SetActive(false);
        }
        
        public void MoveToPosition(Vector3 location)
        {
            Agent.SetDestination(location);
        }

        private void OnDrawGizmos()
        {
            viewRange.radius = unitData.AttackRange * 2f;
        }
    }
}
