using MyBox;
using UnityEngine;
using UnityEngine.AI;
using FullMoon.Interfaces;
using FullMoon.Unit.Data;

namespace FullMoon.Entities.Unit
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class RangedUnitController 
        : BaseUnitController, IAttackable, ISelectable, INavigation
    {
        [Foldout("Ranged Unit Settings")]
        [SerializeField] private GameObject attackEffect;
        
        public NavMeshAgent Agent { get; set; }

        public RangedUnitData OverridenUnitData  { get; set; }

        protected override void Start()
        {
            base.Start();
            Agent = GetComponent<NavMeshAgent>();
            OverridenUnitData = (RangedUnitData)unitData;
        }
        
        public void MoveToPosition(Transform location)
        {
            Agent.SetDestination(location.position);
        }

        public void ExecuteAttack(Transform location)
        {
            // Todo: Object Pooling으로 변경 필요 
            GameObject effect = Instantiate(attackEffect, location.position, Quaternion.identity);
            // effect.GetComponent<ArrowMove>().SetTargetPos(_unitTarget.transform, u_ap, transform);
        }

        public void Select()
        {
            throw new System.NotImplementedException();
        }

        public void ShiftSelect()
        {
            throw new System.NotImplementedException();
        }

        public void DragSelect()
        {
            throw new System.NotImplementedException();
        }
    }
}
