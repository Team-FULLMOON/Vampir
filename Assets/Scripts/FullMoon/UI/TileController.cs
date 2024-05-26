using FullMoon.Util;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FullMoon.UI
{
    public class TileController : ComponentSingleton<TileController>
    {
        [Header("TileMap")]
        [SerializeField] GameObject tile;
        [SerializeField] private Tilemap tileMap;

        void Start()
        {
            tileMap = FindObjectOfType<Tilemap>();
        }

        public void CreateTile(Vector3 pos)
        {
            Vector3Int vector = tileMap.WorldToCell(pos);
            tileMap.SetTile(vector, new Tile()
            {
                gameObject = tile
            });
        }
    }
}