using System.Linq;
using UnityEngine;
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
        
        public void Enter() { }

        public void Execute()
        {
            int enemyCount = controller.UnitInsideViewArea.Count(t => !controller.UnitType.Equals(t.UnitType));
            
            if (enemyCount == 0)
            {
                if (controller.AttackMove)
                {
                    controller.OnUnitAttack(controller.AttackMovePosition);
                }
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
