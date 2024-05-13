using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FullMoon.FSM;
using Unity.Burst;

namespace FullMoon.Entities.Unit.States
{
    [BurstCompile]
    public class MeleeUnitAttack : IState
    {
        private readonly MeleeUnitController controller;
        private float attackDelay;

        public MeleeUnitAttack(MeleeUnitController controller)
        {
            this.controller = controller;
        }

        public void Enter()
        {
            attackDelay = controller.OverridenUnitData.AttackDelay;
        }

        [BurstCompile]
        public void Execute()
        {
            BaseUnitController closestUnit = (controller.Flag ? controller.Flag.UnitInsideViewArea : controller.UnitInsideViewArea)
                .Where(t => !controller.UnitType.Equals(t.UnitType))
                .OrderBy(t => (t.transform.position - controller.transform.position).sqrMagnitude)
                .FirstOrDefault();

            if (closestUnit == null)
            {
                controller.StateMachine.ChangeState(new MeleeUnitIdle(controller));
                return;
            }
            
            Vector3 targetDirection = closestUnit.transform.position - controller.transform.position;
            controller.transform.forward = targetDirection.normalized;
            controller.transform.eulerAngles = new Vector3(0f, controller.transform.eulerAngles.y, controller.transform.eulerAngles.z);
            
            bool checkDistance = (closestUnit.transform.position - controller.transform.position).sqrMagnitude <=
                                 controller.OverridenUnitData.AttackRadius * controller.OverridenUnitData.AttackRadius;
            
            if (checkDistance == false)
            {
                controller.StateMachine.ChangeState(new MeleeUnitChase(controller));
                return;
            }
            
            if (attackDelay > 0)
            {
                attackDelay -= Time.deltaTime;
                return;
            }
            
            if (controller.CurrentAttackCoolTime > 0)
            {
                return;
            }

            controller.CurrentAttackCoolTime = controller.OverridenUnitData.AttackCoolTime;
            
            controller.ExecuteAttack(closestUnit.transform);
        }

        public void FixedExecute() { }

        public void Exit() { }
    }
}
