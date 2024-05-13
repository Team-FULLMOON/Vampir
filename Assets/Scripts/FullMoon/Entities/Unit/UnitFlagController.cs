using MyBox;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using FullMoon.Util;

namespace FullMoon.Entities.Unit
{
    public class UnitFlagController : MonoBehaviour
    {
        [SerializeField] private SphereCollider viewRange;
        [SerializeField] private List<BaseUnitController> unitPreset;
        private List<Vector3> localPositionsPreset;
        
        public HashSet<BaseUnitController> UnitInsideViewArea { get; set; }
        
        private void OnEnable()
        {
            UnitInsideViewArea = new HashSet<BaseUnitController>();
            InitViewRange();
            SaveLocalPositions();
            foreach (var unit in unitPreset)
            {
                unit.Flag = this;
            }
        }

        private void Update()
        {
            UnitInsideViewArea.RemoveWhere(unit => unit is null || !unit.gameObject.activeInHierarchy || !unit.Alive);
        }

        [ButtonMethod]
        private void AutoFindUnit()
        {
            unitPreset = GetComponentsInChildren<BaseUnitController>().ToList();
            SaveLocalPositions();
        }

        private void SaveLocalPositions()
        {
            localPositionsPreset = unitPreset.Select(u => u.transform.localPosition).ToList();
        }

        [ButtonMethod]
        public void PrintWorldPositions()
        {
            for (int i = 0; i < unitPreset.Count; i++)
            {
                Vector3 worldPosition = transform.TransformPoint(localPositionsPreset[i]);
                unitPreset[i].transform.position = worldPosition;
            }
        }

        private void InitViewRange()
        {
            var triggerEvent = viewRange.GetComponent<ColliderTriggerEvents>();
            if (triggerEvent is not null)
            {
                float worldRadius = viewRange.radius *
                                    Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);

                var units = Physics.OverlapSphere(transform.position + viewRange.center, worldRadius)
                    .Where(t => triggerEvent.GetFilterTags.Contains(t.tag) && t.gameObject != gameObject)
                    .Where(t => t.GetComponent<BaseUnitController>() is not null)
                    .ToList();

                foreach (var unit in units)
                {
                    UnitInsideViewArea.Add(unit.GetComponent<BaseUnitController>());
                }
            }
        }
        
        public void EnterViewRange(Collider unit)
        {
            BaseUnitController controller = unit.GetComponent<BaseUnitController>();
            if (controller is null)
            {
                return;
            }
            UnitInsideViewArea.Add(controller);
        }

        public void ExitViewRange(Collider unit)
        {
            BaseUnitController controller = unit.GetComponent<BaseUnitController>();
            if (controller is null)
            {
                return;
            }
            UnitInsideViewArea.Remove(controller);
        }
        
        public void MoveToPosition(Vector3 newPosition)
        {
            ChangeParentPosition(newPosition);
            for (int i = 0; i < unitPreset.Count; i++)
            {
                Vector3 worldPosition = transform.TransformPoint(localPositionsPreset[i]);
                unitPreset[i].MoveToPosition(worldPosition);
            }
        }
        
        private void ChangeParentPosition(Vector3 newPosition)
        {
            List<Vector3> currentWorldPositions = new List<Vector3>();
            foreach (var t in unitPreset)
            {
                currentWorldPositions.Add(t.transform.position);
            }

            transform.position = newPosition;

            for (int i = 0; i < unitPreset.Count; i++)
            {
                unitPreset[i].transform.position = currentWorldPositions[i];
            }
        }
    }
}
