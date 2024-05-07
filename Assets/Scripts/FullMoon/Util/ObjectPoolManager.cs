using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;

namespace FullMoon.Util
{
    public class ObjectPoolManager : MonoBehaviour
    {
        public static List<PooledObjectInfo> ObjectPools = new List<PooledObjectInfo>();

        public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            string name = NameReplace(objectToSpawn);
            string goName = name.Substring(0, name.Length - 7);

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
            string name = NameReplace(obj);
            string goName = name.Substring(0, name.Length - 7);

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

        private static string NameReplace(GameObject obj)
        {
            string originalString = obj.name;
            string modifiedString = Regex.Replace(originalString, @" (\d+)", "");
            return modifiedString;
        }
    }

    public class PooledObjectInfo
    {
        public string LookupString;
        public List<GameObject> InactiveObject = new List<GameObject>();
    }
}