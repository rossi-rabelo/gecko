using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    public Transform firePoint;
    public GameObject bulletPrefab;
    public GameObject chargedBulletPrefab;

    public float bulletCooldown = .5f;
    private float cooldownCount = 0f;

    public float chargeTime = 2f;
    public float chargeTimeCount = 0f;
    private bool isCharging = false;

    public bool isInCooldown = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1") && !isInCooldown)
        {
            isCharging = true;
            Shoot();
        }


        if (Input.GetButtonUp("Fire1") && !isInCooldown)
        {
            if (chargeTimeCount >= chargeTime)
            {
                ChargedShoot();
            }
            else if (!isInCooldown)
            {
                Shoot();
            }

            isCharging = false;
            chargeTimeCount = 0f;
        }

        if (isCharging)
        {
            chargeTimeCount += Time.deltaTime;
        }


        if (isInCooldown)
        {
            cooldownCount += Time.deltaTime;

            if (cooldownCount >= bulletCooldown)
            {
                isInCooldown = false;
                cooldownCount = 0f;
            }
        }
    }

    void Shoot ()
    {
        isInCooldown = true;
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }

    void ChargedShoot ()
    {
        isInCooldown = true;
        Instantiate(chargedBulletPrefab, firePoint.position, firePoint.rotation);
    }

}
