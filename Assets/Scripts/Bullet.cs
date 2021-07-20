using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 40f;
    public Rigidbody2D bulletReference;
    public int lifeTime = 3;
    void Start()
    {
        bulletReference.velocity = transform.right * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Enemy")
        {
            // Enemy take damage
            // Modify it later to get an Enemy class and invoke a takeDamage function
        }
        Destroy(gameObject);
    }
}
