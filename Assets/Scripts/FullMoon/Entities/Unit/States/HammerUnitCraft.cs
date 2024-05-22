using FullMoon.FSM;
using FullMoon.UI;
using FullMoon.Util;
using UnityEngine;


namespace FullMoon.Entities.Unit.States
{
    public class HammerUnitCraft : IState
    {
        private readonly HammerUnitController controller;
        private float craftTimer = 0;

        public HammerUnitCraft(HammerUnitController controller)
        {
            this.controller = controller;
        }

        public void Enter()
        {
            controller.Agent.isStopped = true;
            controller.AnimationController.SetAnimation("Craft", 0.1f);
        }

        public void Execute() 
        {
            craftTimer += Time.deltaTime;
            if (craftTimer >= 3f)
            {
                TileController.Instance.CreateTile(controller.transform.position);
                controller.StateMachine.ChangeState(new HammerUnitIdle(controller));
            }
        }

        public void FixedExecute() { }

        public void Exit() { }
    }
}
