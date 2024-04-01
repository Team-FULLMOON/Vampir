using MyBox;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using FullMoon.Util;
using FullMoon.Effect;
using FullMoon.Interfaces;
using FullMoon.Entities.Unit.States;
using FullMoon.ScriptableObject;

namespace FullMoon.Entities.Unit
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class MeleeUnitController 
        : BaseUnitController, IAttackable
    {
        [Foldout("Ranged Unit Settings")]
        public GameObject attackEffect;

        public MeleeUnitData OverridenUnitData  { get; set; }
        
        public List<BaseUnitController> UnitInsideViewArea { get; set; }

        protected override void Start()
        {
            base.Start();
            OverridenUnitData = (MeleeUnitData)unitData;
            UnitInsideViewArea = new List<BaseUnitController>();
            StateMachine.ChangeState(new MeleeUnitIdle(this));
        }

        public void EnterViewRange(Collider unit)
        {
            BaseUnitController controller = unit.GetComponent<BaseUnitController>();
            if (controller == null)
            {
                return;
            }
            UnitInsideViewArea.Add(controller);
        }

        public void ExitViewRange(Collider unit)
        {
            BaseUnitController controller = unit.GetComponent<BaseUnitController>();
            if (controller == null)
            {
                return;
            }
            UnitInsideViewArea.Remove(controller);
        }

        public void ExecuteAttack(Transform target)
        {
            // GameObject bullet = ObjectPoolManager.SpawnObject(attackEffect, transform.position, Quaternion.identity);
            // bullet.GetComponent<BulletEffectController>().Fire(target, transform, OverridenUnitData.BulletSpeed, OverridenUnitData.AttackDamage);
        }

        public override void MoveToPosition(Vector3 location)
        {
            base.MoveToPosition(location);
            // StateMachine.ChangeState(new RangedUnitMove(this));
        }

        protected virtual void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            
            if (Application.isPlaying == false)
            {
                return;
            }
            
            BaseUnitController closestUnit  = UnitInsideViewArea
                .Where(t => !unitType.Equals(t.unitType))
                .Where(t => (t.transform.position - transform.position).sqrMagnitude <= OverridenUnitData.AttackRange * OverridenUnitData.AttackRange)
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
