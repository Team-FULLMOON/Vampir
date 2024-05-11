using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class NewBehaviourScript : MonoBehaviour
{
    
    // Update is called once per frame
    void Update()
    {
        if (gameObject.transform.hasChanged)
        {
            Renderer[] renderers = GameObject.FindObjectsOfType<Renderer>();
            foreach (var r in renderers)
            {
                Material m;
                
                #if UNITY_EDITOR
                m = r.material;
                
                #else
                m = r.material;

#endif
                
                m = r.material;
                if (string.Compare(strA: m.shader.name, strB: "Shader Graphs/ToonRamp") == 0)

                {
                    m.SetVector(name:"_Light_dir", transform.forward);
                }
            }
            
        }
        
    }
}
