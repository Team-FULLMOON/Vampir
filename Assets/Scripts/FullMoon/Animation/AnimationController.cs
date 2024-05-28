using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FullMoon.Animation
{
    public class AnimationController : MonoBehaviour
    {
        private Animator unitAnimator;
        private CancellationTokenSource cancellationTokenSource;

        private (string, bool) latestStateInfo;

        public void SetAnimator(Animator animator)
        {
            CancelAnimationLoop();
            unitAnimator = animator;
        }
        
        public bool SetAnimation(string stateName, float transitionDuration = 0.3f)
        {
            if (unitAnimator == null)
            {
                return false;
            }

            int stateHash = Animator.StringToHash(stateName);

            if (!unitAnimator.HasState(0, stateHash))
            {
                Debug.LogWarning($"{stateName} 애니메이션이 존재하지 않습니다.");
                return false;
            }

            if (latestStateInfo.Item1 == stateName && latestStateInfo.Item2)
            {
                return true;
            }

            PlayAnimation(stateName, stateHash, transitionDuration);

            return true;
        }

        private void CancelAnimationLoop()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }

        private void PlayAnimation(string stateName, int stateHash, float transitionDuration = 0.3f)
        {
            latestStateInfo = ("", false);
            
            AnimationClip[] clips = unitAnimator.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (clip.name == stateName)
                {
                    latestStateInfo = (clip.name, clip.isLooping);
                    break;
                }
            }   
            
            unitAnimator.Play(stateHash, 0, 0);
            unitAnimator.CrossFade(stateHash, transitionDuration);
        }

        public async UniTaskVoid PlayAnimationAndContinueLoop(string stateName, float transitionDuration = 0.3f)
        {
            if (unitAnimator == null)
            {
                return;
            }
            
            CancelAnimationLoop();
            
            int stateHash = Animator.StringToHash(stateName);

            if (!unitAnimator.HasState(0, stateHash))
            {
                Debug.LogWarning($"{stateName} 애니메이션이 존재하지 않습니다.");
                return;
            }
            
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            string latestStateName = latestStateInfo.Item1;
            bool latestLooping = latestStateInfo.Item2;

            PlayAnimation(stateName, stateHash, transitionDuration);
            
            do
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            } while (!token.IsCancellationRequested && latestLooping && unitAnimator.IsInTransition(0));
            
            if (latestLooping && latestStateInfo.Item1 == stateName)
            {
                PlayAnimation(latestStateName, Animator.StringToHash(latestStateName), transitionDuration);
            }
        }
    }
}
