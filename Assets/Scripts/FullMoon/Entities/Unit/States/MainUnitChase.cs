using System.Linq;
using FullMoon.FSM;
using Unity.Burst;

namespace FullMoon.Entities.Unit.States
{
    [BurstCompile]
    public class MainUnitChase : IState
    {
        private readonly MainUnitController controller;

        public MainUnitChase(MainUnitController controller)
        {
            this.controller = controller;
        }

        public void Enter()
        {
            controller.IsStopped = false;
            controller.Agent.speed = controller.OverridenUnitData.MovementSpeed;
            controller.AnimationController.SetAnimation("Move");
        }

        [BurstCompile]
        public void Execute()
        {
            var unitsInView = controller.Flag != null ? controller.Flag.UnitInsideViewArea : controller.UnitInsideViewArea;
            BaseUnitController closestUnit = unitsInView
                .Where(t => t.gameObject.activeInHierarchy && t.Alive)
                .Where(t => !controller.UnitType.Equals(t.UnitType))
                .OrderBy(t => (t.transform.position - controller.transform.position).sqrMagnitude)
                .FirstOrDefault();

            if (closestUnit == null)
            {
                controller.StateMachine.ChangeState(new MainUnitIdle(controller));
                return;
            }

            float sqrAttackRadius = controller.OverridenUnitData.AttackRadius * controller.OverridenUnitData.AttackRadius;
            if ((closestUnit.transform.position - controller.transform.position).sqrMagnitude <= sqrAttackRadius)
            {
                controller.LatestDestination = controller.transform.position;
                controller.StateMachine.ChangeState(new MainUnitAttack(controller));
            }
            else
            {
                controller.Agent.SetDestination(closestUnit.transform.position);
            }
        }

        public void FixedExecute() { }

        public void Exit()
        {
            controller.IsStopped = true;
        }
    }
}