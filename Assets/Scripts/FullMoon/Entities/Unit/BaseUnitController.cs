using System.Collections.Generic;
using MyBox;
using Unity.Burst;
using UnityEngine;
using UnityEngine.AI;
using FullMoon.Util;
using FullMoon.Interfaces;
using FullMoon.ScriptableObject;

namespace FullMoon.Entities.Unit
{
    [BurstCompile]
    public abstract class BaseUnitController
        : MonoBehaviour, IDamageable, ISelectable, INavigation
    {
        [Foldout("Base Unit Settings"), DisplayInspector] 
        public BaseUnitData unitData;
        
        [Foldout("Base Unit Settings")] 
        public GameObject unitModel;
        
        [Foldout("Base Unit Settings")] 
        public Animator unitAnimator;
        
        [Foldout("Base Unit Settings")] 
        public SphereCollider viewRange;
        
        public readonly FSM.StateMachine StateMachine = new();
        public readonly Animation.AnimationController AnimationController = new();
        
        public Rigidbody Rb { get; private set; }
        public NavMeshAgent Agent { get; set; }
        public UnitFlagController Flag { get; set; }
        public Vector3 LatestDestination { get; set; }
        public int Hp { get; set; }
        public bool Alive { get; private set; }

        public string UnitType { get; private set; }
        public string UnitClass { get; private set; }
        
        public bool IsStopped
        {
            get => Agent == null || !Agent.enabled || !Agent.isOnNavMesh || Agent.isStopped;
            set
            {
                if (Agent != null && Agent.enabled && Agent.isOnNavMesh)
                {
                    Agent.isStopped = value;
                }
                else
                {
                    Debug.LogWarning("Agent is not enabled or not on NavMesh.");
                }
            }
        }
        
        public HashSet<BaseUnitController> UnitInsideViewArea { get; private set; }

        protected virtual void OnEnable()
        {
            Rb = GetComponent<Rigidbody>();
            Agent = GetComponent<NavMeshAgent>();
            UnitInsideViewArea = new HashSet<BaseUnitController>();
            AnimationController.SetAnimator(unitAnimator);
            
            UnitType = unitData.UnitType;
            UnitClass = unitData.UnitClass;
            
            OnAlive();

            if (viewRange != null && unitData != null)
            {
                viewRange.radius = unitData.ViewRadius;
            }
        }

        [BurstCompile]
        protected virtual void Update()
        {
            UnitInsideViewArea.RemoveWhere(unit => unit == null || !unit.gameObject.activeInHierarchy || (!unit.Alive && unit is not MainUnitController));
            StateMachine.ExecuteCurrentState();
        }

        [BurstCompile]
        protected virtual void FixedUpdate()
        {
            StateMachine.FixedExecuteCurrentState();
        }
        
        public virtual void OnAlive()
        {
            Alive = true;
            Agent.enabled = true;
            LatestDestination = transform.position;
            Hp = unitData.MaxHp;
            UnitInsideViewArea.Clear();
        }

        public virtual void ReceiveDamage(int amount, BaseUnitController attacker)
        {
            if (Alive == false)
            {
                return;
            }

            if (attacker.UnitClass == unitData.UnitCounter)
            {
                amount = (int)(amount / (unitData.CounterDamage / 100));
                Hp = Mathf.Clamp(Hp - amount, 0, System.Int32.MaxValue);
            }
            else if (attacker.UnitClass == unitData.UnitAdvance)
            {
                int rand = Random.Range(0, 100);
                if (rand < unitData.CounterGuard)
                {
                    return;
                }
            }
            else
            {
                Hp = Mathf.Clamp(Hp - amount, 0, System.Int32.MaxValue);
            }
            
            if (unitAnimator != null)
            {
                AnimatorStateInfo stateInfo = unitAnimator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.loop || (AnimationController.CurrentStateInfo.Item1 != "Hit" && stateInfo.normalizedTime >= 0.9f))
                {
                    AnimationController.PlayAnimationAndContinueLoop("Hit").Forget();
                }
            }

            if (Hp <= 0)
            {
                Die();
            }
        }

        public virtual void Die()
        {
            Hp = 0;
            Alive = false;
            Agent.enabled = false;
            
            if (UnitType == "Enemy" && unitData.RespawnUnitObject != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    ObjectPoolManager.Instance.SpawnObject(unitData.RespawnUnitObject, transform.position, Quaternion.identity);
                }
            }
        }

        public virtual void Select()
        {
            switch (UnitType)
            {
                case "Player":
                    unitModel.layer = LayerMask.NameToLayer("SelectPlayer");
                    break;
                case "Enemy":
                    unitModel.layer = LayerMask.NameToLayer("SelectEnemy");
                    break;
            }
        }
        
        public virtual void Deselect()
        {
            unitModel.layer = LayerMask.NameToLayer("Default");
        }

        public virtual void MoveToPosition(Vector3 location)
        {
            if (Alive == false)
            {
                return;
            }

            if (UnityEngine.AI.NavMesh.SamplePosition(location, out var hit, 5.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();
                Agent.CalculatePath(hit.position, path);
                Agent.SetPath(path);
                LatestDestination = location;
            }
        }
        
#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            if (viewRange != null && unitData != null)
            {
                viewRange.radius = unitData.ViewRadius;
            }
        }
#endif
    }
}