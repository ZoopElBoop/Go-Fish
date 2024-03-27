using System.Collections;
using UnityEngine;

public class Fishing : MonoBehaviour
{
    #region Variables

    [Header("Rod Settings")]
    [SerializeField] private GameObject fishingRod;

    [Header("Bobber Settings")]
    [SerializeField] private GameObject _Bobber;
    private GameObject bobberInstance;
    private Rigidbody bobberInstanceRb;
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

    [Header("Fishing Line")]
    [SerializeField] private LineRenderer _LineRenderer;

    [Header("Charge Up Velocity Limit & Minimum")]
    [Range(1.0f, 50.0f)]
    [SerializeField] private float _chargeUpMax = 15f;
    [Range(0.01f, 0.3f)]
    [SerializeField] private float _chargeUpMultiplier = 0.1f;
    public float throwCharge;

    [Header("Gizmos")]
    [SerializeField] private bool gizmosActive = true;
    private bool spawnFail;
    private Vector3 throwPos;

    //Cache of caught fish
    private FishControl caughtFishScript;

    //Cameras for switching between boat/1st person for racasts, to be changed
    private Camera mainCamera;
    private Camera playerCamera;

    //Boat Rigidbody for adding to bobber velocity when spawning
    [HideInInspector] public Rigidbody boatRB;

    //Bool states
    private bool inBoat;
    private bool isFishing;
    private bool rodEquiped = false;

    [Header("TESTING (WILL BE REMOVED)")]
    [SerializeField] private float TestVelocityMultiplier; //remove once velocity range is fixed

    #endregion

    #region Start Functions

    private void Start() 
    {
        //Events Init
        EventManager.Instance.OnFishCaught += Fishking;
        EventManager.Instance.OnBoatEnter += SwitchToBoatCamera;
        EventManager.Instance.OnBoatExit += SwitchToPlayerCamera;
        EventManager.Instance.OnDestroyWobber += ObliterateBobber;

        //Sets camera up for raycast (not necessary, change at some point)
        mainCamera = Camera.main;
        playerCamera = mainCamera;

        //Init for target reticle when on boat
        mouseFxInstance = Instantiate(_mouseFx, transform.position, Quaternion.identity);
        mouseFxInstance.SetActive(false);

        mouseFxInstance.transform.parent = transform;

        fishingRod.SetActive(false);
        _LineRenderer.enabled = false;
    }

    private void OnEnable()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void ResetFishing()
    {
        ObliterateBobber();

        isFishing = false;
        _LineRenderer.enabled = false;
        UIManager.Instance.ThrowSliderActive(false);

        if (caughtFishScript != null)
            Escape();

        if (mouseFxInstance != null)
            mouseFxInstance.SetActive(false);
    }

    #endregion

    #region Update Functions

    private void Update() 
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            //Resets fishing if player de-equips rod while fishing
            if (rodEquiped)            
                ResetFishing();         

