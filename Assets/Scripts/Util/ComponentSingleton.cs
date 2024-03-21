using UnityEngine;

namespace FullMoon.Util
{
    [ExecuteInEditMode]
    public abstract class ComponentSingleton<T> : MonoBehaviour where T : ComponentSingleton<T>
    {
        private static T s_Instance;

        public static bool Exists => s_Instance != null;

        public static T Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = FindOrCreateInstance();
                }
                return s_Instance;
            }
        }

        private static T FindOrCreateInstance()
        {
            T existingInstance = FindObjectOfType<T>();
            if (existingInstance != null)
            {
                return existingInstance;
            }

            return CreateNewSingleton();
        }

        protected virtual string GetGameObjectName() => typeof(T).Name;

        private static T CreateNewSingleton()
        {
            GameObject singletonObject = new GameObject();
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(singletonObject);
            }
            singletonObject.name = typeof(T).Name + " (Singleton)";
            return singletonObject.AddComponent<T>();
        }

        protected virtual void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                s_Instance = this as T;
            }
        }

        public static void DestroySingleton()
        {
            if (Exists)
            {
                DestroyImmediate(s_Instance.gameObject);
                s_Instance = null;
            }
        }
    }
}