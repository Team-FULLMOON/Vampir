using Cysharp.Threading.Tasks;
using UnityEngine;
using FullMoon.FSM;
using FullMoon.Util;

namespace FullMoon.Entities.Unit.States
{
    public class CommonUnitDead : IState
    {
        private readonly CommonUnitController controller;

        public CommonUnitDead(CommonUnitController controller)
        {
            this.controller = controller;
        }

        public void Enter()
        {
            DisableAfterAnimation(BaseUnitController.DeadHash).Forget();
        }

        public void Execute() { }

        public void FixedExecute() { }

        public void Exit() { }

        private async UniTask DisableAfterAnimation(int animationHash)
        {
            controller.Agent.enabled = false;
            if (controller.SetAnimation(animationHash))
            {
                await UniTask.WaitUntil(() => 
                {
                    var stateInfo = controller.unitAnimator.GetCurrentAnimatorStateInfo(0);
                    return stateInfo.shortNameHash == animationHash && stateInfo.normalizedTime >= 1.0f;
                });
                await UniTask.Delay(500);
            }
            ObjectPoolManager.Instance.ReturnObjectToPool(controller.gameObject);
        }
    }
}