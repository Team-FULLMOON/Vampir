using System.Linq;
using FullMoon.FSM;

namespace FullMoon.Entities.Unit.States
{
    public class RangedUnitIdle : IState
    {
        private readonly RangedUnitController controller;

        public RangedUnitIdle(RangedUnitController controller)
        {
            this.controller = controller;
        }
        
        public void Enter()
        {
            // Debug.Log($"{controller.name} Idle Enter");
        }

        public void Execute()
        {
            BaseUnitController closestUnit  = controller.UnitInsideViewArea
                .Where(t => !controller.unitType.Equals(t.unitType))
                .OrderBy(t => (t.transform.position - controller.transform.position).sqrMagnitude)
                .FirstOrDefault();
            
            if (closestUnit == null)
            {
                return;
            }
            
            controller.StateMachine.ChangeState(new RangedUnitAttack(controller));
        }

        public void FixedExecute()
        {
            
        }

        public void Exit()
        {
            // Debug.Log($"{controller.name} Idle Exit");
        }
    }
}
