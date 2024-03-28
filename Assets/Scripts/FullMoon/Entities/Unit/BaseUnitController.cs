using System;
using FullMoon.Entities.Unit.States;
using MyBox;
using UnityEngine;
using UnityEngine.AI;
using FullMoon.FSM;
using FullMoon.Unit.Data;
using FullMoon.Interfaces;
using Random = UnityEngine.Random;

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

        protected virtual void Update()
        {
            StateMachine.ExecuteCurrentState();
        }

        protected virtual void FixedUpdate()
        {
            StateMachine.FixedExecuteCurrentState();
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
        
        public virtual void MoveToPosition(Vector3 location)
        {
            float variation = 2.5f;
            Vector3 destinationVariation = new Vector3(Random.Range(-variation, variation), 0, Random.Range(-variation, variation));
            Agent.SetDestination(location + destinationVariation);
        }

        private void OnDrawGizmos()
        {
            if (viewRange == null)
            {
                return;
            }
            viewRange.radius = unitData.AttackRange * 2f;
        }
    }
}
