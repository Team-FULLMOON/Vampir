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
            if (controller.UnitInsideViewArea.Count == 0)
            {
                return;
            }
            
            controller.StateMachine.ChangeState(new RangedUnitChase(controller));
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
