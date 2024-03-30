using System.Linq;
using FullMoon.Effect;
using FullMoon.FSM;
using UnityEngine;

namespace FullMoon.Entities.Unit.States
{
    public class RangeUnitAttack : IState
    {
        private readonly RangedUnitController controller;
        private float timer;

        public RangeUnitAttack(RangedUnitController controller)
        {
            this.controller = controller;
        }

        public void Enter()
        {
            timer = controller.OverridenUnitData.AttackDelay;
        }

        public void Execute()
        {
            BaseUnitController closestUnit = controller.UnitInsideViewArea
                .Where(t => !controller.unitType.Equals(t.unitType))
                .OrderBy(t => (t.transform.position - controller.transform.position).sqrMagnitude)
                .FirstOrDefault();

            if (closestUnit == null)
            {
                controller.StateMachine.ChangeState(new RangeUnitIdle(controller));
                return;
            }
            
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                return;
            }

            controller.ExecuteAttack(closestUnit.transform);

            timer = controller.OverridenUnitData.AttackSpeed;
        }

        public void FixedExecute()
        {
        }

        public void Exit()
        {
        }
    }
}
