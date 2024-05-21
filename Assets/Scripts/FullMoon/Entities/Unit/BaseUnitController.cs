using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.AI;
using FullMoon.Interfaces;
using FullMoon.ScriptableObject;
using FullMoon.UI;
using FullMoon.Util;
using Unity.Burst;

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
        public GameObject unitMarker;
        
        [Foldout("Base Unit Settings")] 
        public SphereCollider viewRange;
        
        public readonly FSM.StateMachine StateMachine = new();
        
        public Rigidbody Rb { get; private set; }
        public NavMeshAgent Agent { get; set; }
        public UnitFlagController Flag { get; set; }
        public Vector3 LatestDestination { get; set; }
        public int Hp { get; set; }
        public bool Alive { get; private set; }

        public string UnitType { get; private set; }
        public string UnitClass { get; private set; }
        
        public HashSet<BaseUnitController> UnitInsideViewArea { get; private set; }
        
        public static readonly int IdleHash = Animator.StringToHash("Idle");
        public static readonly int IdleHash2 = Animator.StringToHash("Idle 2");
        public static readonly int AttackHash = Animator.StringToHash("Attack");
        public static readonly int AttackHash2 = Animator.StringToHash("Attack 2");
        public static readonly int HitHash = Animator.StringToHash("Hit");
        public static readonly int ShockHash = Animator.StringToHash("Shock");
        public static readonly int MoveHash = Animator.StringToHash("Move");
        public static readonly int DeadHash = Animator.StringToHash("Dead");

        protected virtual void OnEnable()
        {
            Alive = true;
            Rb = GetComponent<Rigidbody>();
            Agent = GetComponent<NavMeshAgent>();
            UnitInsideViewArea = new HashSet<BaseUnitController>();
            LatestDestination = transform.position;
            Hp = unitData.MaxHp;
            UnitType = unitData.UnitType;
            UnitClass = unitData.UnitClass;
            unitMarker.SetActive(false);

            if (viewRange != null && unitData != null)
            {
                viewRange.radius = unitData.ViewRadius;
            }

            if (UnitType == "Player")
            {
                MainUIController.Instance.AddUnit(1);
            }
        }

        [BurstCompile]
        protected virtual void Update()
        {
            StateMachine.ExecuteCurrentState();
        }

        [BurstCompile]
        protected virtual void FixedUpdate()
        {
            StateMachine.FixedExecuteCurrentState();
        }
        
        public bool SetAnimation(int stateID, float transitionDuration = 0.3f)
        {
            if (unitAnimator is null)
            {
                return false;
            }

            if (unitAnimator.HasState(0, stateID) == false)
            {
                Debug.LogWarning($"{stateID} 애니메이션이 존재하지 않습니다.");
                return false;
            }

            AnimatorStateInfo currentStateInfo = unitAnimator.GetCurrentAnimatorStateInfo(0);
            bool isInTransition = unitAnimator.IsInTransition(0);

            if (isInTransition)
            {
                AnimatorStateInfo nextStateInfo = unitAnimator.GetNextAnimatorStateInfo(0);
                if (nextStateInfo.shortNameHash == stateID && nextStateInfo.loop)
                {
                    return true;
                }
            }
            else
            {
                if (currentStateInfo.shortNameHash == stateID && currentStateInfo.loop)
                {
                    return true;
                }
            }

            unitAnimator.Play(stateID, 0, 0);
            unitAnimator.CrossFade(stateID, transitionDuration);

            return true;
        }

        public virtual void ReceiveDamage(int amount, BaseUnitController attacker)
        {
            if (Alive == false)
            {
                return;
            }
            
            Hp = Mathf.Clamp(Hp - amount, 0, System.Int32.MaxValue);
            
            if (unitAnimator != null)
            {
                AnimatorStateInfo stateInfo = unitAnimator.GetCurrentAnimatorStateInfo(0);

                if ((!stateInfo.shortNameHash.Equals(AttackHash) && !stateInfo.shortNameHash.Equals(HitHash)) || stateInfo.normalizedTime >= 0.9f)
                {
                    SetAnimation(Animator.StringToHash("Hit"));
                }
            }

            if (Hp > 0)
            {
                return;
            }
            
            Die();
        }

        public virtual void Die()
        {
            Alive = false;
            
            if (UnitType == "Enemy" && unitData.RespawnUnitObject != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    ObjectPoolManager.Instance.SpawnObject(unitData.RespawnUnitObject, transform.position, Quaternion.identity);
                }
                MainUIController.Instance.AddMana(unitData.ManaDrop);
                return;
            }
            
            MainUIController.Instance.AddUnit(-1);
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
            
            unitMarker.SetActive(true);
        }
        
        public virtual void Deselect()
        {
            unitModel.layer = LayerMask.NameToLayer("Default");
            unitMarker.SetActive(false);
        }

        public virtual void MoveToPosition(Vector3 location)
        {
            if (Alive == false)
            {
                return;
            }
            
            NavMeshPath path = new NavMeshPath();
            Agent.CalculatePath(location, path);
            Agent.SetPath(path);
            LatestDestination = location;
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