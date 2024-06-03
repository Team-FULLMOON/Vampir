using UnityEngine;

namespace FullMoon.Util
{
    public class UnityFunctions : MonoBehaviour
    {
        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
