using UnityEngine;

public class Crosshair : MonoBehaviour
{
	public Texture2D crosshairImage;
	public Texture2D activeCrosshairImage;

	public LayerMask raycastMask;

	private bool hitActive;
	private GameObject buton;

	void FixedUpdate()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		RaycastHit[] hit = new RaycastHit[1];

		int hits = Physics.RaycastNonAlloc(ray, hit, 10f, raycastMask, QueryTriggerInteraction.Ignore);

		if (hits > 0)
		{
			hitActive = true;
			buton = hit[0].collider.gameObject;
		}
		else
		{
            hitActive = false;
            buton = null;
        }
	}

	private void Update()
	{
		if (buton != null && Input.GetMouseButtonDown(0))
			buton.SetActive(false);
	}

	void OnGUI()
	{
        Texture2D crosshairTexture = crosshairImage;

		if(hitActive)
			crosshairTexture = activeCrosshairImage;

		float xMin = (Screen.width * 0.5f) - (crosshairTexture.width/8.0f);
		float yMin = (Screen.height * 0.5f) - (crosshairTexture.height/8.0f);
		GUI.DrawTexture(new Rect(xMin, yMin, (crosshairTexture.width * 4.0f)/8.0f, (crosshairTexture.height * 4.0f)/8.0f), crosshairTexture);
	}

	private void OnDrawGizmos()
	{
		if (hitActive)
			Gizmos.color = Color.red;
        else
            Gizmos.color = Color.green;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Gizmos.DrawRay(ray);
	}
}


