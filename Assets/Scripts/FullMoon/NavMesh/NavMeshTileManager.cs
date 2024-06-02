using MyBox;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;
using UnityEditor;

namespace FullMoon.NavMesh
{
    public class NavMeshTileManager : MonoBehaviour
    {
        [DefinedValues(nameof(GetChildGameObjects))]
        public Transform ground;
        
        [DefinedValues(nameof(GetChildGameObjects))]
        public Transform water;
        
        [DefinedValues("Ground", "Water")]
        public string tileType = "Ground";

        private string _currentTileType;
        private NavMeshData _originalNavMeshData;

        [ButtonMethod]
        public void ChangeTileType()
        {
            tileType = tileType == "Ground" ? "Water" : "Ground";
            UpdateTileActivation();
#if UNITY_EDITOR
            BuildAllNavMeshSurfacesInEditor();
#else
            BuildAllNavMeshSurfaces();
#endif
        }
        
        private void Start()
        {
            _currentTileType = tileType;
            UpdateTileActivation();
        }

        private void Update()
        {
            if (_currentTileType != tileType)
            {
                _currentTileType = tileType;
                UpdateTileActivation();
                BuildAllNavMeshSurfaces();
            }
        }

        private Transform[] GetChildGameObjects()
        {
            return GetComponentsInChildren<Transform>(true);
        }

        private void UpdateTileActivation()
        {
            ground.gameObject.SetActive(tileType == "Ground");
            water.gameObject.SetActive(tileType == "Water");
        }

        private void BuildAllNavMeshSurfaces()
        {
            NavMeshSurface[] navMeshSurfaces = FindObjectsOfType<NavMeshSurface>();
            foreach (NavMeshSurface surface in navMeshSurfaces)
            {
                // 원본 NavMeshData를 저장
                _originalNavMeshData = surface.navMeshData;

                // 새로운 NavMeshData 인스턴스를 생성하여 기존 데이터를 덮어쓰지 않도록 함
                surface.navMeshData = new NavMeshData();
                surface.BuildNavMesh();

                // 원본 NavMeshData로 복원
                surface.navMeshData = _originalNavMeshData;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying == false && _currentTileType != tileType)
            {
                _currentTileType = tileType;
                UpdateTileActivation();
                BuildAllNavMeshSurfacesInEditor();
            }
        }

        private void BuildAllNavMeshSurfacesInEditor()
        {
            NavMeshSurface[] navMeshSurfaces = FindObjectsOfType<NavMeshSurface>();
            foreach (NavMeshSurface surface in navMeshSurfaces)
            {
                // 원본 NavMeshData를 저장
                _originalNavMeshData = surface.navMeshData;

                // 새로운 NavMeshData 인스턴스를 생성하여 기존 데이터를 덮어쓰지 않도록 함
                surface.navMeshData = new NavMeshData();
                surface.BuildNavMesh();

                // 원본 NavMeshData로 복원
                surface.navMeshData = _originalNavMeshData;
                EditorUtility.SetDirty(surface);
            }
        }
#endif
    }
}
