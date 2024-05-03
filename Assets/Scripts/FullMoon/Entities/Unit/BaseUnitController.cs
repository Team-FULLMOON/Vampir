using MyBox;
using UnityEngine;
using UnityEngine.AI;
using FullMoon.FSM;
using FullMoon.Interfaces;
using FullMoon.ScriptableObject;
using FullMoon.UI;
using FullMoon.Util;

namespace FullMoon.Entities.Unit
{
    public abstract class BaseUnitController
        : MonoBehaviour, IDamageable, ISelectable, INavigation
    {
        [Foldout("Base Unit Settings"), DisplayInspector] 
        public BaseUnitData unitData;
        
        [Foldout("Base Unit Settings")] 
        public GameObject unitModel;
        
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
            Hp = Mathf.Clamp(Hp - amount, 0, System.Int32.MaxValue);

            Debug.Log($"{gameObject.name} ({Hp}): D -{amount}, F {attacker.name}");

            if (Hp > 0)
            {
                return;
            }
            
            Die();
        }

        public virtual void Die()
        {
            gameObject.SetActive(false);
            if (UnitType == "Enemy")
            {
                RespawnController respawnController = ObjectPoolManager.SpawnObject(unitData.UnitRespawnController.gameObject, transform.position, transform.rotation).GetComponent<RespawnController>();
                respawnController.Setup(unitData.ManaCost, unitData.CreatePrepareTime, unitData.SummonTime, unitData.UnitTransformObject);
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