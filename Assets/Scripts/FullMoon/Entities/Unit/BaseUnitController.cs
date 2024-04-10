using MyBox;
using UnityEngine;
using UnityEngine.AI;
using FullMoon.FSM;
using FullMoon.Interfaces;
using FullMoon.ScriptableObject;
using FullMoon.UI;

namespace FullMoon.Entities.Unit
{
    public abstract class BaseUnitController
        : MonoBehaviour, IDamageable, ISelectable, INavigation
    {
        [Foldout("Base Unit Settings"), DefinedValues("None", "Player", "Enemy")]
        public string unitType;

        [Foldout("Base Unit Settings"), DefinedValues("None", "Ranged", "Melee", "Infantry")]
        public string unitClass;
        
        [Foldout("Base Unit Settings"), DisplayInspector] 
        public BaseUnitData unitData;
        
        [Foldout("Base Unit Settings")] 
        public GameObject unitMarker;
        
        [Foldout("Base Unit Settings")] 
        public SphereCollider viewRange;
        
        public readonly StateMachine StateMachine = new();
        
        public Rigidbody Rb { get; private set; }
        public NavMeshAgent Agent { get; set; }
        public Vector3 LatestDestination { get; set; }
        public int Hp { get; set; }

        protected virtual void Start()
        {
            Rb = GetComponent<Rigidbody>();
            Agent = GetComponent<NavMeshAgent>();
            LatestDestination = transform.position;
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
            
            Debug.Log($"{gameObject.name} [{Hp}]: Damage -{amount}, From {attacker.name}");
            
            if (Hp == 0)
            {
                Die(); 
            }
        }

        public virtual void Die()
        {
            gameObject.SetActive(false);
            if (unitType == "Enemy")
            {
                MainUIController.Instance.AddMana(10);
            }
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
            Agent.SetDestination(location);
            LatestDestination = location;
        }

        public virtual void OnUnitStop()
        {
            MoveToPosition(transform.position);
        }

        public virtual void OnUnitHold()
        {
            MoveToPosition(transform.position);
        }

        protected virtual void OnDrawGizmos()
        {
            if (viewRange != null && unitData != null)
            {
                viewRange.radius = unitData.ViewRadius;
            }
        }
    }
}
