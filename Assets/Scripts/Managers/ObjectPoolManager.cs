using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;

    private List<GameObject> FishPool = new();
    private List<GameObject> HarpoonPool = new();

    [SerializeField][Min(1)] private int duplicateLimit;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
        else
            Destroy(this);
    }

    private List<GameObject> GetPool(string Type)
    {
        switch (Type)
        {
            case "Fish":
                return FishPool;

            case "Harpoon":
                return HarpoonPool;

            default:
                Debug.LogError($"Pool {Type} not found to give!!!");
                break;
        }

        Debug.Break();
        return null;
    }

    #region Duplicate Checks

    private GameObject CheckPoolForAnyDuplicates(List<GameObject> pool, GameObject findInPool)
    {
        /*
        This is honestly one of the worst ideas i have ever had

        So basically to compare if two fish are the same i can't just do findInPool == ObjectPool[i] (thanks unity) so i have decided to do something horrifically jank,
        I'm comparing the names of the objects but since the findInPool object isn't in the scene its missing the "(Clone)" suffix whereas the in-scene object has it,
        So this line just adds that for comparision

        Ik this is a horrible way of doing this, but hey it works (for now) :))))))))
         */
        string conpareTo = findInPool.name + "(Clone)";

        for (int i = 0; i < pool.Count; i++)       
            if (conpareTo == pool[i].name)
                return pool[i];
        
        return null;
    }

    private int CheckPoolForNumberOfDuplicates(List<GameObject> pool, GameObject findInPool)
    {
        int duplicates = 0;

        for (int i = 0; i < pool.Count; i++)        
            if (findInPool.name == pool[i].name)
                duplicates++;
        
        return duplicates;
    }

    #endregion

    #region Spawn

    public GameObject SpawnObject(string Type, GameObject toSpawn, Vector3 position, Quaternion rotation)
    {
        List<GameObject> pool = GetPool(Type);

        GameObject pooledObject = CheckPoolForAnyDuplicates(pool, toSpawn);

        if (pooledObject != null)
        {
            pooledObject.transform.SetPositionAndRotation(position, rotation);
            pooledObject.SetActive(true);

            pool.Remove(pooledObject);

            return pooledObject;
        }
        else
        {
            toSpawn = Instantiate(toSpawn, position, rotation);
            return toSpawn;
        }
    }

    #endregion

    #region Despawn

    public void DespawnObject(string Type, GameObject toDespawn)
    {
        List<GameObject> pool = GetPool(Type);

        if (toDespawn.activeSelf)
            toDespawn.SetActive(false);

        int numOfDuplicates = CheckPoolForNumberOfDuplicates(pool, toDespawn);

        if (numOfDuplicates >= duplicateLimit)
            Destroy(toDespawn);
        else
            pool.Add(toDespawn);
    }

    #endregion
}
