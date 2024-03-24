using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
	/// Used to implement mouselook on the vertical axis.
	public Camera playerCamera;

	/// The speed the player moves at.
	[Range(0.1f, 20.0f)]
	public float moveSpeed = 10.0f;
	// /// The speed at which footstep sounds are triggered.
	// [Range(0.01f, 1.0f)]
	// public float footstepRate = 0.3f;
	///	The amount of force to apply when the player jumps.
	[Range(1.0f, 32.0f)]
	public float jumpForce = 10.0f;
	/// How much gravity to apply to the player.
	[Range(0.0f, 64.0f)]
	public float gravity = 32.0f;

	// /// The Wwise event to trigger a footstep sound.
	// public AK.Wwise.Event footstepSound = new AK.Wwise.Event();
	// ///	The Wwise event to trigger a jump sound.
	// public AK.Wwise.Event jumpSound = new AK.Wwise.Event();
	// ///	The Wwise event to trigger a jump landing sound.
	// public AK.Wwise.Event jumpLandSound = new AK.Wwise.Event();

	// public AK.Wwise.Event LandSound = new AK.Wwise.Event();

	/// Used to set the player's rotation around the y-axis.
	private float playerRotation;
	/// Used to implement mouselook on the vertical axis.
	private float viewY;

	///	Used to let the player jump.
	private float jumpVelocity = 0.0f;

	///	Used to determine when to trigger footstep sounds.
	//private bool walking = false;
	///	Used to determine when to trigger footstep sounds.
	//private float walkCount = 0.0f;

	///	Used to ensure we play the Jump Land sound when we hit the ground.
	private bool inAir = false;
	///	Used to ensure we don't trigger a false Jump Land when the game starts.
	private int inAirCount = 0;

	public int inAirMin;

	private bool isJumping = false;

	// public AK.Wwise.RTPC rtpc = null;

	/// We use this to hide the mouse cursor.
	void OnEnable()
	{
/*		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;*/
	}
	
	/// This is where we move the Player object and Camera.
	public void Update()
	{
		float speed = moveSpeed;

		if(Input.GetButton("Run"))
			speed *= 4.0f;

		//Get our current WASD speed.
		Vector3 strafe = new Vector3(Input.GetAxis("Horizontal") * speed, 0.0f, 0.0f);
		float forwardSpeed = Input.GetAxis("Vertical") * speed;

		if(((Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.0f) ||
			(Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.0f)))
		{
			//walking = true;
		}
		else
		{
			//walking = false;

			// walkCount = footstepRate;
		}

		//Get our current mouse/camera rotation.
		playerRotation = Input.GetAxis("Mouse X") * 6.0f;

		playerCamera.transform.Rotate(new Vector3(viewY, 0.0f, 0.0f));

		viewY += Input.GetAxis("Mouse Y") * 4.0f;

		//Don't let the player rotate the camera more than 90 degrees on the
		//y-axis.
		viewY = Mathf.Clamp(viewY, -90.0f, 90.0f);

		//Rotate the camera up/down.
		playerCamera.transform.Rotate(new Vector3(-viewY, 0.0f, 0.0f));

        //////////////////////////////Vector3.Angle use it

        //Rotate player (clamped so they can't move so fast they make themselves
        //sick).
        Mathf.Clamp(playerRotation, -5.0f, 5.0f);
		transform.Rotate(0.0f, playerRotation, 0.0f);

		//Jump player.
		CharacterController controller = GetComponent<CharacterController>();
		Vector3 jumpVector = Vector3.zero;

        if (Input.GetKey("escape"))
            Application.Quit();

        if (controller.enabled == false)
			return;

		if(!controller.isGrounded)
		{
			inAir = true;
			++inAirCount;
		}
		else
		{
			if(inAir)//&&(inAirCount < 1))
			{
			
				if (isJumping)
				{
					// jumpLandSound.Post(gameObject);
					isJumping = false;
					//print("Landed: Jump");
				}else if (inAirCount >= inAirMin){

					inAirCount += 15;

					if (inAirCount > 100)
						inAirCount = 100;

					// rtpc.SetValue(gameObject, inAirCount);
					// LandSound.Post(gameObject);
					//print("Landed: Fall " + inAirCount);
				}
				inAirCount = 0;
			}
			inAir = false;
		}

		// if(inAir && inAirCount > 0)
		// 	--inAirCount;

		// if(walking && !inAir)
		// {
		// 	walkCount += Time.deltaTime * (speed/10.0f);

		// 	if(walkCount > footstepRate)
		// 	{
		// 		footstepSound.Post(gameObject);

		// 		walkCount = 0.0f;
		// 	}
		// }

		//If the player is holding the jump button down, AND they're not yet
		//jumping AND on the ground, OR they are jumping but they've not reached
		//the top of the jump, increase their jumpAmount and move them
		//accordingly on the y-axis.
		if(Input.GetButton("Jump"))
		{
			if((jumpVelocity <= 0.0f) && controller.isGrounded)
			{
				// jumpSound.Post(gameObject);
				isJumping = true;
				
				jumpVelocity = jumpForce;
			}
		}

		//Move player.
		Vector3 moveDirection = Vector3.zero;

		//Set the player's direction based on the direction of the mouse and
		//which WASD keys they're pressing.
		moveDirection = transform.rotation * ((Vector3.forward * forwardSpeed) + strafe);
		moveDirection.y = jumpVector.y;
		moveDirection.y = jumpVelocity;

		//Apply gravity.
		jumpVelocity -= gravity * Time.deltaTime;
		if(controller.isGrounded && (jumpVelocity < 1.0f))
			jumpVelocity = -1.0f;

		//Finally, apply the updated direction to the player's Controller (this
		//will figure out any collisions with the ground, other objects, etc.).
		controller.Move(moveDirection * Time.deltaTime);
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		var pushPower = 1.5f;

        Rigidbody body = hit.collider.attachedRigidbody;

        // no rigidbody
        if (body == null || body.isKinematic)
            return;

        // We dont want to push objects below us
        if (hit.moveDirection.y < -0.3f)
            return;

        // Calculate push direction from move direction,
        // we only push objects to the sides never up and down
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // If you know how fast your character is trying to move,
        // then you can also multiply the push velocity by that.

        // Apply the push
        body.velocity = pushDir * pushPower;

    }

	private void OnDisable()
	{
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None; 
    }
}
