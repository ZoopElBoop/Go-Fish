using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Fishing : MonoBehaviour
{
    [Header("Bobber Settings")]
    [SerializeField] private GameObject _Bobber;
    [SerializeField] private GameObject bobberInstance;

    public Transform _bobberSpawnPoint;

    [Range(10.0f, 30.0f)]
    [SerializeField] private float _bobberSpawnRangeMax = 3f;
    [Range(0.1f, 10.0f)]
    [SerializeField] private float _bobberSpawnRangeMin = 12f;

    [Header("Bobber Raycast Mask Detection")]
    [SerializeField] private LayerMask groundMask;

    [Header("Mouse Crossshair For Boat Fishing")]
    [SerializeField] private GameObject _mouseFx;
    private GameObject mouseFxInstance;

    [Header("Slider For Fishing (to be moved to seperate ui script)")]
    [SerializeField] private Slider _fishCaughtSlider;
    [SerializeField] private Slider _fishingThrowSlider;

    private bool isFishing;
    private bool barAtTop;

    [Header("Fishing Line (might move to seprate script)")]
    [SerializeField] private LineRenderer _LineRenderer;

    private GameObject caughtFish;

    [Header("Caught Fish Text (to be moved to seperate ui script)")]
    [SerializeField] private TMP_Text catches;
    private int numCaught = 0;

    private Camera mainCamera;
    private Camera playerCamera;

    private bool inBoat;

    [Header("Charge Up Velocity Limit & Minimum")]
    [Range(1.0f, 50.0f)]
    [SerializeField] private float _chargeUpMax = 15f;
    [Range(0.01f, 0.3f)]
    [SerializeField] private float _chargeUpMultiplier = 0.1f;
    private float throwCharge = 1.0f;

    [Header("Gizmos")]

    [SerializeField] private bool gizmosActive = true;
    private bool spawnFail;
    private Vector3 throwPos;

    [Header("TESTING (WILL BE REMOVED)")]
    [SerializeField] private float TestVelocityMultiplier; //remove once velocity range is fixed


    private void Start() 
    {
        //Events Init
        EventManager.Instance.OnFishFished += Fishking;
        EventManager.Instance.OnBoatEnter += CamBoatSwitch;
        EventManager.Instance.OnBoatExit += CamPlayerSwitch;

        //Sets camera up for raycast (not necessary, change at some point)
        mainCamera = Camera.main;
        playerCamera = mainCamera;

        //Init for target reticle when on boat
        mouseFxInstance = Instantiate(_mouseFx, transform.position, Quaternion.identity);
        mouseFxInstance.SetActive(false);

        mouseFxInstance.transform.parent = transform;

        /*        Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;*/
    }

    private void Update() 
    {
        //Player start fishing 
        if (!isFishing) 
        {
            if (Input.GetMouseButton(0) && !inBoat)
            {
                //Used for charging up lob, if player is not in a boat
                if (throwCharge < _chargeUpMax)
                    throwCharge += _chargeUpMultiplier;
                
                ObliterateBobber();

                if (!_fishingThrowSlider.gameObject.activeSelf)
                    _fishingThrowSlider.gameObject.SetActive(true);

                _fishingThrowSlider.value = Mathf.InverseLerp(0f, _chargeUpMax, throwCharge);
            }

            else if (Input.GetMouseButtonUp(0) && throwCharge > 1f && !inBoat)
                LobBobber();
            else if (Input.GetMouseButtonDown(0) && inBoat)
                LobBobber();
        }
        //Used for once player has caught fish
        else 
        {
            Bargame();

            if (Input.GetMouseButtonDown(0))
            {
                ObliterateBobber();

                if (_fishCaughtSlider.value >= 0.4 && _fishCaughtSlider.value <= 0.6)
                    Caught();
                else
                    Escape();

                StartCoroutine(FishingCooldown());
            }               
        }

        //Checks if bobber exists, if so enables line to it & checks if it is too far from player
        if (bobberInstance != null)
        {
            if (IsBobberTooFar())
            {
                ObliterateBobber();

                print("too far");
                if (caughtFish != null)
                    Escape();
            }

            _LineRenderer.enabled = true;
            _LineRenderer.SetPosition(0, _bobberSpawnPoint.position);
            _LineRenderer.SetPosition(1, bobberInstance.transform.position);
        }
        else
            _LineRenderer.enabled = false;
    }

    private void FixedUpdate()
    {
        if (inBoat)
            MouseMoveFx();
    }
    public float distanceBetween = 0f;
    private void MouseMoveFx()
    {
        //gets position of mouse in relation to world space
        var (success, position) = GetMousePosition();

        if (success)
        {
            distanceBetween = (_bobberSpawnPoint.position - position).sqrMagnitude;

            //Disables target reticle if outside range limits
            if (distanceBetween > (_bobberSpawnRangeMax  * _bobberSpawnRangeMax) || distanceBetween < (_bobberSpawnRangeMin * _bobberSpawnRangeMin))
            {
                mouseFxInstance.SetActive(false);
                RotatePlayerToMouse(position);
                return;
            }

            //Enables target reticle if inside range limits & currently not fishing
            if (!isFishing)
            {
                mouseFxInstance.SetActive(true);
                mouseFxInstance.transform.position = position;
            }else
                mouseFxInstance.SetActive(false);

            RotatePlayerToMouse(position);
        }
        else
            mouseFxInstance.SetActive(false);       
    }

    IEnumerator FishingCooldown()
    {
        yield return new WaitForSeconds(0.1f);
        isFishing = false;
    }

    private void RotatePlayerToMouse(Vector3 pos)
    {
        //Sets players rotation to bobber
        //Needs rework as if boat is not flat, wierd things happen

        //Move roatation limits (+/- 90)
        //Do this at some point

        transform.LookAt(pos, Vector3.forward);

        transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
    }

    private void ObliterateBobber() 
    {
        if (bobberInstance != null)
            Destroy(bobberInstance);
    }

    private void LobBobber()
    {
        _fishingThrowSlider.value = 0f;
        _fishingThrowSlider.gameObject.SetActive(false);

        ObliterateBobber();

        if (inBoat)
        {
            //gets position of mouse in relation to world space
            var (success, position) = GetMousePosition();

            throwPos = position;    //For Gizmo 

            if (success)
            {
                //checks if mouse position is within limits, if so spawns bobber
                float distanceBetween = Vector3.Distance(_bobberSpawnPoint.position, position);

                if (distanceBetween > _bobberSpawnRangeMax || distanceBetween < _bobberSpawnRangeMin)
                {
                    Debug.LogWarning("Position out of range to spawn bobber: ABORTING SPAWN");
                    spawnFail = true;   //For Gizmo
                    return;
                }

                spawnFail = false;  //For Gizmo

                float velocityMarkiplier = TestVelocityMultiplier;

                //Adds velocity so bobber goes in mouse direction
                //needs rework as bobber overshoots at close distances & undershoots at far distances
                SpawnBobber(position - _bobberSpawnPoint.position, velocityMarkiplier);
            }
            else
                Debug.LogWarning("Unable to find position to throw bobber: ABORTING SPAWN");
        }
        else
        {
            throwCharge = Mathf.RoundToInt(throwCharge);

            SpawnBobber(transform.forward, throwCharge);

            //resets charge
            throwCharge = 1.0f;
        }
    }

    private void SpawnBobber(Vector3 direction, float velocity)
    {
        bobberInstance = Instantiate(_Bobber, _bobberSpawnPoint.position, _bobberSpawnPoint.rotation);

        bobberInstance.GetComponent<Rigidbody>().velocity = direction * velocity;

        if (gizmosActive)
            Debug.DrawRay(bobberInstance.transform.position, direction * velocity, Color.green, 2.0f);
    }

    private bool IsBobberTooFar()
    {
        float distanceBetween = (_bobberSpawnPoint.position - bobberInstance.transform.position).sqrMagnitude;

        return distanceBetween > 40 * 40;
    }

    private (bool success, Vector3 position) GetMousePosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, groundMask))              
            return (success: true, position: hitInfo.point);      
        else
            return (success: false, position: Vector3.zero);
    }

    public void Fishking(GameObject fish)
    {
        isFishing = true;
        caughtFish = fish;
        //fish.GetComponent<FishControl>().Caught();
        print("caught!!");
        _fishCaughtSlider.gameObject.SetActive(true);
    }

    public void CamBoatSwitch(Camera boatCam)
    {
        //TBRD
        mainCamera.gameObject.SetActive(false);
        mainCamera = boatCam;

        _fishingThrowSlider.value = 0f;
        _fishingThrowSlider.gameObject.SetActive(false);

        inBoat = true;
    }

    public void CamPlayerSwitch()
    {
        //TBRD
        mainCamera = playerCamera;
        mainCamera.gameObject.SetActive(true);
        mouseFxInstance.SetActive(false);

        inBoat = false;

        throwCharge = 1f;
    }

    void Bargame()
    {
        if (!barAtTop)
        {
            _fishCaughtSlider.value += 0.25f * Time.deltaTime;

            if (_fishCaughtSlider.value >= 1)
                barAtTop = true;
        }
        else
        {
            _fishCaughtSlider.value -= 0.25f * Time.deltaTime;

            if (_fishCaughtSlider.value <= 0)
                barAtTop = false;
        }
    }

    void Caught() 
    {
        //caughtFish.GetComponent<FishControl>().isAboutToDie = true;
        //EventManager.Instance.FishCaught(caughtFish);
        caughtFish.GetComponent<FishControl>().FishToPlayer();

        numCaught++;
        catches.text = "CAUGHT FISH :" + numCaught;
        _fishCaughtSlider.value = 0.5f;
        _fishCaughtSlider.gameObject.SetActive(false);
    }

    void Escape()
    {
        caughtFish.GetComponent<FishControl>().Escape(transform);
        _fishCaughtSlider.value = 0.5f;
        _fishCaughtSlider.gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        if (gizmosActive)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_bobberSpawnPoint.position, _bobberSpawnRangeMax);
            Gizmos.DrawWireSphere(_bobberSpawnPoint.position, _bobberSpawnRangeMin);

            if (mouseFxInstance != null)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawWireSphere(mouseFxInstance.transform.position, 1f);
            }

            if (bobberInstance != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(bobberInstance.transform.position, 1f);
            }

            if (!spawnFail)
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;

            if (throwPos != Vector3.zero)        
                Gizmos.DrawWireSphere(throwPos, 1f);          

            if (throwCharge > 1)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(_bobberSpawnPoint.position, transform.forward * throwCharge);
            }
        }
    }
    
    private void OnDestroy()
    {
        EventManager.Instance.OnFishFished -= Fishking;
        EventManager.Instance.OnBoatEnter -= CamBoatSwitch;
        EventManager.Instance.OnBoatExit -= CamPlayerSwitch;
    }

    /*
    public LineRenderer lineRenderer;
    public int lPoints = 100;
    public float timePoints = 0.01f;
    public float sped;

    // Start is called before the first frame update
    void DrawTrajectory()
    {
        print("a");
        Vector3 velocity = sped * transform.forward;
        lineRenderer.positionCount = lPoints;
        float time = 0;

        for (int i = 0; i < lPoints; i++)
        {
            var x = (velocity.x * time) + (Physics.gravity.x / 2 * time * time);
            var y = (velocity.y * time) + (Physics.gravity.y / 2 * time * time);
            var z = (velocity.z * time) + (Physics.gravity.z / 2 * time * time);
            Vector3 point = new Vector3(x, y, z);
            lineRenderer.SetPosition(i, transform.position + point);
            time += timePoints;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (lineRenderer != null)
        {
            if (Input.GetMouseButton(1))
            {
                DrawTrajectory();
                lineRenderer.enabled = true;
            }else
                lineRenderer.enabled = false;
        }
    }*/
}
