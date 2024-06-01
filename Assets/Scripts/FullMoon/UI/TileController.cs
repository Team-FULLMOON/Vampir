using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using FullMoon.Camera;
using FullMoon.Entities.Building;
using FullMoon.Util;
using MyBox;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FullMoon.UI
{
    public enum BuildingType
    {
        None,
        LumberMill,     // 벌목소
        SwordArmy,      // 훈련소(검)
        SpearArmy,      // 훈련소(창)
        CrossbowArmy,   // 훈련소(석궁)
    }

    public class TileController : ComponentSingleton<TileController>
    {
        [Separator("TileMap")]

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

        private Tilemap tileMap;
        private CameraController cameraController;

        void Start()
        {
            tileMap = FindObjectOfType<Tilemap>();
            cameraController = FindObjectOfType<CameraController>();
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
                case BuildingType.LumberMill:
                    tile = lumberPrefab;
                    width = lumberWidthSize;
                    height = lumberHeightSize;
                    break;
                case BuildingType.SwordArmy:
                    tile = swordArmyPrefab;
                    width = swordWidthSize;
                    height = swordHeightSize;
                    break;
                case BuildingType.SpearArmy:
                    tile = spearArmyPrefab;
                    width = spearWidthSize;
                    height = spearHeightSize;
                    break;
                case BuildingType.CrossbowArmy:
                    tile = crossbowArmyPrefab;
                    width = crossbowWidthSize;
                    height = crossbowHeightSize;
                    break;
                default:
                    return;
            }

            Vector3Int vector = tileMap.WorldToCell(pos);

            var localScale = tile.transform.localScale;
            localScale = new Vector3(localScale.x * width, localScale.y * height, localScale.z);
            
            tile.transform.localScale = localScale;
            
            var tileInstance = UnityEngine.ScriptableObject.CreateInstance<Tile>();
            tileInstance.gameObject = tile;
            
            tileMap.SetTile(vector, tileInstance);
        }
    }
}