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

        public void Enter()
        {
            controller.AnimationController.SetAnimation("FightIdle");
        }

        [BurstCompile]
        public void Execute()
        {
            var unitsInView = controller.Flag != null ? controller.Flag.UnitInsideViewArea : controller.UnitInsideViewArea;
            int enemyCount = unitsInView.Count(t => !controller.UnitType.Equals(t.UnitType) && t.gameObject.activeInHierarchy && t.Alive);

            if (enemyCount > 0)
            {
                controller.StateMachine.ChangeState(new MainUnitChase(controller));
                return;
            }

            if (controller.Flag != null)
            {
                Vector3 targetPosition = controller.Flag.GetPresetPosition(controller);
                if (Vector3.Distance(controller.transform.position, targetPosition) > controller.Agent.stoppingDistance * 3f)
                {
                    controller.MoveToPosition(targetPosition);
                }
            }
        }

        public void FixedExecute() { }

        public void Exit() { }
    }
}