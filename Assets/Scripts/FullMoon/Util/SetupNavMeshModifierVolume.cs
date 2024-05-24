using Unity.AI.Navigation;
using UnityEngine.AI;
using UnityEngine;
using System.Collections.Generic;

namespace FullMoon.Util
{
    public class SetupNavMeshModifierVolume : MonoBehaviour
    {
        public NavMeshSurface[] surfaces;
        public float maxDistance = 5.0f;  // 링크 생성 최대 거리

        void Start()
        {
            GenerateOffMeshLinks();
        }

        void GenerateOffMeshLinks()
        {
            foreach (NavMeshSurface surface in surfaces)
            {
                NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByID(surface.agentTypeID);
                List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
                NavMeshBuilder.CollectSources(surface.transform, surface.layerMask, surface.useGeometry, surface.defaultArea, new List<NavMeshBuildMarkup>(), sources);

                // OffMesh Link 생성 로직
                for (int i = 0; i < sources.Count; i++)
                {
                    for (int j = i + 1; j < sources.Count; j++)
                    {
                        Vector3 startPos = sources[i].transform.MultiplyPoint3x4(Vector3.zero);
                        Vector3 endPos = sources[j].transform.MultiplyPoint3x4(Vector3.zero);
                        if (Vector3.Distance(startPos, endPos) <= maxDistance)
                        {
                            CreateOffMeshLink(startPos, endPos);
                        }
                    }
                }
            }
        }

        void CreateOffMeshLink(Vector3 start, Vector3 end)
        {
            GameObject linkObject = new GameObject("OffMeshLink");
            OffMeshLink link = linkObject.AddComponent<OffMeshLink>();
            link.startTransform.position = start;
            link.endTransform.position = end;
            link.biDirectional = true;  // 양방향 이동 설정
        }
    }
}
