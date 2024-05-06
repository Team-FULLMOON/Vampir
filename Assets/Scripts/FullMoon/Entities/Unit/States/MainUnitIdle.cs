using System.Linq;
using UnityEngine;
using FullMoon.FSM;
using Unity.Burst;

namespace FullMoon.Entities.Unit.States
{
    [BurstCompile]
    public class MainUnitIdle : IState
    {
        private readonly MainUnitController controller;

        public MainUnitIdle(MainUnitController controller)
        {
            this.controller = controller;
        }
        
        public void Enter() { }

        [BurstCompile]
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
            
            controller.StateMachine.ChangeState(new MainUnitChase(controller));
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
