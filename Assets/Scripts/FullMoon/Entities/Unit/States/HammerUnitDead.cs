using Cysharp.Threading.Tasks;
using FullMoon.FSM;
using FullMoon.Util;
using UnityEngine;

namespace FullMoon.Entities.Unit.States
{
    public class HammerUnitDead : IState
    {
        private readonly HammerUnitController controller;

        public HammerUnitDead(HammerUnitController controller)
        {
            this.controller = controller;
        }

        public void Enter()
        {
            DisableAfterAnimation("Dead").Forget();
        }

        public void Execute() { }

        public void FixedExecute() { }

        public void Exit() { }

        private async UniTask DisableAfterAnimation(string animationName)
        {
            if (controller.AnimationController.SetAnimation(animationName))
            {
                int animationHash = Animator.StringToHash(animationName);
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