using System.Linq;
using FullMoon.FSM;

namespace FullMoon.Entities.Unit.States
{
    public class MeleeUnitChase : IState
    {
        private readonly MeleeUnitController controller;

        public MeleeUnitChase(MeleeUnitController controller)
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
            BaseUnitController closestUnit = controller.UnitInsideViewArea
                .Where(t => !controller.UnitType.Equals(t.UnitType))
                .OrderBy(t => (t.transform.position - controller.transform.position).sqrMagnitude)
                .FirstOrDefault();

            if (closestUnit == null)
            {
                controller.StateMachine.ChangeState(new MeleeUnitIdle(controller));
                return;
            }

            bool checkDistance = (closestUnit.transform.position - controller.transform.position).sqrMagnitude <=
                           controller.OverridenUnitData.AttackRadius * controller.OverridenUnitData.AttackRadius;
            
            if (checkDistance)
            {
                controller.LatestDestination = controller.transform.position;
                controller.StateMachine.ChangeState(new MeleeUnitAttack(controller));
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
