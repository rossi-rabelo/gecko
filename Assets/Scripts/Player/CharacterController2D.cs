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

	public Transform firePoint;

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

	public void Move(float moveHorizontal, bool crouch, bool jump, bool climbing)
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

				
				if (m_FacingRight)
				{
					RaycastHit2D hit = Physics2D.Raycast(firePoint.position, Vector2.right, 5f);
					Debug.DrawRay(firePoint.position, Vector2.right * 5f, Color.red);


					if (hit && hit.collider != null)
					{
						if (hit.collider.CompareTag("Wall") && climbing)
                        {
							DirectionalMovement(moveHorizontal, true);

                        } else
                        {
							DirectionalMovement(moveHorizontal, false);
						}
					}
					else if (climbing && !m_Grounded)
					{
						DirectionalMovement(moveHorizontal, true);
					}
					else
					{
						DirectionalMovement(moveHorizontal, false);
					}

				}
				else
				{
					RaycastHit2D hit = Physics2D.Raycast(firePoint.position, Vector2.left, 5f);
					Debug.DrawRay(firePoint.position, Vector2.left * 5f, Color.red);


					if (hit && hit.collider != null)
					{
						if (hit.collider.CompareTag("Wall") && climbing)
						{
							DirectionalMovement(moveHorizontal, true);
						}
						else
						{
							DirectionalMovement(moveHorizontal, false);
						}
					}
					else if (climbing && !m_Grounded)
					{
						DirectionalMovement(moveHorizontal, true);

					} else 
					{
						DirectionalMovement(moveHorizontal, false);
					}

				}

				// Disable one of the colliders when crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = false;
			}
			else
			{

				DirectionalMovement(moveHorizontal, false);

				// Enable the collider when not crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = true;

				if (m_wasCrouching)
				{
					m_wasCrouching = false;
					OnCrouchEvent.Invoke(false);
				}
			}


			// If the input is moving the player right and the player is facing left...
			if (moveHorizontal > 0 && !m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
			// Otherwise if the input is moving the player left and the player is facing right...
			else if (moveHorizontal < 0 && m_FacingRight)
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
		}
	}

	private void DirectionalMovement(float moveHorizontal, bool isWall)
    {

		Vector3 targetVelocity;

		if (isWall)
		{
			float multiplicatorFactor = m_FacingRight ? -1 : 1;
			
			targetVelocity = new Vector2(m_Rigidbody2D.velocity.x, Math.Abs(moveHorizontal) * 10f * multiplicatorFactor);
			m_Rigidbody2D.gravityScale = 0;
		}
		else
		{
			// moveHorizontal the character by finding the target velocity
			targetVelocity = new Vector2(moveHorizontal * 10f, m_Rigidbody2D.velocity.y);
			m_Rigidbody2D.gravityScale = 3;
		}
		// And then smoothing it out and applying it to the character
		m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
	}

	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		transform.Rotate(0f, 180f, 0f);
	}
}