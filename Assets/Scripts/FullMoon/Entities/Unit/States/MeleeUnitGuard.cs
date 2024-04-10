using System.Collections;
using System.Collections.Generic;
using FullMoon.Camera;
using FullMoon.FSM;
using FullMoon.Util;
using MyBox;
using UnityEngine;

namespace FullMoon.Entities.Unit.States
{
    public class MeleeUnitGuard : IState
    {
        private readonly MeleeUnitController controller;

        public MeleeUnitGuard(MeleeUnitController controller)
        {
            this.controller = controller;
        }
            
        public void Enter()
        {
            controller.isGuard = true;
            controller.hidePrefab.SetActive(true);
        }

        public void Execute()
        {

        }

        public void FixedExecute()
        {
            
        }

        public void Exit()
        {
            controller.isGuard = false;
            controller.hidePrefab.SetActive(false);
        }
    }
}
