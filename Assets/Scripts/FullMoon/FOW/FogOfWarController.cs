using UnityEngine;

namespace FullMoon.FOW
{
    public class FogOfWarController : MonoBehaviour
    {
        public Vector3 areaSize = new(10f, 10f, 10f);
        public float gridSpacing = 1f;
        public float gridBoxSize = 1f;
        public LayerMask layerMask;

        private void OnDrawGizmos()
        {
            // 기본 큐브 그리기
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, areaSize);

            // 윗면에서 그리드 포인트를 계산
            Vector3 startPoint = transform.position + new Vector3(-areaSize.x / 2, areaSize.y / 2, -areaSize.z / 2);
            int gridX = Mathf.CeilToInt(areaSize.x / gridSpacing);
            int gridZ = Mathf.CeilToInt(areaSize.z / gridSpacing);

            for (int x = 0; x <= gridX; x++)
            {
                for (int z = 0; z <= gridZ; z++)
                {
                    Vector3 rayOrigin = startPoint + new Vector3(x * gridSpacing, 0, z * gridSpacing);

                    if (Physics.Raycast(rayOrigin, Vector3.down, out var hit, areaSize.y, ~layerMask))
                    {
                        // 바닥으로부터의 높이에 따라 색상 계산
                        float heightAboveGround = hit.point.y - (transform.position.y - areaSize.y / 2);
                        float maxPossibleHeight = areaSize.y;
                        float colorFactor = heightAboveGround / maxPossibleHeight;

                        // 색상은 바닥에서부터의 높이가 증가함에 따라 녹색에서 빨간색으로 변화
                        Color lerpedColor = Color.Lerp(Color.green, Color.red, colorFactor);
                        Gizmos.color = lerpedColor;
                        Gizmos.DrawCube(hit.point, new Vector3(gridBoxSize, gridBoxSize, gridBoxSize));
                    }
                }
            }
        }
    }
}