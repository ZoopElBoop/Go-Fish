using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FishSpawn : MonoBehaviour
{
    [Header("Spawn & Despawn Range")]
    [Range(0f, 100f)]
    public float _spawnRange;
    [Range(0f, 100f)]
    public float _destroyRange;

    private bool activeTime = false;

    [Header("DEBUG")]
    [SerializeField] private bool gizmosActive;
    [SerializeField] private bool debugLog;

    //REMOVE LATER
    [Header("Spawned Fish Text (to be moved to seperate ui script)")]
    [SerializeField] private TMP_Text fishies;

    private int LayerIgnoreRaycast;
    private LayerMask LayersToIgnore = -1;

    private void Start()
    {
        SpawnTypeOf();

        LayerIgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");

        LayersToIgnore &= ~(1 << LayerIgnoreRaycast);   //sets layer to ignore "ignore raycast" layer

        LayerIgnoreRaycast = LayerMask.NameToLayer("Water");

        LayersToIgnore &= ~(1 << LayerIgnoreRaycast);   //sets layer to ignore "water" layer
    }

    private void SpawnTypeOf() 
    {
        float[,] fishProbability = new float[FishDataManager.Instance.GetFishDataSize(), 2];      //Contains index to fishdata array & empty probability factor
        float totalProbabilityValue = 0f;

        for (int i = 0; i < FishDataManager.Instance.GetFishDataSize(); i++)
        {
            float spawnStart = FishDataManager.Instance.GetSpawnStart(i);
            float spawnEnd = FishDataManager.Instance.GetSpawnEnd(i);
            float playerYPos = transform.position.y;

            if (playerYPos - _spawnRange > spawnStart || playerYPos + _spawnRange < spawnEnd)
                continue;

            float spawnHigh = FishDataManager.Instance.GetSpawnHigh(i);
            float distanceToHigh;

            if (playerYPos > spawnHigh)     //why try to do all the math stuff when unity has an inbuilt func for that, not reading docs meant i wasted like 5 hours on this, lmao
                distanceToHigh = Mathf.InverseLerp(spawnStart + _spawnRange, spawnHigh, playerYPos) * FishDataManager.Instance.GetRarity(i);
            else
                distanceToHigh = Mathf.InverseLerp(spawnEnd - _spawnRange, spawnHigh, playerYPos) * FishDataManager.Instance.GetRarity(i);

            fishProbability[i, 0] = i;
            fishProbability[i, 1] = distanceToHigh;

            totalProbabilityValue += distanceToHigh;
        }

        if (totalProbabilityValue <= 0)
        {
            Debug.LogWarning("No items found to spawn: ABORTING SPAWN"); 
            return;
        }

        float randVal = Random.Range(0.0f, totalProbabilityValue);          //added minus bit incase floating points get funky with prob value at limit
                                                                            //idk if this is actully an issue but i'd rather not risk it
        if (debugLog)                                                       //nvm don't touch it, many things break if it goes into negatives
        {
            print("RND VAL: " + randVal);

            print("/---/");
            for (int i = 0; i < fishProbability.GetLength(0); i++)
            {
                print(FishDataManager.Instance.GetFishName(i));
                print(fishProbability[i, 1]);
            }
            print("/---/");
        }

        int index = 0;

        while (randVal > 0.0f)
        {
            randVal -= fishProbability[index, 1];

            if (debugLog)
            {
                print("IND: " + index);
                print("RND VAL NEW: " + randVal);
            }

            index++;
        }

        index--;

        if (index < 0)
        {
            Debug.LogError("UH OH");
            Debug.Break();
            index++;
        }

        if (debugLog)
        {
            print("IND FOUND: " + index);
            print("SPAWNING " + FishDataManager.Instance.GetFishName(index));
            print("////////////////////////////////////////////////////////////////////////");
        }

        _Spawn((int)fishProbability[index, 0]);
    }

    public void _Spawn(int index)
    {
        GameObject fishToSpawn = FishDataManager.Instance.GetFish(index);
        Vector3 spawnPos;
        int spawnIteration = 0;     

        while (true)
        {
            spawnIteration += 1;

            if (spawnIteration > 16)    //Used to escape loop if unable to spawn fish after enough checks, or computer explodes :)
            {
                Debug.LogWarning("Unable to find suitable spawn pos after " + (spawnIteration - 1) + " iterations: ABORTING SPAWN");
                return;
            }

            spawnPos = Random.onUnitSphere * _spawnRange + transform.position;

            if (spawnPos.y > -2f)  //To be reworked for water surface & floor detection
                continue;

            if (!InWater(spawnPos))
                continue;

            Collider[] hitColliders = new Collider[1];  //literally the most useless array, but since OverlapSphere only takes an array here it stays

            Physics.OverlapSphereNonAlloc(spawnPos, FishDataManager.Instance.GetCollisionRange(index), hitColliders);

            if (hitColliders[0] == null)    //So for some reason if the collider detects nothing it dosen't return a empty array but an array with an NULL proterty, WHY???????????        
                break;
        }
        GameObject tempFishHolder = Instantiate(fishToSpawn, spawnPos, Quaternion.identity);    //temp fix for now, obliterate later
                                                                                                //fun story so theres a 1 in 100 chance the fish dosen't get the _playerPos & _destroyRange variables
        GameManager.Instance.AddFishToBuffer(tempFishHolder);                                   //since im stupid and set fishscript to fishToSpawn, but cuz i want to set the fish to a list a temp variable is needed atm
                                                                                                //when i decide to be competent ima redo this bit :)
        var fishScript = tempFishHolder.GetComponent<FishControl>();
        fishScript._playerPos = transform;
        fishScript._destroyRange = _destroyRange * FishDataManager.Instance.GetDespawnMultiplier(index);
        fishScript._dataIndex = index;
    }

    private bool InWater(Vector3 pos) 
    {
        Ray ray = new(pos, (-Vector3.up + pos) - pos);

        RaycastHit[] colliderFound = new RaycastHit[1];

        int hits = Physics.RaycastNonAlloc(ray, colliderFound, 350f, LayersToIgnore, QueryTriggerInteraction.Ignore);

        return hits > 0;
    }

    private void Update()
    {
        var SpawnedFish = GameManager.Instance.GetFishBufferSize();

        if (!activeTime && SpawnedFish < 10)     //Dodgy interval spawn, to be changed at some point
            SpawnTypeOf();

        fishies.text = SpawnedFish + " Fish";    //TESTING, remove later
    }

    IEnumerator CallSpawn()
    {
        activeTime = true;
        yield return new WaitForSeconds(Random.Range(0.25f, 1f));
        activeTime = false;
        SpawnTypeOf();
    }

    private void OnDrawGizmos() 
    {
        if (gizmosActive)
        {
            Gizmos.DrawWireSphere(transform.position, _spawnRange);

            Gizmos.color = Color.blue;

            /*        for (int i= 0; i < _spawnedFish.Count; i++)
                        Gizmos.DrawWireSphere(_spawnedFish[i].transform.position, fishCollisonRange);*/

            Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(transform.position, _destroyRange);
        }
    }
}