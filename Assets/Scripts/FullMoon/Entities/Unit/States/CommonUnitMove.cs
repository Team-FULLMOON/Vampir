using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Unity.Burst;
using FullMoon.FSM;

namespace FullMoon.Entities.Unit.States
{
    [BurstCompile]
    public class CommonUnitMove : IState
    {
        private readonly CommonUnitController controller;
        private CancellationTokenSource cts;

        public CommonUnitMove(CommonUnitController controller)
        {
            this.controller = controller;
        }
        
        public void Enter()
        {
            controller.Agent.isStopped = true;
            controller.Agent.speed = controller.OverridenUnitData.MovementSpeed;
            
            cts = new CancellationTokenSource();
            Shock(cts.Token).Forget();
        }
        
        private async UniTask Shock(CancellationToken token)
        {
            if (controller.AnimationController.SetAnimation("Shock", 0f))
            {
                int shockHash = Animator.StringToHash("Shock");
                try
                {
                    await UniTask.WaitUntil(() => 
                    {
                        var stateInfo = controller.unitAnimator.GetCurrentAnimatorStateInfo(0);
                        return stateInfo.shortNameHash == shockHash && stateInfo.normalizedTime >= 1f;
                    }, cancellationToken: token);
                }
                catch
                {
                    controller.StateMachine.ChangeState(new CommonUnitIdle(controller));
                    return;
                }
            }
            controller.moveDustEffect.SetActive(true);
            controller.Agent.isStopped = false;
        }
        
        [BurstCompile]
        public void Execute()
        {
            if (!controller.Agent.enabled || controller.Agent.isStopped)
            {
                return;
            }
            
            if (!controller.Agent.pathPending && controller.Agent.remainingDistance <= controller.Agent.stoppingDistance)
            {
                controller.StateMachine.ChangeState(new CommonUnitIdle(controller));
                return;
            }
            
            var unitsInView = controller.UnitInsideViewArea;
            var ownTypeUnits = unitsInView.Where(t => controller.UnitType.Equals(t.UnitType) && t.Agent.isStopped);
            var destination = controller.LatestDestination;
            
            BaseUnitController closestUnit = ownTypeUnits.FirstOrDefault(t =>
                Mathf.Approximately(destination.x, t.LatestDestination.x) &&
                Mathf.Approximately(destination.y, t.LatestDestination.y) &&
                Mathf.Approximately(destination.z, t.LatestDestination.z) &&
                Vector3.Distance(controller.transform.position, destination) <= controller.viewRange.radius &&
                Vector3.Distance(controller.transform.position, t.transform.position) <= 3f);
            
            if (closestUnit != null)
            {
                controller.StateMachine.ChangeState(new CommonUnitIdle(controller));
            }
        }

        public void FixedExecute() { }

        public void Exit()
        {
            cts?.Cancel();
            controller.moveDustEffect.SetActive(false);
            controller.Agent.isStopped = true; 
        }
    }
}