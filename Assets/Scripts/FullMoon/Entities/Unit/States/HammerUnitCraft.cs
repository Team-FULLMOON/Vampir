using FullMoon.FSM;
using FullMoon.UI;
using FullMoon.Util;
using UnityEngine;


namespace FullMoon.Entities.Unit.States
{
    public class HammerUnitCraft : IState
    {
        private readonly HammerUnitController controller;

        public HammerUnitCraft(HammerUnitController controller)
        {
            this.controller = controller;
        }

        public void Enter()
        {
            controller.IsStopped = true;
            controller.AnimationController.SetAnimation("Craft", 0.1f);
        }

        public void Execute() 
        {
        }

        public void FixedExecute() { }

        public void Exit() { }
    }
}
