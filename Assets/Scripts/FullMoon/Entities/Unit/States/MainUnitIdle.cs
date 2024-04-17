using System.Linq;
using UnityEngine;
using FullMoon.FSM;
using FullMoon.Input;
using FullMoon.UI;

namespace FullMoon.Entities.Unit.States
{
    public class MainUnitIdle : IState
    {
        private readonly MainUnitController controller;

        public MainUnitIdle(MainUnitController controller)
        {
            this.controller = controller;
        }
        
        public void Enter()
        {
            controller.Rb.velocity = Vector3.zero;
        }

        public void Execute()
        {
            if (PlayerInputManager.Instance.respawn)
            {
                PlayerInputManager.Instance.respawn = false;
                
                if (MainUIController.Instance.CurrentUnitValue >= MainUIController.Instance.UnitLimitValue)
                {
                    return;
                }
                
                RespawnController closestRespawnUnit = controller.RespawnUnitInsideViewArea
                    .Where(t => MainUIController.Instance.ManaValue >= t.ManaCost)
                    .OrderBy(t => (t.transform.position - controller.transform.position).sqrMagnitude)
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
                
                controller.StateMachine.ChangeState(new MainUnitRespawn(controller));
            }
        }

        public void FixedExecute()
        {
            
        }

        public void Exit()
        {
            
        }
    }
}
