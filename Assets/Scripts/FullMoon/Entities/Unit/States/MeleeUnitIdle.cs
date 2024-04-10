using System.Linq;
using UnityEngine;
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
            controller.Rb.velocity = Vector3.zero;
        }

        public void Execute()
        {
            int enemyCount = controller.UnitInsideViewArea.Count(t => !controller.unitType.Equals(t.unitType));
            
            if (enemyCount == 0)
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
