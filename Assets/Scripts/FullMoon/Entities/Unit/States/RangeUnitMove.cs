using System.Linq;
using UnityEngine;
using FullMoon.FSM;

namespace FullMoon.Entities.Unit.States
{
    public class RangeUnitMove : IState
    {
        private readonly RangedUnitController controller;

        public RangeUnitMove(RangedUnitController controller)
        {
            this.controller = controller;
        }
        
        public void Enter()
        {
            Debug.Log($"{controller.name} Move Enter");
        }

        public void Execute()
        {
            if (!controller.Agent.pathPending && controller.Agent.remainingDistance <= controller.Agent.stoppingDistance)
            {
                if (!controller.Agent.hasPath)
                {
                    Debug.Log($"{controller.name} Destination reached.");
                    controller.Agent.ResetPath();
                    controller.StateMachine.ChangeState(new RangeUnitIdle(controller));
                }
            }
        }

        public void FixedExecute()
        {
            
        }

        public void Exit()
        {
            Debug.Log($"{controller.name} Move Exit");
        }
    }
}
