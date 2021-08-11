using UnityEngine;
using UnityEngine.Events;
using System;

public class CharacterController2D : MonoBehaviour
{
	[SerializeField] private float m_JumpForce = 600f;                          // Amount of force added when the player jumps.
	[Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;          // Amount of maxSpeed applied to crouching movement. 1 = 100%
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;  // How much to smooth out the movement
	[SerializeField] private bool m_AirControl = false;                         // Whether or not a player can steer while jumping;
	[SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
	[SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
	[SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings
	[SerializeField] private Collider2D m_CrouchDisableCollider;                // A collider that will be disabled when crouching

	const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
	public bool m_Grounded;            // Whether or not the player is grounded.
	const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up
	private Rigidbody2D m_Rigidbody2D;
	public bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private Vector3 m_Velocity = Vector3.zero;
	
	public bool isOnWall = false;
	public bool isTurning = false;

	public Transform wallCheck;
	public Transform outerWallCheckFront;
	public Transform outerWallCheckBack;
	public LayerMask climbable;

	public float lerpSpeed;
	private float lerpPercent = 0f;

	public bool isLeft = false;
	public bool hitGround = false;
	public float previousAngle = 0f;
	public float actualAngle = 0f;

	public Vector2 playerNormal = new Vector2(0f, 1f);
	public Vector2 hitNormal;

	public bool climbing = false;
	public bool jumping = false;

	public bool changedFromLeft = false;
	public bool changedFromRight = false;

	[Header("Events")]
	[Space]

	public UnityEvent OnLandEvent;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	public BoolEvent OnCrouchEvent;
	private bool m_wasCrouching = false;

	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();


		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		if (OnCrouchEvent == null)
			OnCrouchEvent = new BoolEvent();
	}

	private void Start()
	{
		hitNormal = playerNormal;
	}

    private void FixedUpdate()
	{
		bool wasGrounded = m_Grounded;
		m_Grounded = false;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
			{
				m_Grounded = true;
				if (!wasGrounded)
					OnLandEvent.Invoke();
			}
		}
	}


	public void DoKnockback(float knockbackForce)
    {
		float knockbackDirection = knockbackForce;

		if (m_FacingRight)
		{
			knockbackDirection *= -1;
		}

		Vector3 targetVelocity = new Vector2(knockbackDirection, m_Rigidbody2D.velocity.y);
		// And then smoothing it out and applying it to the character
		m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

	}

	public void Move(float move, bool crouch, bool jump)
	{
		// If crouching, check to see if the character can stand up
		if (!crouch)
		{
			// If the character has a ceiling preventing them from standing up, keep them crouching
			if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
			{
				crouch = true;
			}
		}

		
		//only control the player if grounded or airControl is turned on
		if (m_Grounded || m_AirControl)
		{

			// If crouching
			if (crouch)
			{
				if (!m_wasCrouching)
				{
					m_wasCrouching = true;
					OnCrouchEvent.Invoke(true);
				}

				// Reduce the speed by the crouchSpeed multiplier
				// move *= m_CrouchSpeed;

				// Disable one of the colliders when crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = false;

				RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, wallCheck.right, 1f, climbable);
				Debug.DrawRay(wallCheck.position, wallCheck.right * 1f, Color.red);

				RaycastHit2D hitFront = Physics2D.Raycast(outerWallCheckFront.position, -outerWallCheckFront.right, .5f, climbable);
				Debug.DrawRay(outerWallCheckFront.position, -outerWallCheckFront.right * .5f, Color.red);

				RaycastHit2D hitBack = Physics2D.Raycast(outerWallCheckBack.position, outerWallCheckBack.right, .5f, climbable);
				Debug.DrawRay(outerWallCheckBack.position, outerWallCheckBack.right * .5f, Color.red);

				if (!m_FacingRight && m_Grounded)
				{
					isLeft = true;
				}
				else if (m_FacingRight && m_Grounded)
				{
					isLeft = false;
				}

				if (m_FacingRight && isLeft)
                {
					changedFromLeft = true;
					changedFromRight = false;

				}

				if (!m_FacingRight && !isLeft)
                {
					changedFromRight = true;
					changedFromLeft = false;
				}

				if (m_Grounded)
                {
					changedFromLeft = false;
					changedFromRight = false;
				}

				if (hit.collider != null)
				{

					if (!hit.collider.CompareTag("Player"))
					{

						if (!isTurning)
						{
							hitNormal = hit.normal;
						}

						if (hit.collider.CompareTag("Ground"))
						{
							isOnWall = false;
							isTurning = true;
							hitGround = true;

							climbing = false;

						}
						else
						{
							hitGround = false;
						}

						if (hit.collider.CompareTag("Wall"))
						{
							isOnWall = true;
							isTurning = true;

						}
					}
				}
				else
                {
					if (!climbing && !isTurning)
                    {
						isOnWall = false;
					}
				}

				if(isTurning)
                {
					Turn();
				}

			}
			else
			{
				// Enable the collider when not crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = true;

				if (m_wasCrouching)
				{
					m_wasCrouching = false;
					OnCrouchEvent.Invoke(false);
				}
			}

			Movement(move, crouch);

			// If the input is moving the player right and the player is facing left...
			if (move > 0 && !m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
			// Otherwise if the input is moving the player left and the player is facing right...
			else if (move < 0 && m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
		}


		// If the player should jump...
		if (m_Grounded && jump)
		{
			// Add a vertical force to the player.
			m_Grounded = false;
			m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
			jumping = true;
		}

		if (m_Grounded && !jump && jumping)
        {
			jumping = false;
        }

	}

	private void Turn()
    {
		previousAngle = transform.eulerAngles.z;

		// float direction = 90;
		float direction = Vector2.Angle(playerNormal, hitNormal);

		//m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, -hitNormal * 10f, ref m_Velocity, m_MovementSmoothing);

		float degree = changedFromLeft || changedFromRight ? (actualAngle - direction) * -1 : actualAngle + direction;

		// degree = Mathf.Repeat(degree, 360); // Faz mesma coisa que degree % 360
		
		
		if (hitGround)
        {
			degree = 0;
		}

		

		lerpPercent = Mathf.MoveTowards(lerpPercent, 1f, Time.fixedDeltaTime * lerpSpeed);

		Quaternion target = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, degree);
		transform.rotation = Quaternion.Slerp(transform.rotation, target, lerpPercent);

		if (lerpPercent >= 1f)
		{
			isTurning = false;
			actualAngle = transform.eulerAngles.z;
			playerNormal = hitNormal;
		}
	}

	private void Movement(float move, bool crouch)
    {
		
		Vector2 targetVelocity;

		/*
		float switchChange = isLeft ? 1 : -1;

		if (m_FacingRight)
        {
			if (isWall)
            {
				if (previousAngle > 0 && previousAngle < 180)
				{
					targetVelocity = new Vector2(m_Rigidbody2D.velocity.x, move * -10f * switchChange);
					m_Rigidbody2D.gravityScale = 0;
				}
				else
				{
					targetVelocity = new Vector2(m_Rigidbody2D.velocity.x, move * 10f * switchChange);
					m_Rigidbody2D.gravityScale = 0;
				}
			} else
            {
				if (isOnCeiling)
                {
					m_Rigidbody2D.gravityScale = 0;
					targetVelocity = new Vector2(move * -10f, m_Rigidbody2D.velocity.y);

				} else
                {
					m_Rigidbody2D.gravityScale = 3;
					targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
				}

			}

        } else
        {
			if (isWall)
			{
				if (previousAngle > 0 && previousAngle < 180)
                {
					targetVelocity = new Vector2(m_Rigidbody2D.velocity.x, move * -10f * switchChange);
					m_Rigidbody2D.gravityScale = 0;
				} else
                {
					targetVelocity = new Vector2(m_Rigidbody2D.velocity.x, move * 10f * switchChange);
					m_Rigidbody2D.gravityScale = 0;
				}
			}
			else
			{
				if (isOnCeiling)
				{
					m_Rigidbody2D.gravityScale = 0;
					targetVelocity = new Vector2(move * -10f, m_Rigidbody2D.velocity.y);
				}
				else
				{
					m_Rigidbody2D.gravityScale = 3;
					targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
				}
			}
		}
		*/

		if ((isOnWall || climbing) && crouch)
		{
		
			targetVelocity = new Vector2(0, 0);
			m_Rigidbody2D.gravityScale = 0;

		}
		else
		{
			if (!isTurning)
			{
				targetVelocity = new Vector2(0, m_Rigidbody2D.velocity.y);
				m_Rigidbody2D.gravityScale = 3;
			} else
            {
				targetVelocity = new Vector2(0, 0);
				m_Rigidbody2D.gravityScale = 0;
			}
		}


		if (move > 0)
		{
			float angleNormal = Mathf.Atan2(playerNormal.y, playerNormal.x) * Mathf.Rad2Deg;
			float radiandos = (angleNormal - 90) * Mathf.Deg2Rad;
			float jumpVelocity = jumping || (!m_Grounded && !crouch) ? m_Rigidbody2D.velocity.y : Mathf.Sin(radiandos) * Math.Abs(move) * 10f;
			targetVelocity = new Vector2(Mathf.Cos(radiandos) * Math.Abs(move) * 10f, jumpVelocity);


		}
		else if (move < 0)
		{
			float angleNormal = Mathf.Atan2(playerNormal.y, playerNormal.x) * Mathf.Rad2Deg;
			float radiandos = (angleNormal + 90) * Mathf.Deg2Rad;
			float jumpVelocity = jumping || (!m_Grounded && !crouch) ? m_Rigidbody2D.velocity.y : Mathf.Sin(radiandos) * Math.Abs(move) * 10f;
			targetVelocity = new Vector2(Mathf.Cos(radiandos) * Math.Abs(move) * 10f, jumpVelocity);

		}

		targetVelocity = new Vector2(Mathf.Clamp(targetVelocity.x, -40, 40), Mathf.Clamp(targetVelocity.y, -40, 40));

		m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

	}
	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		transform.Rotate(0f, 180f, 0f);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{

		if (collision.gameObject.tag == "Wall")
		{
			climbing = true;
		}

	}

}