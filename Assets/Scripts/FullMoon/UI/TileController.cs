using MyBox;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.Tilemaps;
using FullMoon.Util;
using FullMoon.Camera;
using FullMoon.NavMesh;

namespace FullMoon.UI
{
    public enum BuildingType
    {
        None,
        Ground,         // 타일
        LumberMill,     // 벌목소
        SwordArmy,      // 훈련소(검)
        SpearArmy,      // 훈련소(창)
        CrossbowArmy,   // 훈련소(석궁)
    }

    public class TileController : ComponentSingleton<TileController>
    {
        [Separator("TileMap")]

        [SerializeField, OverrideLabel("타일 프리팹")]
        private GameObject tilePrefab;

        [SerializeField, OverrideLabel("타일 가로 사이즈")]
        private int tileWidthSize = 1;

        [SerializeField, OverrideLabel("타일 세로 사이즈")]
        private int tileHeightSize = 1;

        [Separator]

        [SerializeField, OverrideLabel("벌목소 프리팹")] 
        private GameObject lumberPrefab;

        [SerializeField, OverrideLabel("벌목소 가로 사이즈")]
        private int lumberWidthSize = 1;

        [SerializeField, OverrideLabel("벌목소 세로 사이즈")]
        private int lumberHeightSize = 1;

        [Separator]

        [SerializeField, OverrideLabel("훈련소(검) 프리팹")]
        private GameObject swordArmyPrefab;

        [SerializeField, OverrideLabel("훈련소(검) 가로 사이즈")]
        private int swordWidthSize = 1;

        [SerializeField, OverrideLabel("훈련소(검) 세로 사이즈")]
        private int swordHeightSize = 1;

        [Separator]

        [SerializeField, OverrideLabel("훈련소(창) 프리팹")]
        private GameObject spearArmyPrefab;

        [SerializeField, OverrideLabel("훈련소(창) 가로 사이즈")]
        private int spearWidthSize = 1;

        [SerializeField, OverrideLabel("훈련소(창) 세로 사이즈")]
        private int spearHeightSize = 1;

        [Separator]

        [SerializeField, OverrideLabel("훈련소(석궁) 프리팹")]
        private GameObject crossbowArmyPrefab;

        [SerializeField, OverrideLabel("훈련소(석궁) 가로 사이즈")]
        private int crossbowWidthSize = 1;

        [SerializeField, OverrideLabel("훈련소(석궁) 세로 사이즈")]
        private int crossbowHeightSize = 1;

        [Separator]

        [SerializeField, OverrideLabel("건설 UI 버튼")]
        private GameObject buildUIButton;

        [SerializeField, OverrideLabel("건설 UI")]
        private GameObject buildUI;

        [SerializeField, OverrideLabel("건설 UI 취소 버튼")]
        private GameObject cancelBuildUI;

        [Separator]

        public NavMeshSurface playerSurfaces;
        public NavMeshSurface enemySurfaces;
        
        private List<NavMeshBuildSource> playerSources = new();
        private List<NavMeshBuildSource> enemySources = new();

        private Tilemap buildingTileMap;
        private Tilemap groundTileMap;
        private Tilemap baseTile;
        private CameraController cameraController;

        private void Start()
        {
            buildingTileMap = GetComponent<GameObjectDictionary>().GetComponentByName<Tilemap>("Building");
            groundTileMap = GetComponent<GameObjectDictionary>().GetComponentByName<Tilemap>("Ground");
            cameraController = FindObjectOfType<CameraController>();
            
            if (playerSurfaces == null)
            {
                playerSurfaces = GameObject.Find("Player Navmesh Surface").GetComponent<NavMeshSurface>();
            }
            
            if (enemySurfaces == null)
            {
                enemySurfaces = GameObject.Find("Enemy Navmesh Surface").GetComponent<NavMeshSurface>();
            }

            BuildNavMesh();
        }

        public void SettingTile(string buildingType)
        {
            cameraController.CreateTileSetting(true, (BuildingType)Enum.Parse(typeof(BuildingType), buildingType));
        }

        public void OffBuildingUI()
        {
            buildUIButton.SetActive(true);
            buildUI.SetActive(false);
        }

        public void OnBuildingUI()
        {
            buildUIButton.SetActive(false);
            buildUI.SetActive(true);
        }

        public void CreateTile(Vector3 pos, BuildingType building)
        {
            GameObject tile;
            float width;
            float height;

            switch (building)
            {
                case BuildingType.Ground:
                    tile = tilePrefab;
                    width = tileWidthSize;
                    height = tileHeightSize;

                    baseTile = groundTileMap;
                    break;
                case BuildingType.LumberMill:
                    tile = lumberPrefab;
                    width = lumberWidthSize;
                    height = lumberHeightSize;

                    baseTile = buildingTileMap;
                    break;
                case BuildingType.SwordArmy:
                    tile = swordArmyPrefab;
                    width = swordWidthSize;
                    height = swordHeightSize;

                    baseTile = buildingTileMap;
                    break;
                case BuildingType.SpearArmy:
                    tile = spearArmyPrefab;
                    width = spearWidthSize;
                    height = spearHeightSize;

                    baseTile = buildingTileMap;
                    break;
                case BuildingType.CrossbowArmy:
                    tile = crossbowArmyPrefab;
                    width = crossbowWidthSize;
                    height = crossbowHeightSize;

                    baseTile = buildingTileMap;
                    break;
                default:
                    return;
            }

            Vector3Int vector = baseTile.WorldToCell(pos);

            var localScale = tile.transform.localScale;
            localScale = new Vector3(localScale.x * width, localScale.y * height, localScale.z);
            
            tile.transform.localScale = localScale;
            
            var tileInstance = UnityEngine.ScriptableObject.CreateInstance<Tile>();
            tileInstance.gameObject = tile;
            
            baseTile.SetTile(vector, tileInstance);

            if (building is BuildingType.Ground)
            {
                BuildNavMesh();
            }
        }
        
        public void DeleteTile(Vector3 position)
        {
            Vector3Int vector = baseTile.WorldToCell(position);
            buildingTileMap.SetTile(vector, null);
        }

        private void BuildNavMesh()
        {
            NavMeshTag.PlayerCollect(ref playerSources);
            NavMeshTag.EnemyCollect(ref enemySources);

            Bounds bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(500, 500, 500));

            var playerBuildSettings = playerSurfaces.GetBuildSettings();
            UnityEngine.AI.NavMeshBuilder.UpdateNavMeshDataAsync(playerSurfaces.navMeshData, playerBuildSettings, playerSources, bounds);

            var enemyBuildSettings = enemySurfaces.GetBuildSettings();
            UnityEngine.AI.NavMeshBuilder.UpdateNavMeshDataAsync(enemySurfaces.navMeshData, enemyBuildSettings, enemySources, bounds);
        }
    }
}