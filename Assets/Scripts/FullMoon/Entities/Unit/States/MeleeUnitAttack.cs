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
        private BaseUnitController target;
        private float attackDelay;

        public MeleeUnitAttack(MeleeUnitController controller)
        {
            this.controller = controller;
        }

        public void Enter()
        {
            attackDelay = controller.OverridenUnitData.AttackDelay;

            var unitsInView = controller.Flag != null ? controller.Flag.UnitInsideViewArea : controller.UnitInsideViewArea;
            target = unitsInView
                .Where(t => t.gameObject.activeInHierarchy && t.Alive)
                .Where(t => !controller.UnitType.Equals(t.UnitType))
                .OrderBy(t => (t.transform.position - controller.transform.position).sqrMagnitude)
                .FirstOrDefault();
            
            controller.AnimationController.SetAnimation("Idle");
        }

        [BurstCompile]
        public void Execute()
        {
            if (target == null || target.gameObject.activeInHierarchy == false || target.Alive == false)
            {
                controller.StateMachine.ChangeState(new MeleeUnitIdle(controller));
                return;
            }

            Vector3 targetDirection = target.transform.position - controller.transform.position;
            controller.transform.forward = targetDirection.normalized;
            controller.transform.eulerAngles = new Vector3(0f, controller.transform.eulerAngles.y, controller.transform.eulerAngles.z);

            float sqrAttackRadius = controller.OverridenUnitData.AttackRadius * controller.OverridenUnitData.AttackRadius;
            if ((target.transform.position - controller.transform.position).sqrMagnitude > sqrAttackRadius)
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
            controller.ExecuteAttack(target.transform).Forget();
        }

        public void FixedExecute() { }

        public void Exit() { }
    }
}