using System;
using System.Collections.Generic;
using MyBox;
using Unity.Burst;
using UnityEngine;
using UnityEngine.AI;
using FullMoon.Util;
using FullMoon.Interfaces;
using FullMoon.ScriptableObject;
using FullMoon.UI;
using Random = UnityEngine.Random;

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
            
            if (UnitType is "Enemy")
            {
                MainUIController.Instance.ChangeEnemyAmount(1);
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

            if (Enum.TryParse(attacker.UnitClass, out UnitClassFlag attackerClass) == false)
            {
                Debug.LogWarning($"Invalid UnitClass string: {attacker.UnitClass}");
                Hp = Mathf.Clamp(Hp - amount, 0, int.MaxValue);
            }
            else
            {
                // 비트플래그를 사용한 상성 비교
                if ((attackerClass & unitData.UnitCounter) is not 0)
                {
                    amount = (int)(amount * (unitData.CounterDamage / 100f));
                    Hp = Mathf.Clamp(Hp - amount, 0, int.MaxValue);
                }
                else if ((attackerClass & unitData.UnitAdvance) is not 0)
                {
                    float rand = Random.Range(0f, 100f);
                    if (rand > unitData.CounterGuard)
                    {
                        Hp = Mathf.Clamp(Hp - amount, 0, int.MaxValue);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    Hp = Mathf.Clamp(Hp - amount, 0, int.MaxValue);
                }
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
            
            if (UnitType is "Enemy")
            {
                MainUIController.Instance.ChangeEnemyAmount(-1);
                
                if (unitData.RespawnUnitObject == null)
                {
                    return;
                }
                
                for (int i = 0; i < unitData.UnitDrop; i++)
                {
                    Vector2 randomPosition = Random.insideUnitCircle * 1f;
                    Vector3 spawnPosition = new Vector3(transform.position.x + randomPosition.x, transform.position.y, transform.position.z + randomPosition.y);
                    ObjectPoolManager.Instance.SpawnObject(unitData.RespawnUnitObject, spawnPosition, Quaternion.identity);
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