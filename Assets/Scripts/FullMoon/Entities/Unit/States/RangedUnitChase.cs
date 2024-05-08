using System.Collections.Generic;
using System.Linq;
using FullMoon.FSM;
using Unity.Burst;

namespace FullMoon.Entities.Unit.States
{
    [BurstCompile]
    public class RangedUnitChase : IState
    {
        private readonly RangedUnitController controller;

        public RangedUnitChase(RangedUnitController controller)
        {
            this.controller = controller;
        }
        
        public void Enter()
        {
            controller.Agent.isStopped = false;
            controller.Agent.speed = controller.OverridenUnitData.MovementSpeed;
            
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
            BaseUnitController closestUnit = controller.AttackTarget ? controller.AttackTarget : controller.UnitInsideViewArea
                .Where(t => !controller.UnitType.Equals(t.UnitType))
                .OrderBy(t => (t.transform.position - controller.transform.position).sqrMagnitude)
                .FirstOrDefault();

            if (closestUnit == null)
            {
                controller.StateMachine.ChangeState(new RangedUnitIdle(controller));
                return;
            }

            bool checkDistance = (closestUnit.transform.position - controller.transform.position).sqrMagnitude <=
                           controller.OverridenUnitData.AttackRadius * controller.OverridenUnitData.AttackRadius;
            
            if (checkDistance)
            {
                controller.LatestDestination = controller.transform.position;
                controller.StateMachine.ChangeState(new RangedUnitAttack(controller));
            }
            else
            {
                controller.Agent.SetDestination(closestUnit.transform.position);
            }
        }

        public void FixedExecute()
        {
            
        }

        public void Exit()
        {
            controller.Agent.isStopped = true;
        }
    }
}
