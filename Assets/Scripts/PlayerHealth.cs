using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class PlayerHealth : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public int maxHealth = 100;
    public int currentHealth;

    public float invincibleTime = .5f;
    public float invincibilityFrames = 0f;

    public float knockbackForce = 10f;

    public bool isInvincible = false;
    public bool isKnockback = false;


    public HealthBarScript healthBarScript;
    public CharacterController2D controller;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        healthBarScript.SetMaxHealth(maxHealth);
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

        if (currentHealth <= 0)
        {
            // Die();
        }

        if (isInvincible)
        {
            Blink();
            invincibilityFrames += Time.deltaTime;

            if (invincibilityFrames >= invincibleTime)
            {
                spriteRenderer.enabled = true;
                isInvincible = false;
                invincibilityFrames = 0f;
            }
        }

    }

    private void FixedUpdate()
    {
        if (isKnockback)
        {
            controller.DoKnockback(knockbackForce);
            isKnockback = false;
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    public void TakeDamage(int damage)
    {
        if (!isInvincible)
        {
            currentHealth -= damage;
            CameraShaker.Instance.ShakeOnce(4f, 4f, .1f, .1f);

            healthBarScript.SetHealth(currentHealth);

            isKnockback = true;
            isInvincible = true;
        }
    }

    void Blink()
    {
        spriteRenderer.enabled = !spriteRenderer.enabled;
    }

}
