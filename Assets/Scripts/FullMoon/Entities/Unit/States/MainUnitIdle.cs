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
        private static readonly int IdleHash = Animator.StringToHash("Idle");

        public MainUnitIdle(MainUnitController controller)
        {
            this.controller = controller;
        }

        public void Enter()
        {
            controller.SetAnimation(IdleHash);
        }

        [BurstCompile]
        public void Execute()
        {
            var unitsInView = controller.Flag != null ? controller.Flag.UnitInsideViewArea : controller.UnitInsideViewArea;
            int enemyCount = unitsInView.Count(t => !controller.UnitType.Equals(t.UnitType));

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