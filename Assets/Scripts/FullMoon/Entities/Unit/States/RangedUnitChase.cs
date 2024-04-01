using System.Linq;
using FullMoon.FSM;

namespace FullMoon.Entities.Unit.States
{
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
        }

        public void Execute()
        {
            BaseUnitController closestUnit  = controller.UnitInsideViewArea
                .Where(t => !controller.unitType.Equals(t.unitType))
                .OrderBy(t => (t.transform.position - controller.transform.position).sqrMagnitude)
                .FirstOrDefault();

            if (closestUnit == null)
            {
                controller.StateMachine.ChangeState(new RangedUnitIdle(controller));
                return;
            }

            bool checkDistance = (closestUnit.transform.position - controller.transform.position).sqrMagnitude <=
                           controller.OverridenUnitData.AttackRange * controller.OverridenUnitData.AttackRange;
            
            if (checkDistance)
            {
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
