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

namespace FullMoon.Entities.Unit
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class MeleeUnitController 
        : BaseUnitController, IAttackable
    {
        [Foldout("Melee Unit Settings")]
        public DecalProjector decalProjector;

        public MeleeUnitData OverridenUnitData { get; private set; }
        
        public List<BaseUnitController> UnitInsideViewArea { get; set; }

        [Foldout("Melee Unit Settings"), ConditionalField(nameof(UnitClass), false, "Infantry")]
        public CoverController hidePrefab;
        
        [Foldout("Melee Unit Settings"), ConditionalField(nameof(UnitClass), false, "Infantry")]
        public bool isGuard;

        protected override void Start()
        {
            base.Start();
            OverridenUnitData = (MeleeUnitData)unitData;
            UnitInsideViewArea = new List<BaseUnitController>();
            StateMachine.ChangeState(new MeleeUnitIdle(this));
        }
        
        protected void LateUpdate()
        {
            UnitInsideViewArea.RemoveAll(unit => unit == null || !unit.gameObject.activeInHierarchy);
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
            BaseUnitController targetController = target.GetComponent<BaseUnitController>();

            if (targetController == null || targetController.gameObject.activeInHierarchy == false)
            {
                return;
            }

            targetController.ReceiveDamage(OverridenUnitData.AttackDamage, this);
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
            if (UnitClass == "Infantry")
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
                decalProjector.size = new Vector3(unitData.AttackRadius * 2f, unitData.AttackRadius * 2f, decalProjector.size.z);
            }
        }
    }
}
