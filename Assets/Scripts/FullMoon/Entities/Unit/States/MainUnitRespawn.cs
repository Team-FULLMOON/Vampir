using System.Linq;
using UnityEngine;
using FullMoon.FSM;
using FullMoon.UI;

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
            controller.Rb.velocity = Vector3.zero;
            
            RespawnController closestRespawnUnit = controller.RespawnUnitInsideViewArea
                .Where(t => MainUIController.Instance.ManaValue >= t.ManaCost)
                .Where(t => t == controller.ReviveTarget)
                .FirstOrDefault();
                
            if (closestRespawnUnit == null)
            {
                return;
            }
                
            bool checkDistance = (closestRespawnUnit.transform.position - controller.transform.position).sqrMagnitude <=
                                 controller.OverridenUnitData.RespawnRadius * controller.OverridenUnitData.RespawnRadius;
                
            if (checkDistance == false)
            {
                return;
            }
            
            controller.StartSpawn(closestRespawnUnit);
        }

        public void Execute()
        {
            
        }

        public void FixedExecute()
        {
            
        }

        public void Exit()
        {
            controller.CancelSpawn();
        }
    }
}
