using System;
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
        
        public HashSet<BaseUnitController> UnitInsideViewArea { get; private set; }
        
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
            UnitInsideViewArea.RemoveWhere(unit => unit == null || !unit.gameObject.activeInHierarchy || (!unit.Alive && unit is not MainUnitController));
        }

        [ButtonMethod]
        private void AutoFindUnit()
        {
            unitPreset = GetComponentsInChildren<BaseUnitController>().ToList();
            SaveLocalPositions();
        }
        
        [ButtonMethod]
        public void InitUnitPositions()
        {
            for (int i = 0; i < unitPreset.Count; i++)
            {
                Vector3 worldPosition = transform.TransformPoint(localPositionsPreset[i]);
                unitPreset[i].transform.position = worldPosition;
            }
        }
        
        public Vector3 GetPresetPosition(BaseUnitController targetObject)
        {
            int index = unitPreset.IndexOf(targetObject);
            if (index == -1)
            {
                throw new ArgumentException("The object does not exist in the preset.");
            }
            Vector3 localPosition = localPositionsPreset[index];
            Vector3 worldPosition = transform.TransformPoint(localPosition);
            return worldPosition;
        }
        
        public void MoveToPosition(Vector3 newPosition)
        {
            ChangeParentPosition(newPosition);
            for (int i = 0; i < unitPreset.Count; i++)
            {
                if (unitPreset[i] is null || unitPreset[i].gameObject.activeInHierarchy == false || unitPreset[i].Alive == false)
                {
                    continue;
                }
                Vector3 worldPosition = transform.TransformPoint(localPositionsPreset[i]);
                unitPreset[i].MoveToPosition(worldPosition);
            }
        }
        
        private void SaveLocalPositions()
        {
            localPositionsPreset = unitPreset.Select(u => u.transform.localPosition).ToList();
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
    }
}
