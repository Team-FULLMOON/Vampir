using System.Linq;
using UnityEngine;
using FullMoon.FSM;

namespace FullMoon.Entities.Unit.States
{
    public class RangedUnitAttack : IState
    {
        private readonly RangedUnitController controller;
        private float timer;

        public RangedUnitAttack(RangedUnitController controller)
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
                controller.StateMachine.ChangeState(new RangedUnitIdle(controller));
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
