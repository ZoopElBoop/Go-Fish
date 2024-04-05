using System.Collections;
using UnityEngine;

public class FishSpawn : MonoBehaviour
{
    #region Variables

    [Header("Spawn & Despawn Range")]
    [Range(0f, 100f)]
    [SerializeField] private float _spawnRange;
    [Range(0f, 150f)]
    [SerializeField] private float _destroyRange;

    [Header("Spawn Cooldown (In Seconds)")]
    [Min(0.1f)][SerializeField] private float spawnCoolDown;
    private bool canTrySpawn = true;

    [Header("Spawn Limit")]
    [Min(1)][SerializeField] private int maxFishActive;

    [Header("Number Of Spawn Checks Before Giving Up")]
    [Range(1, 64)]
    [SerializeField] private int maxSpawnCheckIterations;

    [Header("DEBUG")]
    [SerializeField] private bool gizmosActive;
    [SerializeField] private bool debugLog;

    //Layer mask for ray checks
    private int LayerIgnoreRaycast;
    private LayerMask LayersToIgnore = -1;

    #endregion

    #region Start Function

    private void Start()
    {
        LayerIgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");

        LayersToIgnore &= ~(1 << LayerIgnoreRaycast);   //sets layer to ignore "ignore raycast" layer

        LayerIgnoreRaycast = LayerMask.NameToLayer("Water");

        LayersToIgnore &= ~(1 << LayerIgnoreRaycast);   //sets layer to ignore "water" layer
    }

    #endregion

    #region Probability Find

    private void SpawnTypeOf() 
    {
        FishProbability[] fishProbability = new FishProbability[FishDataManager.Instance.GetFishDataSize()];      //Contains index to fishdata array & empty probability factor
        float totalProbabilityValue = 0f;

        for (int i = 0; i < fishProbability.Length; i++)
        {
            float spawnStart = FishDataManager.Instance.GetSpawnStart(i);
            float spawnHigh = FishDataManager.Instance.GetSpawnHigh(i);
            float spawnEnd = FishDataManager.Instance.GetSpawnEnd(i);

            float playerYPos = transform.position.y;
            float distanceToHigh = 0f;

            if (playerYPos - _spawnRange < spawnStart || playerYPos + _spawnRange > spawnEnd)   //checks if spawn range within spawn min & max depth of fish, if so calculates distance ratio from high point
            { 
                if (playerYPos > spawnHigh)     //why try to do all the math stuff when unity has an inbuilt func for that, not reading docs meant i wasted like 5 hours on this, lmao
                    distanceToHigh = Mathf.InverseLerp(spawnStart + _spawnRange, spawnHigh, playerYPos) * FishDataManager.Instance.GetRarity(i);
                else
                    distanceToHigh = Mathf.InverseLerp(spawnEnd - _spawnRange, spawnHigh, playerYPos) * FishDataManager.Instance.GetRarity(i);
            }

            fishProbability[i] = new FishProbability{
                Index = i, 
                Probability = distanceToHigh
            };

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
            for (int i = 0; i < fishProbability.Length; i++)
            {
                print(FishDataManager.Instance.GetFishName(i));
                print(fishProbability[i].Probability);
            }
            print("/---/");
        }

        int index = 0;

        while (randVal > 0.0f)
        {
            randVal -= fishProbability[index].Probability;

            if (debugLog)
            {
                print($"INDEX: {index}");
                print($"RND VAL NEW: {randVal}");
            }

            index++;
        }

        index--;

        if (index < 0)
        {
            Debug.LogError($"INDEX ({index}), FAILED SPAWN PROBABILITY CALC");
            Debug.Break();
            index++;
        }

        if (debugLog)
        {
            print($"INDEX FOUND: {index}");
            print($"SPAWNING {FishDataManager.Instance.GetFishName(index)}");
            print($"////////////////////////////////////////////////////////////////////////");
        }

        Spawn(fishProbability[index].Index);
    }

    #endregion

    #region Spawn

    public void Spawn(int index)
    {
        //Fish Spawn Checks
        
        // 1. Sets y position between limits of fish spawn (includes limits to spawn range so fish aren't spawned at their limit if spawn range can't reach)
        // 2. Is the position in water (fires ray to see if it hits sea floor)
        // 3. Is the position empty

        //If all these checks pass the fish is spawned (limited to 16 iterations to avoid infinite loop)

        GameObject fishToSpawn = FishDataManager.Instance.GetFish(index);
        Vector3 spawnPos;

        int spawnIteration = 0;     

        while (true)
        {
            spawnIteration += 1;

            if (spawnIteration == maxSpawnCheckIterations + 1)    //Used to escape loop if unable to spawn fish after enough checks, or computer explodes :)
            {
                Debug.LogWarning($"Unable to find suitable spawn position after {maxSpawnCheckIterations} iterations: ABORTING SPAWN");
                return;
            }

            spawnPos = Random.onUnitSphere * _spawnRange + transform.position;

            spawnPos.y = GetNewYPos(index);      

            if (!InWater(spawnPos))     //Checks if position is in water            
                continue;           

            Collider[] hitColliders = new Collider[1];  //literally the most useless array, but since OverlapSphere only takes an array here it stays

            Physics.OverlapSphereNonAlloc(spawnPos, FishDataManager.Instance.GetCollisionRange(index), hitColliders, LayersToIgnore);

            if (hitColliders[0] == null)    //So for some reason if the collider detects nothing it dosen't return a empty array but an array with an NULL proterty, WHY???????????        
                break;
        }

        FishControl fishScript = GameManager.Instance.SpawnFishAndGetScript(fishToSpawn, spawnPos, Quaternion.identity);

        fishScript._playerPos = transform;
        fishScript._destroyRange = _destroyRange * FishDataManager.Instance.GetDespawnMultiplier(index);
        fishScript._dataIndex = index;
    }

    private float GetNewYPos(int index)
    {
        float yPosEnd;
        float spawnEnd = FishDataManager.Instance.GetSpawnEnd(index);
        float spawnStart = FishDataManager.Instance.GetSpawnStart(index);

        if (spawnEnd < (-_spawnRange + transform.position.y))
            yPosEnd = -_spawnRange + transform.position.y;
        else
            yPosEnd = spawnEnd;

        float yPos = Random.Range(yPosEnd, spawnStart);

        return yPos;
    }

    private bool InWater(Vector3 pos) 
    {
        Ray ray = new(pos, -Vector3.up);

        RaycastHit[] colliderFound = new RaycastHit[1];

        int hits = Physics.RaycastNonAlloc(ray, colliderFound, 500f, LayersToIgnore, QueryTriggerInteraction.Ignore);

        return hits > 0;
    }

    #endregion

    #region Spawn Call

    private void Update()
    {
        var SpawnedFish = GameManager.Instance.GetFishBufferSize();

        if (canTrySpawn && SpawnedFish < maxFishActive)     
            StartCoroutine(CallSpawn());
    }

    IEnumerator CallSpawn()
    {
        canTrySpawn = false;
        yield return new WaitForSeconds(spawnCoolDown);
        canTrySpawn = true;
        SpawnTypeOf();
    }

    #endregion

    #region Gizmos

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

    #endregion

}

public class FishProbability
{
    public int Index = 0;
    public float Probability = 0f;
}