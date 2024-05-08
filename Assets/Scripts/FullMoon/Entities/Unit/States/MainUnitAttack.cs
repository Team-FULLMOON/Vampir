using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FullMoon.FSM;
using Unity.Burst;

namespace FullMoon.Entities.Unit.States
{
    [BurstCompile]
    public class MainUnitAttack : IState
    {
        private readonly MainUnitController controller;
        private float attackDelay;

        public MainUnitAttack(MainUnitController controller)
        {
            this.controller = controller;
        }

        public void Enter()
        {
            attackDelay = controller.OverridenUnitData.AttackDelay;
            
            BaseUnitController closestUnit = controller.AttackTarget ? controller.AttackTarget : controller.UnitInsideViewArea
                .Where(t => !controller.UnitType.Equals(t.UnitType))
                .OrderBy(t => (t.transform.position - controller.transform.position).sqrMagnitude)
                .FirstOrDefault();
            
            if (closestUnit is null)
            {
                return;
            }
            
            controller.OnUnitStateTransition(closestUnit);
        }

        [BurstCompile]
        public void Execute()
        {
            if (controller.AttackTarget is not null && !controller.AttackTarget.gameObject.activeInHierarchy)
            {
                controller.AttackTarget = null;
            }

            BaseUnitController closestUnit = controller.AttackTarget ? controller.AttackTarget : controller.UnitInsideViewArea
                .Where(t => !controller.UnitType.Equals(t.UnitType))
                .OrderBy(t => (t.transform.position - controller.transform.position).sqrMagnitude)
                .FirstOrDefault();

            if (closestUnit == null)
            {
                controller.StateMachine.ChangeState(new MainUnitIdle(controller));
                return;
            }
            
            Vector3 targetDirection = closestUnit.transform.position - controller.transform.position;
            controller.transform.forward = targetDirection.normalized;
            controller.transform.eulerAngles = new Vector3(0f, controller.transform.eulerAngles.y, controller.transform.eulerAngles.z);
            
            bool checkDistance = (closestUnit.transform.position - controller.transform.position).sqrMagnitude <=
                                 controller.OverridenUnitData.AttackRadius * controller.OverridenUnitData.AttackRadius;
            
            if (checkDistance == false)
            {
                controller.StateMachine.ChangeState(new MainUnitChase(controller));
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
