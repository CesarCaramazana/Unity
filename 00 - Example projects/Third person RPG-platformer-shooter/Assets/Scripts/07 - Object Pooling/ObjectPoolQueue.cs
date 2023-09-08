using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolQueue : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string poolTag;
        public GameObject prefab;
        public int poolSize;
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDicionary;


    public static ObjectPoolQueue Instance;

    private void Awake()
    {
        Instance = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        poolDicionary = new Dictionary<string, Queue<GameObject>>();


        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.poolSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDicionary.Add(pool.poolTag, objectPool);
        }
        
    }



    public GameObject SpawnFromPool (string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDicionary.ContainsKey(tag))
        {
            Debug.LogWarning("No object pool with tag: " + tag);
            
            return null;
        }

        GameObject objectToSpawn = poolDicionary[tag].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        poolDicionary[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }


}
