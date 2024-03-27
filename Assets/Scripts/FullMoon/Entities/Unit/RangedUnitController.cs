using System.Collections.Generic;
using System.Linq;
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

        public RangedUnitData OverridenUnitData  { get; set; }
        
        public List<BaseUnitController> UnitInsideViewArea { get; set; }

        protected override void Start()
        {
            base.Start();
            OverridenUnitData = (RangedUnitData)unitData;
            UnitInsideViewArea = new List<BaseUnitController>();
        }

        public void EnterViewRange(Collider unit)
        {
            BaseUnitController controller = unit.GetComponent<BaseUnitController>();
            if (controller == null)
            {
                return;
            }
            UnitInsideViewArea.Add(controller);
            Debug.Log($"{gameObject.name}: {UnitInsideViewArea.Count}");
        }

        public void ExitViewRange(Collider unit)
        {
            BaseUnitController controller = unit.GetComponent<BaseUnitController>();
            if (controller == null)
            {
                return;
            }
            UnitInsideViewArea.Remove(controller);
            Debug.Log($"{gameObject.name}: {UnitInsideViewArea.Count}");
        }

        public void ExecuteAttack(Transform location)
        {
            // Todo: Object Pooling으로 변경 필요 
            GameObject effect = Instantiate(attackEffect, location.position, Quaternion.identity);
            // effect.GetComponent<ArrowMove>().SetTargetPos(_unitTarget.transform, u_ap, transform);
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying == false)
            {
                return;
            }
            
            BaseUnitController closestUnit  = UnitInsideViewArea
                .Where(t => !unitType.Equals(t.unitType))
                .OrderBy(t => (t.transform.position - transform.position).sqrMagnitude)
                .FirstOrDefault();
            
            if (closestUnit == null)
            {
                return;
            }

            switch (unitType)
            {
                case "Player":
                    Gizmos.color = new Color(0f, 1f, 0f, 1f);
                    break;
                case "Enemy":
                    Gizmos.color = new Color(0f, 0f, 1f, 0.4f);
                    break;
            }
            
            Gizmos.DrawLine(transform.position, closestUnit.transform.position);
        }
    }
}
