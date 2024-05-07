using MyBox;
using UnityEngine;
using UnityEngine.AI;
using FullMoon.Interfaces;
using FullMoon.ScriptableObject;
using FullMoon.UI;
using FullMoon.Util;
using Unity.Burst;
using Unity.VisualScripting;
using StateMachine = FullMoon.FSM.StateMachine;

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
        
        public readonly StateMachine StateMachine = new();
        
        public Rigidbody Rb { get; private set; }
        public NavMeshAgent Agent { get; set; }
        public Vector3 LatestDestination { get; set; }
        public int Hp { get; set; }

        public string UnitType { get; set; }
        public string UnitClass { get; set; }

        public bool AttackMove { get; set; }
        public BaseUnitController AttackTarget { get; set; }
        public Vector3 AttackMovePosition { get; set; }
        
        public readonly SimpleEventSystem OnStartEvent = new();

        protected virtual void Start()
        {
            Rb = GetComponent<Rigidbody>();
            Agent = GetComponent<NavMeshAgent>();
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
        
        public void SetAnimation(int stateID, float transitionDuration = 0f)
        {
            if (unitAnimator is null)
            {
                return;
            }
            
            if (unitAnimator.HasState(0, stateID) == false)
            {
                Debug.LogWarning($"{stateID} 애니메이션이 존재하지 않습니다.");
                return;
            }
            
            unitAnimator.Play(stateID, 0, 0);
            unitAnimator.CrossFade(stateID, transitionDuration);
        }

        public virtual void ReceiveDamage(int amount, BaseUnitController attacker)
        {
            Hp = Mathf.Clamp(Hp - amount, 0, System.Int32.MaxValue);
            
            //Debug.Log($"{gameObject.name} ({Hp}): D -{amount}, F {attacker.name}");
            
            SetAnimation(Animator.StringToHash("Hit"));

            if (Hp > 0)
            {
                return;
            }
            
            Die();
        }

        public virtual void Die()
        {
            ObjectPoolManager.ReturnObjectToPool(gameObject);
            if (UnitType == "Enemy")
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector3 randomPosition = transform.position + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
                    ObjectPoolManager.SpawnObject(unitData.RespawnUnitObject.gameObject, randomPosition, Quaternion.identity);
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
            NavMeshPath path = new NavMeshPath();
            Agent.CalculatePath(location, path);
            Agent.SetPath(path);
            LatestDestination = location;
        }

        public virtual void OnUnitStop()
        {
            MoveToPosition(transform.position);
            AttackMove = false;
            AttackTarget = null;
        }

        public virtual void OnUnitHold()
        {
            MoveToPosition(transform.position);
            AttackMove = false;
            AttackTarget = null;
        }

        public virtual void OnUnitAttack(Vector3 targetPosition)
        {
            AttackTarget = null;
            AttackMove = true;
            AttackMovePosition = targetPosition;
            MoveToPosition(targetPosition);
        }

        public virtual void OnUnitForceAttack(BaseUnitController target)
        {
            AttackTarget = target;
            AttackMove = false;
        }

        public virtual void OnUnitStateTransition(Vector3 targetPosition) { }
        
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