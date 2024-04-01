using FullMoon.FSM;

namespace FullMoon.Entities.Unit.States
{
    public class MeleeUnitIdle : IState
    {
        private readonly MeleeUnitController controller;

        public MeleeUnitIdle(MeleeUnitController controller)
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
            
            controller.StateMachine.ChangeState(new MeleeUnitChase(controller));
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
