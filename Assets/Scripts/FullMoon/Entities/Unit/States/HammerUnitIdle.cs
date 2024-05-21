using System.Linq;
using UnityEngine;
using FullMoon.FSM;
using Unity.Burst;

namespace FullMoon.Entities.Unit.States
{
    [BurstCompile]
    public class HammerUnitIdle : IState
    {
        private readonly HammerUnitController controller;

        public HammerUnitIdle(HammerUnitController controller)
        {
            this.controller = controller;
        }

        public void Enter()
        {
            controller.SetAnimation(BaseUnitController.IdleHash);
        }

        public void Execute() { }

        public void FixedExecute() { }

        public void Exit() { }
    }
}