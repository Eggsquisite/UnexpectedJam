using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Components")]
    private Animator anim;

    [Header("Stats")]
    public int maxHealth = 50;
    private int currentHealth;

    [Header("SFX")]
    public AudioClip hurtSFX;

    // Start is called before the first frame update
    void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        Debug.Log(name + currentHealth);
        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        //anim.SetTrigger("die");
        Debug.Log("Dying");
    }
}
