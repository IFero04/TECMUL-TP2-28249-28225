using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class scr_PlayerHealth : MonoBehaviour
{
    private float health;
    private float lerpTimer;
    private float maxHealth = 100;
    public float chipSpeed = 2f;
    public Image frontHB;
    public Image backHB;
    public GameObject hurtEffect;
    public scr_MenuHandler menu;

    void Start()
    {
        health = maxHealth;   
    }

    // Update is called once per frame
    void Update()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthUi();
    }

    public void UpdateHealthUi()
    {
        float fillF = frontHB.fillAmount;
        float fillB = backHB.fillAmount;
        float hFraction = health / maxHealth;
        if(fillB > hFraction)
        {
            frontHB.fillAmount = hFraction;
            backHB.color = Color.red;
            lerpTimer += Time.deltaTime;
            float percentComplete = lerpTimer / chipSpeed;
            percentComplete = percentComplete * percentComplete;
            backHB.fillAmount = Mathf.Lerp(fillB, hFraction, percentComplete);
        }
        if (fillF < hFraction)
        {
            backHB.color = Color.green;
            backHB.fillAmount = hFraction;
            lerpTimer += Time.deltaTime;
            float percentComplete = lerpTimer / chipSpeed;
            percentComplete = percentComplete * percentComplete;
            frontHB.fillAmount = Mathf.Lerp(fillF, backHB.fillAmount, percentComplete);
        }
    }

    public void CallTakeDamage(float damage)
    {
        StartCoroutine(TakeDamage(damage));
    }

    IEnumerator TakeDamage(float damage)
    {
        yield return new WaitForSeconds(1);
        health -= damage;
        hurtEffect.SetActive(true);
        lerpTimer = 0;
        if (health <= 0)
        {
            Die();
        }
        yield return new WaitForSeconds(1);
        hurtEffect.SetActive(false);
    }

    public void RestoreHealh(float heal)
    {
        if (health < 100)
        {
            if (health + heal > 100)
            {
                health = 100;
            }
            health += heal;
        }
        lerpTimer = 0;
    }

    public void Die()
    {
        menu.GameOver();
    }
}
