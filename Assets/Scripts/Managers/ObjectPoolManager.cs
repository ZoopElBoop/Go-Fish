using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;

    private List<GameObject> ObjectPool = new();

    [SerializeField] [Min(1)] private int duplicateLimit;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
        else
            Destroy(this);

        //Events Init
        EventManager.Instance.OnFishDisable += DespawnObject;
    }

    private GameObject CheckPoolForAnyDuplicates(GameObject findInPool)
    {
        /*
        This is honestly one of the worst ideas i have ever had

        So basically to compare if two fish are the same i can't just do findInPool == ObjectPool[i] (thanks unity) so i have decided to do something horrifically jank,
        I'm comparing the names of the objects but since the findInPool object isn't in the scene its missing the "(Clone)" suffix whereas the in-scene object has it,
        So this line just adds that for comparision

        Ik this is a horrible way of doing this, but hey it works (for now) :))))))))
         */
        string conpareTo = findInPool.name + "(Clone)";

        for (int i = 0; i < ObjectPool.Count; i++)       
            if (conpareTo == ObjectPool[i].name)
                return ObjectPool[i];
        
        return null;
    }

    private int CheckPoolForNumberOfDuplicates(GameObject findInPool)
    {
        int duplicates = 0;

        for (int i = 0; i < ObjectPool.Count; i++)        
            if (findInPool.name == ObjectPool[i].name)
                duplicates++;
        
        return duplicates;
    }

    public GameObject SpawnObject(GameObject toSpawn, Vector3 position, Quaternion rotation)
    {
        GameObject pooledObject = CheckPoolForAnyDuplicates(toSpawn);

        if (pooledObject != null)
        {
            pooledObject.transform.SetPositionAndRotation(position, rotation);
            pooledObject.SetActive(true);

            ObjectPool.Remove(pooledObject);

            print("pooled");
            return pooledObject;
        }
        else
        {
            toSpawn = Instantiate(toSpawn, position, rotation);
            print("spawned");
            return toSpawn;
        }    
    }

    public void DespawnObject(GameObject toDespawn)
    {
        print("DESPAWNING");

        if (toDespawn.activeSelf)
            toDespawn.SetActive(false);

        int numOfDuplicates = CheckPoolForNumberOfDuplicates(toDespawn);

        if (numOfDuplicates >= duplicateLimit)        
            Destroy(toDespawn);      
        else       
            ObjectPool.Add(toDespawn);       
    }

    private void OnDestroy()
    {
        EventManager.Instance.OnFishDisable -= DespawnObject;
    }
}
