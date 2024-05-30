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
        GameObject lumberPrefab;

        [SerializeField, OverrideLabel("벌목소 가로 사이즈")]
        int lumberWidthSize = 1;

        [SerializeField, OverrideLabel("벌목소 세로 사이즈")]
        int lumberHeightSize = 1;

        [Separator]

        [SerializeField, OverrideLabel("훈련소(검) 프리팹")]
        GameObject swordArmyPrefab;

        [SerializeField, OverrideLabel("훈련소(검) 가로 사이즈")]
        int swordWidthSize = 1;

        [SerializeField, OverrideLabel("훈련소(검) 세로 사이즈")]
        int swordHeightSize = 1;

        [Separator]

        [SerializeField, OverrideLabel("훈련소(창) 프리팹")]
        GameObject spearArmyPrefab;

        [SerializeField, OverrideLabel("훈련소(창) 가로 사이즈")]
        int spearWidthSize = 1;

        [SerializeField, OverrideLabel("훈련소(창) 세로 사이즈")]
        int spearHeightSize = 1;

        [Separator]

        [SerializeField, OverrideLabel("훈련소(석궁) 프리팹")]
        GameObject crossbowArmyPrefab;

        [SerializeField, OverrideLabel("훈련소(석궁) 가로 사이즈")]
        int crossbowWidthSize = 1;

        [SerializeField, OverrideLabel("훈련소(석궁) 세로 사이즈")]
        int crossbowHeightSize = 1;


        private Tilemap tileMap;

        void Start()
        {
            tileMap = FindObjectOfType<Tilemap>();
        }

        public void CreateTile(Vector3 pos, BuildingType buildingType)
        {
            GameObject tile;
            float width = 1;
            float height = 1;

            switch (buildingType)
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
            tile.transform.localScale = new Vector3(tile.transform.localScale.x * width, tile.transform.localScale.y * height, tile.transform.localScale.z);
            tile.GetComponent<BaseBuildingController>().targetPos = pos;
            tileMap.SetTile(vector, new Tile()
            {
                gameObject = tile,
            });
        }
    }
}