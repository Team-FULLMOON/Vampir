using System.Linq;
using FullMoon.FSM;
using Unity.Burst;
using UnityEngine;

namespace FullMoon.Entities.Unit.States
{
    [BurstCompile]
    public class MeleeUnitChase : IState
    {
        private readonly MeleeUnitController controller;
        private static readonly int MoveHash = Animator.StringToHash("Move");

        public MeleeUnitChase(MeleeUnitController controller)
        {
            this.controller = controller;
        }
        
        public void Enter()
        {
            controller.Agent.isStopped = false;
            controller.Agent.speed = controller.OverridenUnitData.MovementSpeed;
            controller.SetAnimation(MoveHash);
        }

        [BurstCompile]
        public void Execute()
        {
            var unitsInView = controller.Flag != null ? controller.Flag.UnitInsideViewArea : controller.UnitInsideViewArea;
            BaseUnitController closestUnit = unitsInView
                .Where(t => !controller.UnitType.Equals(t.UnitType))
                .OrderBy(t => (t.transform.position - controller.transform.position).sqrMagnitude)
                .FirstOrDefault();

            if (closestUnit == null)
            {
                controller.StateMachine.ChangeState(new MeleeUnitIdle(controller));
                return;
            }

            float sqrAttackRadius = controller.OverridenUnitData.AttackRadius * controller.OverridenUnitData.AttackRadius;
            if ((closestUnit.transform.position - controller.transform.position).sqrMagnitude <= sqrAttackRadius)
            {
                controller.LatestDestination = controller.transform.position;
                controller.StateMachine.ChangeState(new MeleeUnitAttack(controller));
            }
            else
            {
                controller.Agent.SetDestination(closestUnit.transform.position);
            }
        }

        public void FixedExecute() { }

        public void Exit()
        {
            controller.Agent.isStopped = true;
        }
    }
}