using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public int touchDamage = 25;

    public HealthBarScript healthBarScript;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        healthBarScript.SetMaxHealth(maxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHealth <= 0)
        {
            Die();
        }

    }

    void Die()
    {
        Destroy(gameObject);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        healthBarScript.SetHealth(currentHealth);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.SendMessage("TakeDamage", touchDamage);
        }
    }

}
