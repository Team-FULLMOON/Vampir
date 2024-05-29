using System.Collections.Generic;
using MyBox;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using FullMoon.Interfaces;
using FullMoon.Entities.Unit.States;
using FullMoon.ScriptableObject;
using FullMoon.Util;
using Unity.Burst;
using UnityEngine.Serialization;

namespace FullMoon.Entities.Unit
{
    [RequireComponent(typeof(NavMeshAgent)), BurstCompile]
    public class MainUnitController : BaseUnitController, IAttackable
    {
        [Foldout("Main Unit Settings")]
        public DecalProjector decalProjector;

        [Foldout("Main Unit Settings")]
        public List<GameObject> attackEffects;

        [Foldout("Main Unit Settings")]
        public List<GameObject> attackPointEffects;

        public MainUnitData OverridenUnitData { get; private set; }

        public float CurrentAttackCoolTime { get; set; }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            OverridenUnitData = unitData as MainUnitData;
            CurrentAttackCoolTime = unitData.AttackCoolTime;

            InitializeViewRange();

            if (decalProjector != null)
            {
                InitializeDecalProjector();
            }

            StateMachine.ChangeState(new MainUnitIdle(this));
        }

        [BurstCompile]
        protected override void Update()
        {
            ReduceAttackCoolTime();
            UnitInsideViewArea.RemoveWhere(unit => unit == null || !unit.gameObject.activeInHierarchy || !unit.Alive);
            base.Update();
        }

        public override void Die()
        {
            base.Die();
            StateMachine.ChangeState(new MainUnitDead(this));
        }

        public void EnterViewRange(Collider unit)
        {
            if (unit.TryGetComponent(out BaseUnitController controller))
            {
                UnitInsideViewArea.Add(controller);
            }
        }

        public void ExitViewRange(Collider unit)
        {
            if (unit.TryGetComponent(out BaseUnitController controller))
            {
                UnitInsideViewArea.Remove(controller);
            }
        }

        [BurstCompile]
        public async UniTaskVoid ExecuteAttack(Transform target)
        {
            if (target.TryGetComponent(out BaseUnitController targetController) && targetController.gameObject.activeInHierarchy)
            {
                Vector3 targetDirection = target.position - transform.position;
                Vector3 hitPosition = CalculateHitPosition(targetDirection);

                AlignToTarget(targetDirection);

                int effectType = Random.Range(0, 2);
                
                AnimationController.SetAnimation(effectType == 0 ? "Attack" : "Attack2");

                PlayAttackEffects(effectType, targetDirection, hitPosition);

                await UniTask.DelayFrame(OverridenUnitData.HitAnimationFrame);

                if (targetController.gameObject.activeInHierarchy)
                {
                    targetController.ReceiveDamage(OverridenUnitData.AttackDamage, this);
                }
            }
        }

        public override void Select()
        {
            base.Select();
            decalProjector?.gameObject.SetActive(true);
        }

        public override void Deselect()
        {
            base.Deselect();
            decalProjector?.gameObject.SetActive(false);
        }

        public override void MoveToPosition(Vector3 location)
        {
            base.MoveToPosition(location);
            StateMachine.ChangeState(new MainUnitMove(this));
        }

        private void ReduceAttackCoolTime()
        {
            if (CurrentAttackCoolTime > 0)
            {
                CurrentAttackCoolTime -= Time.deltaTime;
            }
        }

        private void InitializeViewRange()
        {
            if (viewRange != null && unitData != null)
            {
                float worldRadius = viewRange.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
                var triggerEvent = viewRange.GetComponent<ColliderTriggerEvents>();

                if (triggerEvent != null)
                {
                    var units = Physics.OverlapSphere(transform.position + viewRange.center, worldRadius)
                        .Where(t => triggerEvent.GetFilterTags.Contains(t.tag) && t.gameObject != gameObject)
                        .Select(t => t.GetComponent<BaseUnitController>())
                        .Where(unit => unit != null)
                        .ToList();

                    UnitInsideViewArea.UnionWith(units);
                }
            }
        }

        private void InitializeDecalProjector()
        {
            decalProjector.gameObject.SetActive(false);
            decalProjector.size = new Vector3(unitData.AttackRadius * 2f, unitData.AttackRadius * 2f, decalProjector.size.z);
        }

        private void AlignToTarget(Vector3 targetDirection)
        {
            transform.forward = targetDirection.normalized;
            transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, transform.eulerAngles.z);
        }

        private Vector3 CalculateHitPosition(Vector3 targetDirection)
        {
            if (Physics.Raycast(unitModel.transform.position, targetDirection.normalized, out var hit, OverridenUnitData.AttackRadius, 1 << LayerMask.NameToLayer("Unit")))
            {
                return hit.point;
            }
            return targetDirection;
        }

        private void PlayAttackEffects(int effectType, Vector3 targetDirection, Vector3 hitPosition)
        {
            if (attackEffects != null)
            {
                GameObject attackFX = ObjectPoolManager.Instance.SpawnObject(attackEffects[effectType], unitModel.transform.position, Quaternion.identity);
                attackFX.transform.forward = targetDirection.normalized;
            }

            if (attackPointEffects != null)
            {
                GameObject attackPointFX = ObjectPoolManager.Instance.SpawnObject(attackPointEffects[effectType], hitPosition, Quaternion.identity);
                attackPointFX.transform.forward = targetDirection.normalized;
            }
        }

#if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (decalProjector != null)
            {
                decalProjector.size = new Vector3(unitData.AttackRadius * 2f, unitData.AttackRadius * 2f, decalProjector.size.z);
            }
        }
#endif
    }
}
