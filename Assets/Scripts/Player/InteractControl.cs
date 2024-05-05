using UnityEngine;

public class InteractControl : MonoBehaviour
{
	[Header("Crosshair Sprites")]
    [SerializeField] private Texture2D crosshairSprite;
    [SerializeField] private Texture2D activeCrosshairSprite;

    [Header("Interact Setting")]
    [SerializeField][Range(1, 10)] private int interactRange;

	private GameObject objectHit;
    private GameObject objectHitLastFrame;

    private LayerMask interactMask;
    private LayerMask IgnoreinteractMask = -1;
    private bool hitActive;

	private void Start()
	{
		int ignoreLayer = LayerMask.NameToLayer("Interact");
        IgnoreinteractMask &= ~(1 << ignoreLayer);   //sets layer to ignore "Interact" layer
        interactMask |= (1 << ignoreLayer);          //sets layer to only "Interact" layer


        ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
        IgnoreinteractMask &= ~(1 << ignoreLayer);   //sets layer to ignore "Ignore Raycast" layer

    }

    private void OnEnable()
	{
        GameManager.Instance.ShowPlayerMouse(false);
	}

	void FixedUpdate()
	{
        objectHitLastFrame = objectHit;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit[] hit = new RaycastHit[1];

		int interactHits = Physics.RaycastNonAlloc(ray, hit, interactRange, interactMask, QueryTriggerInteraction.Ignore);

		if (interactHits > 0)
		{
            int collisionHits = Physics.RaycastNonAlloc(ray, hit, hit[0].distance, IgnoreinteractMask, QueryTriggerInteraction.Ignore);	//checks if position to interact object is clear

			if (collisionHits == 0)
			{
                objectHit = hit[0].transform.gameObject;
				hitActive = true;

                OnRayExitAndEnter();
				return;
			}
        }

        objectHit = null;
		hitActive = false;

        OnRayExitAndEnter();
    }

	private void OnRayExitAndEnter()
	{
		if (objectHitLastFrame == null && objectHit != null)
		{
            if (objectHit.TryGetComponent<Interactable>(out var temp))
                temp.OnEnter();

			return;
        }

		if (objectHitLastFrame != objectHit)
		{
            if (objectHitLastFrame.TryGetComponent<Interactable>(out var temp))
                temp.OnExit();

			if (objectHit != null && objectHit.TryGetComponent<Interactable>(out var temp2))
                temp2.OnEnter();
        }
    }

    private void Update()
	{
		if (Input.GetMouseButtonDown(0) && objectHit != null)
		{
			if (objectHit.TryGetComponent<Interactable>(out var interactable))
				interactable.ActivateEvent();
        }
	}

	void OnGUI()
	{
        Texture2D crosshairTexture = crosshairSprite;

		if(hitActive)
			crosshairTexture = activeCrosshairSprite;

		float xMin = (Screen.width * 0.5f) - (crosshairTexture.width/8.0f);
		float yMin = (Screen.height * 0.5f) - (crosshairTexture.height/8.0f);
		GUI.DrawTexture(new Rect(xMin, yMin, (crosshairTexture.width * 4.0f)/8.0f, (crosshairTexture.height * 4.0f)/8.0f), crosshairTexture);
	}

	private void OnDrawGizmos()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			Gizmos.DrawRay(transform.position, transform.forward * interactRange);
			return;
		}
#endif
        if (enabled)
		{
            if (hitActive)
			Gizmos.color = Color.red;
			else
			Gizmos.color = Color.green;

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			Gizmos.DrawRay(transform.position, ray.direction * interactRange);
		}
	}
}