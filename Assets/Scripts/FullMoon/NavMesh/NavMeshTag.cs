using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.AI;

namespace FullMoon.NavMesh
{
    public class NavMeshTag : MonoBehaviour
    {
        [SerializeField, OverrideLabel("Type"), DefinedValues("Both", "Player", "Enemy")]
        private string type = "Both";


        public static List<MeshFilter> playerMeshes = new List<MeshFilter>();
        public static List<MeshFilter> enemyMeshes = new List<MeshFilter>();

        void OnEnable()
        {
            var m = GetComponent<MeshFilter>();
            if (m != null)
            {
                if (type is "")
                {
                    type = "Both";
                }

                if (type is "Both")
                {
                    playerMeshes.Add(m);
                    enemyMeshes.Add(m);
                }
                else if (type is "Player")
                {
                    playerMeshes.Add(m);
                }
                else if (type is "Enemy")
                {
                    enemyMeshes.Add(m);
                }
            }
        }

        void OnDisable()
        {
            var m = GetComponent<MeshFilter>();
            if (m != null)
            {
                if (type is "Both")
                {
                    playerMeshes.Remove(m);
                    enemyMeshes.Remove(m);
                }
                else if (type is "Player")
                {
                    playerMeshes.Remove(m);
                }
                else if (type is "Enemy")
                {
                    enemyMeshes.Remove(m);
                }
            }
        }

        public static void PlayerCollect(ref List<NavMeshBuildSource> sources)
        {
            sources.Clear();

            for (var i = 0; i < playerMeshes.Count; ++i)
            {
                var mf = playerMeshes[i];
                if (mf == null) continue;

                var m = mf.sharedMesh;
                if (m == null) continue;

                var s = new NavMeshBuildSource();
                s.shape = NavMeshBuildSourceShape.Mesh;
                s.sourceObject = m;
                s.transform = mf.transform.localToWorldMatrix;
                s.area = 0;
                sources.Add(s);
            }
        }

        public static void EnemyCollect(ref List<NavMeshBuildSource> sources)
        {
            sources.Clear();

            for (var i = 0; i < enemyMeshes.Count; ++i)
            {
                var mf = enemyMeshes[i];
                if (mf == null) continue;

                var m = mf.sharedMesh;
                if (m == null) continue;

                var s = new NavMeshBuildSource();
                s.shape = NavMeshBuildSourceShape.Mesh;
                s.sourceObject = m;
                s.transform = mf.transform.localToWorldMatrix;
                s.area = 0;
                sources.Add(s);
            }
        }
    }
}
