using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullMoon.Util
{
    [Serializable]
    struct ObjectDictionaryItem
    {
        public string name;
        public GameObject gameObject;
    }

    public class GameObjectDictionary : MonoBehaviour
    {
        [SerializeField] private List<ObjectDictionaryItem> items;

        public GameObject GetGameObjectByName(string objectName)
        {
            foreach (var item in items)
            {
                if (item.name == objectName)
                {
                    return item.gameObject;
                }
            }
            
            Debug.LogWarning($"Object with name {objectName} not found.");
            return null;
        }
    }
}