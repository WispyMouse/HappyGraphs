using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Singleton { get; private set; }

    Dictionary<System.Type, List<MonoBehaviour>> ObjectPool = new Dictionary<System.Type, List<MonoBehaviour>>();

    private void Awake()
    {
        Singleton = this;
    }

    public static T GetObject<T>(T prefab, Transform parent = null) where T : MonoBehaviour
    {
        if (Singleton.ObjectPool.ContainsKey(prefab.GetType()))
        {
            MonoBehaviour firstInPool = Singleton.ObjectPool[prefab.GetType()].FirstOrDefault();

            if (firstInPool != null)
            {
                Singleton.ObjectPool[prefab.GetType()].Remove(firstInPool);
                firstInPool.transform.SetParent(parent);
                firstInPool.gameObject.SetActive(true);
                return (T)firstInPool;
            }
        }
        else
        {
            Singleton.ObjectPool.Add(prefab.GetType(), new List<MonoBehaviour>());
        }

        MonoBehaviour newInstance = Instantiate(prefab, parent);
        return (T)newInstance;
    }

    public static void ReturnObject<T>(T toReturn) where T : MonoBehaviour
    {
        if (!Singleton.ObjectPool.ContainsKey(toReturn.GetType()))
        {
            Destroy(toReturn.gameObject);
            return;
        }

        toReturn.gameObject.SetActive(false);
        Singleton.ObjectPool[toReturn.GetType()].Add(toReturn);
    }
}
