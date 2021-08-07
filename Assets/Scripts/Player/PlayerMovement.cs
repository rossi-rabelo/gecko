using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public CharacterController2D controller;
    public Animator animator;

    public float runSpeed = 40f;
    public float crawlSpeed = 60f;

    float horizontalMove = 0f;
    public bool jump = false;
    public bool crouch = false;

    public bool climbing = false;
    public bool ceiling = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));
        
        if (Input.GetButtonDown("Jump"))
        {
            animator.SetBool("Jumping", true);
            jump = true;
        }

        if (Input.GetButtonDown("Crouch"))
        {
            crouch = !crouch;
        }

    }

    public void OnLanding ()
    {
        animator.SetBool("Jumping", false);
    }

    private void FixedUpdate()
    {

        controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump, climbing, ceiling);
        jump = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.tag == "Wall")
        {
            climbing = true;
        }

        if (collision.gameObject.tag == "Ceiling")
        {
            ceiling = true;
        }

    }

    private void OnCollisionExit2D(Collision2D collision)
    {

        if (collision.gameObject.tag == "Wall")
        {
            climbing = false;
        }

        if (collision.gameObject.tag == "Ceiling")
        {
            ceiling = false;
        }

    }

}
