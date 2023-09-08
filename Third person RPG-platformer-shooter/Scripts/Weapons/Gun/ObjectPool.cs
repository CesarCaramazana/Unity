using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Rendering.CameraUI;


public enum ProjectileType {Bullet, Shell, Arrow, Casquet};


public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public ProjectileType bulletType;
        public GameObject prefab;
        public int poolSize;
        public bool canGrow;
    }

    public static ObjectPool Instance;

    public List<Pool> pools;
    public Dictionary<ProjectileType, List<GameObject>> poolDictionary;
    public Dictionary<ProjectileType, Pool> poolParams;

    private bool canGrow;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {

        poolDictionary = new Dictionary<ProjectileType, List<GameObject>>();
        poolParams = new Dictionary<ProjectileType, Pool>();

        foreach (Pool pool in pools)
        {
            List<GameObject> objectPool = new List<GameObject>();

            for (int i = 0; i < pool.poolSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Add(obj);
            }

            poolDictionary.Add(pool.bulletType, objectPool);
            poolParams.Add(pool.bulletType, pool);

        }
    }

    public void ClearPool(ProjectileType projectileType)
    {
        if (!poolDictionary.ContainsKey(projectileType))
        {
            Debug.LogError("No object pooled with projectile type: " + projectileType);
            return;
        }

        List<GameObject> objectPool = poolDictionary[projectileType];
        Pool parameters = poolParams[projectileType];
        
        //Debug.Log("Old count: " + objectPool.Count); 
        
        for (int i = 0; i < objectPool.Count; i++)
        {
            GameObject obj = objectPool[i];
            objectPool.RemoveAt(i);
            Destroy(obj);
        }

       // Debug.Log("New count: " + objectPool.Count);   
    }


    public GameObject SpawnPooledObject(ProjectileType projectileType, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(projectileType))
        {
            Debug.LogError("No object pooled with projectile type: " + projectileType);
            return null;
        }


        List<GameObject> objectPool = poolDictionary[projectileType];
        Pool parameters = poolParams[projectileType];

        for (int i = 0; i < objectPool.Count; i++)
        {
            if (!objectPool[i].activeInHierarchy)
            {
                objectPool[i].SetActive(true);
                objectPool[i].transform.localPosition = position;
                objectPool[i].transform.localRotation = rotation;
                return objectPool[i];
            }            
        }

        if (parameters.canGrow)
        {
            GameObject obj = Instantiate(parameters.prefab);
            obj.SetActive(true);
            obj.transform.localPosition = position;
            obj.transform.localRotation = rotation;
            poolDictionary[projectileType].Add(obj);
            return obj;
        }
        

        return null;

    }


}
