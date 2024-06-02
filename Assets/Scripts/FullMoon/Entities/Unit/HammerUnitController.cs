using MyBox;
using System.Linq;
using Cysharp.Threading.Tasks;
using FullMoon.Entities.Unit.States;
using UnityEngine;
using UnityEngine.AI;
using FullMoon.ScriptableObject;
using FullMoon.Util;
using Unity.Burst;
using UnityEngine.Rendering.Universal;

namespace FullMoon.Entities.Unit
{
    [RequireComponent(typeof(NavMeshAgent)), BurstCompile]
    public class HammerUnitController : BaseUnitController
    {
        [Foldout("Hammer Unit Settings")]
        public DecalProjector decalProjector;
        
        public HammerUnitData OverridenUnitData { get; private set; }

        protected override void OnEnable()
        {
            base.OnEnable();
            OverridenUnitData = unitData as HammerUnitData;

            InitializeViewRange();
            
            if (decalProjector != null)
            {
                InitializeDecalProjector();
            }
            
            StateMachine.ChangeState(new HammerUnitCraft(this));

            ObjectPoolManager.Instance.ReturnObjectToPool(gameObject, 5f).Forget();
        }

        public override void Die()
        {
            base.Die();
            StateMachine.ChangeState(new HammerUnitDead(this));
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
            StateMachine.ChangeState(new HammerUnitMove(this));
        }

        private void InitializeDecalProjector()
        {
            decalProjector.gameObject.SetActive(false);
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
    }
}
