using System.Collections.Generic;
using System.Linq;
using FullMoon.Effect;
using FullMoon.Entities.Unit.States;
using MyBox;
using UnityEngine;
using UnityEngine.AI;
using FullMoon.Interfaces;
using FullMoon.Unit.Data;
using FullMoon.Util;

namespace FullMoon.Entities.Unit
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class RangedUnitController 
        : BaseUnitController, IAttackable
    {
        [Foldout("Ranged Unit Settings")]
        public GameObject attackEffect;

        public RangedUnitData OverridenUnitData  { get; set; }
        
        public List<BaseUnitController> UnitInsideViewArea { get; set; }

        protected override void Start()
        {
            base.Start();
            OverridenUnitData = (RangedUnitData)unitData;
            UnitInsideViewArea = new List<BaseUnitController>();
            StateMachine.ChangeState(new RangeUnitIdle(this));
        }

        public void EnterViewRange(Collider unit)
        {
            BaseUnitController controller = unit.GetComponent<BaseUnitController>();
            if (controller == null)
            {
                return;
            }
            UnitInsideViewArea.Add(controller);
            // Debug.Log($"{gameObject.name}: {UnitInsideViewArea.Count}");
        }

        public void ExitViewRange(Collider unit)
        {
            BaseUnitController controller = unit.GetComponent<BaseUnitController>();
            if (controller == null)
            {
                return;
            }
            UnitInsideViewArea.Remove(controller);
            // Debug.Log($"{gameObject.name}: {UnitInsideViewArea.Count}");
        }

        public void ExecuteAttack(Transform target)
        {
            // Todo: Object Pooling으로 변경 필요 
            GameObject bullet = ObjectPoolManager.SpawnObject(attackEffect, transform.position, Quaternion.identity);
            bullet.GetComponent<BulletEffectController>().Fire(target, transform, OverridenUnitData.BulletSpeed, OverridenUnitData.AttackDamage);
        }

        public override void MoveToPosition(Vector3 location)
        {
            base.MoveToPosition(location);
            StateMachine.ChangeState(new RangeUnitMove(this));
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
