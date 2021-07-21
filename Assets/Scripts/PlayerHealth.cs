using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

    public int maxHealth = 100;
    public int currentHealth;

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

}
