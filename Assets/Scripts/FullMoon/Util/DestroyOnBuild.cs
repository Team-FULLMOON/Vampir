using UnityEngine;

namespace FullMoon.Util
{
    public class DestroyOnBuild : MonoBehaviour
    {
        [SerializeField] private bool destroyInDevelopmentBuild;

        private void Awake()
        {
#if DEVELOPMENT_BUILD
            if (destroyInDevelopmentBuild)
            {
                Destroy(gameObject);
            }
#elif !UNITY_EDITOR
            Destroy(gameObject);
#endif
        }
    }
}