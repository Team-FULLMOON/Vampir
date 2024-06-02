using Unity.Burst;

namespace FullMoon.FSM
{
    [BurstCompile]
    public class StateMachine
    {
        public IState CurrentState { get; private set; }

        public void ChangeState(IState newState)
        {
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
        }

        public void ExecuteCurrentState()
        {
            CurrentState?.Execute();
        }
        
        public void FixedExecuteCurrentState()
        {
            CurrentState?.FixedExecute();
        }
    }
}