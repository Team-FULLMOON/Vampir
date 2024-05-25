using System.Collections;
using System.Collections.Generic;
using FullMoon.Util;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

namespace FullMoon.UI
{
    public class TileController : ComponentSingleton<TileController>
    {
        [Header("TileMap")]
        [SerializeField] GameObject tile;
        Tilemap tileMap;
        private NavMeshSurface nav;

        Rect castleRect;
        private Vector2 castleMiddlePos;
        private Vector2 castleSize;
        private float rectLength = 4;

        void Start()
        {
            castleMiddlePos = new Vector2(-1f, -6f);
            castleSize = new Vector2(8f, 8f);
            castleRect = new Rect(castleMiddlePos, castleSize * 2);

            tileMap = FindObjectOfType<Tilemap>();
        }

        public void CreateTile(Vector3 pos)
        {
            Vector3Int vector = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z), (int)pos.y);
            tileMap.SetTile(vector, new Tile()
            {
                gameObject = tile
            });
        }
    }
}