using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FullMoon.Util
{
    public class ObjectPoolManager : MonoBehaviour
    {
        public static List<PooledObjectInfo> ObjectPools = new List<PooledObjectInfo>();

        public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            string goName = objectToSpawn.name.Substring(0, objectToSpawn.name.Length - 7);

            PooledObjectInfo pool = ObjectPools.Find(p => p.LookupString == goName);
            
            if (pool == null)
            {
                pool = new PooledObjectInfo() { LookupString = goName };
                ObjectPools.Add(pool);
            }

            GameObject spawnableObj = pool.InactiveObject.FirstOrDefault();

            if (spawnableObj == null)
            {
                spawnableObj = Instantiate(objectToSpawn, spawnPosition, spawnRotation);
            }
            else
            {
                pool.InactiveObject.Remove(spawnableObj);
                spawnableObj.transform.position = spawnPosition;
                spawnableObj.transform.rotation = spawnRotation;
                spawnableObj.SetActive(true);
            }
            return spawnableObj;
        }

        public static void ReturnObjectToPool(GameObject obj)
        {
            string goName = obj.name.Substring(0, obj.name.Length - 7);

            PooledObjectInfo pool = ObjectPools.Find(p => p.LookupString == goName);

            if (pool == null)
            {
                pool = new PooledObjectInfo() { LookupString = goName };
                ObjectPools.Add(pool);
                pool.InactiveObject.Add(obj);
                obj.SetActive(false);
            }
            else
            {
                pool.InactiveObject.Add(obj);
                obj.SetActive(false);
            }
        }
    }

    public class PooledObjectInfo
    {
        public string LookupString;
        public List<GameObject> InactiveObject = new List<GameObject>();
    }
}