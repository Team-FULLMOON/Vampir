using UnityEditor;
using UnityEngine;

namespace FullMoon.FOW.Editor
{
    [CustomEditor(typeof(FogOfWarController))]
    public class FogOfWarEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            FogOfWarController fow = target as FogOfWarController;
            Transform handleTransform = fow.transform;
            
            Vector3 localPosition = handleTransform.localPosition;

            Vector3[] points = new Vector3[6];
            points[0] = localPosition + Vector3.right * fow.areaSize.x / 2;    // Right
            points[1] = localPosition - Vector3.right * fow.areaSize.x / 2;    // Left
            points[2] = localPosition + Vector3.forward * fow.areaSize.z / 2;  // Forward
            points[3] = localPosition - Vector3.forward * fow.areaSize.z / 2;  // Back
            points[4] = localPosition + Vector3.up * fow.areaSize.y / 2;       // Up
            points[5] = localPosition - Vector3.up * fow.areaSize.y / 2;       // Down

            Handles.color = Color.yellow;
            Vector3 snap = new Vector3(0.5f, 0.5f, 0.5f);

            for (int i = 0; i < points.Length; i++)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newPoint = Handles.FreeMoveHandle(points[i],  0.2f, snap, Handles.CircleHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(fow, "Resize Fog of War Area");
                    Undo.RecordObject(handleTransform, "Resize Fog of War Area Transform");
                    
                    float delta;
                    switch (i)
                    {
                        case 0: // Right
                            delta = newPoint.x - points[i].x;
                            fow.areaSize.x += delta;
                            localPosition.x += delta / 2;
                            break;
                        case 1: // Left
                            delta = newPoint.x - points[i].x;
                            fow.areaSize.x -= delta;
                            localPosition.x += delta / 2;
                            break;
                        case 2: // Forward
                            delta = newPoint.z - points[i].z;
                            fow.areaSize.z += delta;
                            localPosition.z += delta / 2;
                            break;
                        case 3: // Back
                            delta = newPoint.z - points[i].z;
                            fow.areaSize.z -= delta;
                            localPosition.z += delta / 2;
                            break;
                        case 4: // Up
                            delta = newPoint.y - points[i].y;
                            fow.areaSize.y += delta;
                            localPosition.y += delta / 2;
                            break;
                        case 5: // Down
                            delta = newPoint.y - points[i].y;
                            fow.areaSize.y -= delta;
                            localPosition.y += delta / 2;
                            break;
                    }
                    
                    fow.areaSize = new Vector3(Mathf.Max(1f, fow.areaSize.x), Mathf.Max(1f, fow.areaSize.y), Mathf.Max(1f, fow.areaSize.z));
                    handleTransform.localPosition = localPosition;
                }
            }
        }
    }
}
