using System;
using Cysharp.Threading.Tasks;
using FullMoon.FSM;

namespace FullMoon.Entities.Unit.States
{
    public class MainUnitGroggy : IState
    {
        private readonly MainUnitController controller;
        
        public MainUnitGroggy(MainUnitController controller)
        {
            this.controller = controller;
        }

        public void Enter()
        {
            AliveAfterSeconds("Groggy").Forget();
        }

        public void Execute() { }

        public void FixedExecute() { }

        public void Exit() { }

        private async UniTask AliveAfterSeconds(string animationName)
        {
            controller.AnimationController.SetAnimation(animationName);
            
            await UniTask.Delay(TimeSpan.FromSeconds(controller.OverridenUnitData.GroggyTime));

            controller.OnAlive();
            controller.StateMachine.ChangeState(new MainUnitIdle(controller));
        }
    }
}