            rodEquiped = !rodEquiped;
            fishingRod.SetActive(rodEquiped);
        }

        if (rodEquiped)
        {
            FishingControl();

            BobberLine();           
        }
    }

    private void FixedUpdate()
    {
        if (inBoat && rodEquiped)
            MouseMoveFx();
    }

    #endregion

    #region Fishing Control

    private void FishingControl() 
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

                UIManager.Instance.ThrowSliderActive(true);
                UIManager.Instance.ThrowSlider(_chargeUpMax, throwCharge);
            }

            else if (Input.GetMouseButtonUp(0) && throwCharge > 1f && !inBoat)  //throws bobber when player not in boat
                LobBobber();
            else if (Input.GetMouseButtonDown(0) && inBoat)     //throws bobber when player in boat
                LobBobber();
        }
        //Used for once player has caught fish
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                ObliterateBobber();

                float sliderVal = UIManager.Instance.GetFishingSliderValue();

                if (sliderVal >= 0.4 && sliderVal <= 0.6)
                    Caught();
                else
                    Escape();

                StartCoroutine(FishingCooldown());
            }
        }
    }

    IEnumerator FishingCooldown()
    {
        yield return new WaitForSeconds(0.25f);
        isFishing = false;
    }

    private (bool success, Vector3 position) GetMousePosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, groundMask))
            return (success: true, position: hitInfo.point);
        else
            return (success: false, position: Vector3.zero);
    }

    #endregion

    #region Boat Fishing

    private void MouseMoveFx()
    {
        //gets position of mouse in relation to world space
        var (success, position) = GetMousePosition();

        print(success);

        if (success)
        {
            float distanceBetween = (_bobberSpawnPoint.position - position).sqrMagnitude;

            //Disables target reticle if outside range limits
            if (distanceBetween > (_bobberSpawnRangeMax  * _bobberSpawnRangeMax) || distanceBetween < (_bobberSpawnRangeMin * _bobberSpawnRangeMin))
            {
                Debug.LogWarning($"Mouse ({distanceBetween}) not in range");
                mouseFxInstance.SetActive(false);
                RotatePlayerToMouse(position);
                return;
            }

            //Enables target reticle if inside range limits & currently not fishing
            if (!isFishing)
            {
                mouseFxInstance.SetActive(true);
                mouseFxInstance.transform.position = position;
            }
            else
                mouseFxInstance.SetActive(false);

            RotatePlayerToMouse(position);
        }
        else
            mouseFxInstance.SetActive(false);       
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

    #endregion

    #region Bobber Functions

    private void BobberLine()
    {
        //Checks if bobber exists, if so enables line to it & checks if it is too far from player
        if (bobberInstance != null && bobberInstance.activeSelf)
        {
            if (IsBobberTooFar())
            {
                ObliterateBobber();

                if (caughtFishScript != null)
                    Escape();
            }

            _LineRenderer.enabled = true;
            _LineRenderer.SetPosition(0, _bobberSpawnPoint.position);
            _LineRenderer.SetPosition(1, bobberInstance.transform.position);
        }
        else
            _LineRenderer.enabled = false;
    }
    private bool IsBobberTooFar()
    {
        float distanceBetween = (_bobberSpawnPoint.position - bobberInstance.transform.position).sqrMagnitude;

        return distanceBetween > 80 * 80;
    }

    private void ObliterateBobber() 
    {
        if (bobberInstance != null)
            bobberInstance.SetActive(false);
    }

    private void LobBobber()
    {
        UIManager.Instance.ThrowSliderActive(false);

        ObliterateBobber();

        if (inBoat)
        {
            //gets position of mouse in relation to world space
            var (success, position) = GetMousePosition();

            throwPos = position;    //For Gizmo 

            if (success)
            {
                //checks if mouse position is within limits, if so spawns bobber
                float distanceBetween = (_bobberSpawnPoint.position - position).sqrMagnitude;

                if (distanceBetween > (_bobberSpawnRangeMax * _bobberSpawnRangeMax) || distanceBetween < (_bobberSpawnRangeMin * _bobberSpawnRangeMin))
                {
                    Debug.LogWarning($"Position ({distanceBetween}) out of range to spawn bobber: ABORTING SPAWN");
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
        if (bobberInstance == null)
        {
            bobberInstance = Instantiate(_Bobber, _bobberSpawnPoint.position, _bobberSpawnPoint.rotation);
            bobberInstanceRb = bobberInstance.GetComponent<Rigidbody>();
        }
        else
            bobberInstance.SetActive(true);

        bobberInstance.transform.SetPositionAndRotation(_bobberSpawnPoint.position, _bobberSpawnPoint.rotation);

        if (inBoat)
            bobberInstanceRb.velocity = direction * velocity + boatRB.velocity;
        else
            bobberInstanceRb.velocity = direction * velocity;

        if (gizmosActive)
            Debug.DrawRay(bobberInstance.transform.position, direction * velocity, Color.green, 2.0f);
    }

    #endregion

    #region Fishing Events

    public void Fishking(GameObject fish)
    {
        caughtFishScript = GameManager.Instance.GetFishConrolScript(fish);

        if (caughtFishScript == null)
            return;

        isFishing = true;
        UIManager.Instance.FishingSliderActive(true);
    }

    public void SwitchToBoatCamera(Camera boatCam)
    {
        print("bobt");
        mainCamera.gameObject.SetActive(false);
        mainCamera = boatCam;

        UIManager.Instance.ThrowSliderActive(false);
        inBoat = true;
    }

    public void SwitchToPlayerCamera()
    {
        mainCamera = playerCamera;
        mainCamera.gameObject.SetActive(true);
        mouseFxInstance.SetActive(false);

        inBoat = false;

        print("playr");

        throwCharge = 1f;
    }

    void Caught()
    {
        caughtFishScript.ActivateFishToPlayer();

        UIManager.Instance.FishingSliderActive(false);

        if (inBoat)
            InventoryManager.Instance.StoreOnBoat(caughtFishScript);
        else
            InventoryManager.Instance.StoreOnPlayer(caughtFishScript);
    }

    void Escape()
    {
        caughtFishScript.Escape(transform);
        UIManager.Instance.FishingSliderActive(false);
    }

    #endregion

    #region Gizmos

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

            if (bobberInstance != null && bobberInstance.activeSelf)
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

    #endregion

    #region End Functions

    private void OnDisable()
    {
        ResetFishing();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnDestroy()
    {
        EventManager.Instance.OnFishCaught -= Fishking;
        EventManager.Instance.OnBoatEnter -= SwitchToBoatCamera;
        EventManager.Instance.OnBoatExit -= SwitchToPlayerCamera;
        EventManager.Instance.OnDestroyWobber -= ObliterateBobber;
    }

    #endregion

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
