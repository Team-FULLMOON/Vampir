using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FullMoon.Util
{
    public class ObjectPoolManager : ComponentSingleton<ObjectPoolManager>
    {
        private readonly Dictionary<string, PooledObjectInfo> objectPools = new();
        
        public GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            string goName = NameReplace(objectToSpawn);

            if (!objectPools.TryGetValue(goName, out PooledObjectInfo pool))
            {
                pool = new PooledObjectInfo();
                objectPools.Add(goName, pool);
            }

            GameObject spawnableObj = pool.GetInactiveObject();

            if (spawnableObj == null)
            {
                spawnableObj = Instantiate(objectToSpawn, spawnPosition, spawnRotation);
            }
            else
            {
                spawnableObj.transform.position = spawnPosition;
                spawnableObj.transform.rotation = spawnRotation;
                spawnableObj.SetActive(true);
            }
            return spawnableObj;
        }

        public void ReturnObjectToPool(GameObject obj)
        {
            string goName = NameReplace(obj);

            if (!objectPools.TryGetValue(goName, out PooledObjectInfo pool))
            {
                pool = new PooledObjectInfo();
                objectPools[goName] = pool;
            }

            pool.ReturnObject(obj);
        }

        private string NameReplace(GameObject obj)
        {
            string originalString = obj.name;
            return Regex.Replace(originalString, @" (\d+)|\(Clone\)", "");
        }
    }

    public class PooledObjectInfo
    {
        private readonly HashSet<GameObject> inactiveObject = new();

        public GameObject GetInactiveObject()
        {
            if (inactiveObject.Count > 0)
            {
                GameObject obj = inactiveObject.First();
                inactiveObject.Remove(obj);
                return obj;
            }
            return null;
        }

        public void ReturnObject(GameObject obj)
        {
            inactiveObject.Add(obj);
            obj.SetActive(false);
        }
    }
}
