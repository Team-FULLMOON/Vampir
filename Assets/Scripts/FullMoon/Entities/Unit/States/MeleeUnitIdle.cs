using System.Linq;
using FullMoon.FSM;

namespace FullMoon.Entities.Unit.States
{
    public class MeleeUnitIdle : IState
    {
        private readonly MeleeUnitController controller;

        public MeleeUnitIdle(MeleeUnitController controller)
        {
            this.controller = controller;
        }
        
        public void Enter()
        {
            // Debug.Log($"{controller.name} Idle Enter");
        }

        public void Execute()
        {
            BaseUnitController closestUnit  = controller.UnitInsideViewArea
                .Where(t => !controller.unitType.Equals(t.unitType))
                .Where(t => (t.transform.position - controller.transform.position).sqrMagnitude <= controller.OverridenUnitData.AttackRange * controller.OverridenUnitData.AttackRange)
                .OrderBy(t => (t.transform.position - controller.transform.position).sqrMagnitude)
                .FirstOrDefault();

            
            if (closestUnit == null)
            {
                return;
            }
            
            // controller.StateMachine.ChangeState(new MeleeUnitAttack(controller));
        }

        public void FixedExecute()
        {
            
        }

        public void Exit()
        {
            // Debug.Log($"{controller.name} Idle Exit");
        }
    }
}
