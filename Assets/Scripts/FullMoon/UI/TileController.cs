using Cysharp.Threading.Tasks;
using FullMoon.Util;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FullMoon.UI
{
    public class TileController : ComponentSingleton<TileController>
    {
        [Header("TileMap")]
        [SerializeField] Tilemap tileMap;
        [SerializeField] GameObject tile;
        [SerializeField] NavMeshSurface nav;
        [SerializeField] NavMeshSurface nav2;

        Rect castleRect;
        private Vector2 castleMiddlePos;
        private Vector2 castleSize;
        private float rectLength = 4;

        void Start()
        {
            castleMiddlePos = new Vector2(-1f, -6f);
            castleSize = new Vector2(8f, 8f);
            castleRect = new Rect(castleMiddlePos, castleSize * 2);
        }

        public void CreateTile(Vector3 pos)
        {
            Vector3Int vector = new Vector3Int((int)pos.x, (int)pos.z, 5);
            tileMap.SetTile(vector, new Tile()
            {
                gameObject = tile
            });
            Debug.Log("CreateTile");

            // NavMesh 빌드 실행
        }

        // 월드 좌표 -> 타일 좌표
        public (Vector2, Vector2) WorldToTileVector(Vector3 mousePos, float size)
        {
            Vector2 vector = new Vector2(mousePos.x, mousePos.z);
            Vector2 startValue = new Vector2(0, 0);
            Vector2 endValue = new Vector2(0, 0);

            if (castleRect.Contains(vector))
            {
            }
            else
            {
                if (vector.x < castleRect.center.x)
                {
                    startValue.y = Mathf.Abs((int)(castleRect.center.x - vector.x) / rectLength);
                }
                else if (vector.x > castleRect.center.x)
                {
                    startValue.y = (int)((castleRect.center.x - vector.x) / rectLength);
                }
                if (vector.y < castleRect.center.y)
                {
                    startValue.x = Mathf.Abs((int)(castleRect.center.y - vector.y) / rectLength);
                }
                else if (vector.y >= castleRect.center.y)
                {
                    startValue.x = (int)((castleRect.center.y - vector.y) / rectLength);
                }

                endValue.x = startValue.x * size;
                endValue.y = startValue.y * size;
            }
            
            return (startValue, endValue);
        }

        // 타일 좌표 -> 월드 좌표(타일 중앙 값)
        public Vector2 TileToWorldVector(Vector3 pos, float size)
        {
            (Vector2 startVec, Vector2 endVec) = WorldToTileVector(pos, size);

            return new Vector2(castleRect.center.x + (startVec.x * rectLength), castleRect.center.y + (startVec.y * rectLength));
        }
    }
}