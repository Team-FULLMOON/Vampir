using UnityEngine;

namespace FullMoon.Util
{
    public abstract class ComponentSingleton<T> : MonoBehaviour where T : ComponentSingleton<T>
    {
        private static T _instance;

        public static bool Exists => _instance != null;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindOrCreateInstance();
                }
                return _instance;
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
            GameObject singletonObject = new GameObject
            {
                name = typeof(T).Name + " (Singleton)"
            };
            
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(singletonObject);
            }
            
            return singletonObject.AddComponent<T>();
        }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                _instance = this as T;
            }
        }

        public static void DestroySingleton()
        {
            if (Exists)
            {
                DestroyImmediate(_instance.gameObject);
                _instance = null;
            }
        }
    }
}