using MyBox;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using FullMoon.Interfaces;
using FullMoon.Entities.Unit.States;
using FullMoon.ScriptableObject;
using FullMoon.Camera;
using FullMoon.Util;

namespace FullMoon.Entities.Unit
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class MeleeUnitController 
        : BaseUnitController, IAttackable
    {
        [Foldout("Melee Unit Settings")]
        public DecalProjector decalProjector;

        public MeleeUnitData OverridenUnitData  { get; set; }
        
        public List<BaseUnitController> UnitInsideViewArea { get; set; }

        [Foldout("Melee Unit Settings"), ConditionalField(nameof(unitClass), false, "Infantry")]
        public GameObject hidePrefab;
        
        [Foldout("Melee Unit Settings"), ConditionalField(nameof(unitClass), false, "Infantry")]
        public bool isGuard;

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
            // Todo
        }

        public override void MoveToPosition(Vector3 location)
        {
            base.MoveToPosition(location);
            StateMachine.ChangeState(new MeleeUnitMove(this));
        }

        public override void OnUnitStop()
        {
            base.OnUnitStop();
            StateMachine.ChangeState(new MeleeUnitIdle(this));
        }

        public override void OnUnitHold()
        {
            if (unitClass == "Infantry")
            {
                base.OnUnitHold();
                StateMachine.ChangeState(new MeleeUnitGuard(this));
            }
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (decalProjector != null)
            {
                decalProjector.size = new Vector3(unitData.AttackRange * 2f, unitData.AttackRange * 2f, decalProjector.size.z);
            }
            
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
