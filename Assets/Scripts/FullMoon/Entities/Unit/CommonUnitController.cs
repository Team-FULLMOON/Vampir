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
    public class CommonUnitController : BaseUnitController
    {
        [Foldout("Melee Unit Settings")]
        public DecalProjector decalProjector;

        [Foldout("Melee Unit Settings")]
        public GameObject attackEffect;

        [Foldout("Melee Unit Settings")]
        public GameObject attackPointEffect;

        public MeleeUnitData OverridenUnitData { get; private set; }

        public float CurrentAttackCoolTime { get; set; }
        
        private static readonly int AttackHash = Animator.StringToHash("Attack");

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

            // StateMachine.ChangeState(new MeleeUnitIdle(this));
        }

        [BurstCompile]
        protected override void Update()
        {
            UnitInsideViewArea.RemoveWhere(unit => unit == null || !unit.gameObject.activeInHierarchy || !unit.Alive);
            base.Update();
        }

        public override void Die()
        {
            base.Die();
            // StateMachine.ChangeState(new MeleeUnitDead(this));
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

        public override void MoveToPosition(Vector3 location)
        {
            base.MoveToPosition(location);
            // StateMachine.ChangeState(new MeleeUnitMove(this));
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
