using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 40f;
    public Rigidbody2D bulletReference;
    public int lifeTime = 3;
    public int bulletDamage = 10;

    void Start()
    {
        bulletReference.velocity = transform.right * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();

        if (enemyHealth != null)
        {

            enemyHealth.TakeDamage(bulletDamage);
        }

        Destroy(gameObject);
    }
}
