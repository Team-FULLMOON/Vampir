using MyBox;
using UnityEngine;
using UnityEngine.AI;
using FullMoon.Interfaces;
using FullMoon.Unit.Data;

namespace FullMoon.Entities.Unit
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class RangedUnitController 
        : BaseUnitController, IAttackable
    {
        [Foldout("Ranged Unit Settings")]
        [SerializeField] private GameObject attackEffect;
        
        // [Foldout("Ranged Unit Settings")]
        // [SerializeField] private GameObject ;

        public RangedUnitData OverridenUnitData  { get; set; }

        protected override void Start()
        {
            base.Start();
            OverridenUnitData = (RangedUnitData)unitData;
        }

        public void ExecuteAttack(Transform location)
        {
            // Todo: Object Pooling으로 변경 필요 
            GameObject effect = Instantiate(attackEffect, location.position, Quaternion.identity);
            // effect.GetComponent<ArrowMove>().SetTargetPos(_unitTarget.transform, u_ap, transform);
        }
    }
}
