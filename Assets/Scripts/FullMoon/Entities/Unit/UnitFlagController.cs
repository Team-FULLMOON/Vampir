using System;
using MyBox;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using FullMoon.UI;
using FullMoon.Util;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace FullMoon.Entities.Unit
{
    public class UnitFlagController : MonoBehaviour
    {
        [SerializeField] private float viewRangeRadius;
        [SerializeField] private SphereCollider viewRange;
        [SerializeField] private GameObjectDictionary flagModel;
        [SerializeField] private List<BaseUnitController> unitPreset;
        private List<Vector3> localPositionsPreset;
        
        public HashSet<BaseUnitController> UnitInsideViewArea { get; private set; }
        public Vector3 BuildingPosition { get; set; }

        private GameObjectDictionary currentFlagModel;
        private Tween flagMoveTween;

        private void OnEnable()
        {
            UnitInsideViewArea = new HashSet<BaseUnitController>();
            InitViewRange();
            SaveLocalPositions();
            foreach (var unit in unitPreset)
            {
                unit.Flag = this;
                unit.gameObject.SetActive(true);
            }
            ChangeFlagModelPosition();
            InitViewRangeRadius();
            Deselect();
        }

        private async void Update()
        {
            UnitInsideViewArea.RemoveWhere(unit => unit == null || !unit.gameObject.activeInHierarchy || (!unit.Alive && unit is not MainUnitController));
            
            if (unitPreset.All(unit => unit.Alive is false && unit.gameObject.activeInHierarchy is false && unit is not MainUnitController))
            {
                await UniTask.Delay(500);
                if (currentFlagModel != null)
                {
                    ObjectPoolManager.Instance.ReturnObjectToPool(currentFlagModel.gameObject);
                    currentFlagModel = null;
                }
                if (BuildingPosition != Vector3.zero)
                {
                    TileController.Instance.DeleteTile(BuildingPosition);
                    BuildingPosition = Vector3.zero;
                }
                ObjectPoolManager.Instance.ReturnObjectToPool(gameObject);
            }
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
        
        public void Select()
        {
            currentFlagModel.GetGameObjectByName("Decal")?.SetActive(true);
        }
        
        public void Deselect()
        {
            currentFlagModel.GetGameObjectByName("Decal")?.SetActive(false);
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
            ChangeFlagModelPosition();
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

        private void ChangeFlagModelPosition()
        {
            if (currentFlagModel == null)
            {
                currentFlagModel = ObjectPoolManager.Instance.SpawnObject(flagModel.gameObject, new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), Quaternion.identity).GetComponent<GameObjectDictionary>();
                var decalProjector = currentFlagModel.GetGameObjectByName("Decal")?.GetComponent<DecalProjector>();
                if (decalProjector != null)
                {
                    decalProjector.size = new Vector3(viewRangeRadius * 2f, viewRangeRadius * 2f, decalProjector.size.z);
                }
            }

            flagMoveTween?.Kill();

            currentFlagModel.transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
            flagMoveTween = currentFlagModel.transform.DOMove(transform.position, .3f).SetEase(Ease.OutBounce);
        }
        
        private void InitViewRangeRadius()
        {
            if (viewRange != null)
            {
                viewRange.radius = viewRangeRadius;
            }
        }
        
#if UNITY_EDITOR
        protected void OnDrawGizmos()
        {
            InitViewRangeRadius();
        }
#endif
    }
}