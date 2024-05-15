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
            int enemyCount = (controller.Flag ? controller.Flag.UnitInsideViewArea : controller.UnitInsideViewArea)
                .Count(t => !controller.UnitType.Equals(t.UnitType));
            
            if (enemyCount > 0)
            {
                controller.StateMachine.ChangeState(new MainUnitChase(controller));
                return;
            }
            
            if (controller.Flag is not null)
            {
                Vector3 targetPosition = controller.Flag.GetPresetPosition(controller);
                if (Vector3.Distance(controller.transform.position, targetPosition) > controller.Agent.stoppingDistance)
                {
                    controller.MoveToPosition(targetPosition);
                }
            }
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
