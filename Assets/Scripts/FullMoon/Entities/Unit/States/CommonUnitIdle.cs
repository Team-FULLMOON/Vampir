using System.Linq;
using UnityEngine;
using FullMoon.FSM;
using Unity.Burst;

namespace FullMoon.Entities.Unit.States
{
    [BurstCompile]
    public class CommonUnitIdle : IState
    {
        private readonly CommonUnitController controller;
        private static readonly int IdleHash = Animator.StringToHash("Idle");

        public CommonUnitIdle(CommonUnitController controller)
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
            if (controller.MainUnit == null)
            {
                return;
            }

            if (controller.UnitInsideViewArea.Contains(controller.MainUnit) == false)
            {
                controller.MoveToPosition(controller.MainUnit.transform.position);
            }
        }

        public void FixedExecute() { }

        public void Exit() { }
    }
}