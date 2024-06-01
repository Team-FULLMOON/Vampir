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

namespace FullMoon.Entities.Unit
{
    [RequireComponent(typeof(NavMeshAgent)), BurstCompile]
    public class MeleeUnitController : BaseUnitController, IAttackable
    {
        [Foldout("Melee Unit Settings")]
        public DecalProjector decalProjector;

        [Foldout("Melee Unit Settings")]
        public GameObject attackEffect;

        [Foldout("Melee Unit Settings")]
        public GameObject attackPointEffect;

        public MeleeUnitData OverridenUnitData { get; private set; }

        public float CurrentAttackCoolTime { get; set; }

        protected override void OnEnable()
        {
            base.OnEnable();
            OverridenUnitData = unitData as MeleeUnitData;
            CurrentAttackCoolTime = unitData.AttackCoolTime;

            InitializeViewRange();

            if (decalProjector != null)
            {
                InitializeDecalProjector();
            }

            StateMachine.ChangeState(new MeleeUnitIdle(this));
        }

        [BurstCompile]
        protected override void Update()
        {
            ReduceAttackCoolTime();
            base.Update();
        }

        public override void Die()
        {
            base.Die();
            StateMachine.ChangeState(new MeleeUnitDead(this));
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

                if (targetController.gameObject.activeInHierarchy && targetController.Alive)
                {
                    AnimationController.SetAnimation("Attack", 0.1f);
                    PlayAttackEffects(targetDirection, hitPosition);
                }

                await UniTask.DelayFrame(OverridenUnitData.HitAnimationFrame);

                if (OverridenUnitData.UnitClass.Equals("Spear") && targetController.gameObject.activeInHierarchy && targetController.Alive)
                {
                    targetController.Rb.isKinematic = false;
                    targetController.Rb.AddForce(transform.forward * OverridenUnitData.SpearPushForce, ForceMode.Impulse);

                    await UniTask.DelayFrame(OverridenUnitData.SpearPushFrame);

                    targetController.Rb.isKinematic = true;
                }

                if (targetController.gameObject.activeInHierarchy && targetController.Alive)
                {
                    targetController.ReceiveDamage(OverridenUnitData.AttackDamage, this);
                }
            }
        }

        public override void Select()
        {
            base.Select();
            if (Flag != null)
            {
                Flag.Select();
                return;
            }
            decalProjector?.gameObject.SetActive(true);
        }

        public override void Deselect()
        {
            base.Deselect();
            if (Flag != null)
            {
                Flag.Deselect();
                return;
            }
            decalProjector?.gameObject.SetActive(false);
        }

        public override void MoveToPosition(Vector3 location)
        {
            base.MoveToPosition(location);
            StateMachine.ChangeState(new MeleeUnitMove(this));
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

        private void PlayAttackEffects(Vector3 targetDirection, Vector3 hitPosition)
        {
            if (attackEffect != null)
            {
                GameObject attackFX = ObjectPoolManager.Instance.SpawnObject(attackEffect, unitModel.transform.position, Quaternion.identity);
                attackFX.transform.eulerAngles = unitModel.transform.eulerAngles;
            }

            if (attackPointEffect != null)
            {
                GameObject attackPointFX = ObjectPoolManager.Instance.SpawnObject(attackPointEffect, hitPosition, Quaternion.identity);
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
