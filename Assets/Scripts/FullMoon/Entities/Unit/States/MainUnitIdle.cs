using System.Linq;
using UnityEngine;
using FullMoon.FSM;
using FullMoon.Input;
using FullMoon.UI;

namespace FullMoon.Entities.Unit.States
{
    public class MainUnitIdle : IState
    {
        private readonly MainUnitController controller;

        public MainUnitIdle(MainUnitController controller)
        {
            this.controller = controller;
        }
        
        public void Enter()
        {
            controller.Rb.velocity = Vector3.zero;
        }

        public void Execute()
        {
        }

        public void FixedExecute()
        {
            
        }

        public void Exit()
        {
            
        }
    }
}
