using System.Collections;
using System.Collections.Generic;
using FullMoon.FSM;
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
            controller.hidePrefab.gameObject.SetActive(true);
            controller.hidePrefab.SetList(true);
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
            controller.hidePrefab.gameObject.SetActive(false);
            controller.hidePrefab.SetList(false);
        }
    }
}
