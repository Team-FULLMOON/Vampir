using System.Linq;
using UnityEngine;
using FullMoon.FSM;

namespace FullMoon.Entities.Unit.States
{
    public class RangeUnitIdle : IState
    {
        private readonly RangedUnitController controller;

        public RangeUnitIdle(RangedUnitController controller)
        {
            this.controller = controller;
        }
        
        public void Enter()
        {
            Debug.Log($"{controller.name} Idle Enter");
        }

        public void Execute()
        {
            BaseUnitController closestUnit  = controller.UnitInsideViewArea
                .Where(t => !controller.unitType.Equals(t.unitType))
                .OrderBy(t => (t.transform.position - controller.transform.position).sqrMagnitude)
                .FirstOrDefault();
            
            if (closestUnit == null)
            {
                return;
            }
            
            controller.StateMachine.ChangeState(new RangeUnitAttack(controller));
        }

        public void FixedExecute()
        {
            
        }

        public void Exit()
        {
            Debug.Log($"{controller.name} Idle Exit");
        }
    }
}
