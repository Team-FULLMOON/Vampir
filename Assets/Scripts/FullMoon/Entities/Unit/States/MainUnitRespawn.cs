using UnityEngine;
using FullMoon.FSM;

namespace FullMoon.Entities.Unit.States
{
    public class MainUnitRespawn : IState
    {
        private readonly MainUnitController controller;

        public MainUnitRespawn(MainUnitController controller)
        {
            this.controller = controller;
        }
        
        public void Enter()
        {
            if (controller.ReviveTarget is null || controller.ReviveTarget.gameObject.activeInHierarchy == false)
            {
                return;
            }
            
            bool checkDistance = (controller.ReviveTarget.transform.position - controller.transform.position).sqrMagnitude <=
                                 controller.OverridenUnitData.RespawnRadius * controller.OverridenUnitData.RespawnRadius;
            
            if (checkDistance == false)
            {
                controller.CheckAbleToRespawn(controller.ReviveTarget);
                return;
            }
            
            controller.StartRespawn(controller.ReviveTarget);
        }

        public void Execute()
        {
            
        }

        public void FixedExecute()
        {
            
        }

        public void Exit()
        {
            controller.CancelRespawn();
        }
    }
}
