using MyBox;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using FullMoon.Util;
using FullMoon.Effect;
using FullMoon.Interfaces;
using FullMoon.Entities.Unit.States;
using FullMoon.ScriptableObject;
using Unity.Burst;

namespace FullMoon.Entities.Unit
{
    [RequireComponent(typeof(NavMeshAgent)), BurstCompile]
    public class RangedUnitController : BaseUnitController, IAttackable
    {
        [Foldout("Ranged Unit Settings")]
        public DecalProjector decalProjector;

        [Foldout("Ranged Unit Settings")]
        public GameObject attackEffect;

        public RangedUnitData OverridenUnitData { get; private set; }

        public float CurrentAttackCoolTime { get; set; }

        protected override void OnEnable()
        {
            base.OnEnable();
            OverridenUnitData = unitData as RangedUnitData;
            CurrentAttackCoolTime = unitData.AttackCoolTime;

            InitializeViewRange();

            if (decalProjector != null)
            {
                InitializeDecalProjector();
            }

            StateMachine.ChangeState(new RangedUnitIdle(this));
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
            StateMachine.ChangeState(new RangedUnitDead(this));
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

        public async UniTaskVoid ExecuteAttack(Transform target)
        {
            Vector3 targetDirection = target.position - transform.position;

            AlignToTarget(targetDirection);

            AnimationController.SetAnimation("Attack", 0.1f);

            await UniTask.DelayFrame(OverridenUnitData.HitAnimationFrame);

            FireBullet(target);
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
            StateMachine.ChangeState(new RangedUnitMove(this));
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

        private void FireBullet(Transform target)
        {
            GameObject bullet = ObjectPoolManager.Instance.SpawnObject(attackEffect,
                transform.TransformPoint(GetComponent<CapsuleCollider>().center), Quaternion.identity);
            bullet.GetComponent<BulletEffectController>().Fire(target, transform, OverridenUnitData.BulletSpeed, OverridenUnitData.AttackDamage);
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