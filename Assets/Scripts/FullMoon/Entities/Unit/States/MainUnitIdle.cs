using FullMoon.FSM;

namespace FullMoon.Entities.Unit.States
{
    public class MainUnitIdle : IState
    {
        private readonly MainUnitController controller;

        public MainUnitIdle(MainUnitController controller)
        {
            this.controller = controller;
        }
        
        public void Enter() { }

        public void Execute() { }

        public void FixedExecute() { }

        public void Exit() { }
    }
}
