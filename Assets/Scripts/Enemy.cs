using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Components")]
    public Transform attackPoint;
    private Animator anim;
    private Collider2D coll;
    private Rigidbody2D rb;

    [Header("Stats")]
    public int maxHealth = 50;
    private int currentHealth;

    [Header("Combat")]
    public LayerMask playerLayer;
    public float attackRange = 0.5f;
    public int damage = 10;
    public float shakeMagnitude = 0.015f, shakeDuration = 0.5f;
    public bool dead = false;

    private Vector3 initPos;
    private bool hit = false;

    [Header("SFX")]
    public AudioClip hurtSFX;

    [Header("Movement")]
    public float moveSpeed = 5f;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (anim == null) anim = GetComponent<Animator>();
        if (coll == null) coll = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // COMBAT //////////////////////////////////////////////////////////////////////

    private void Attack()
    {
        Collider2D[] playersHit = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);

        foreach(Collider2D player in playersHit)
            player.GetComponent<Player>().TakeDamage(damage);
    }

    private void Shake()
    {
        initPos = transform.position;
        InvokeRepeating("StartShake", 0f, 0.005f);
        Invoke("StopShake", shakeDuration);
    }

    private void StartShake()
    {
        float shakingOffsetX = Random.value * shakeMagnitude * 2 - shakeMagnitude;
        float shakingOffsetY = Random.value * shakeMagnitude * 2 - shakeMagnitude;
        Vector3 intermedPos = transform.position;

        intermedPos.x += shakingOffsetX;
        intermedPos.y += shakingOffsetY;
        transform.position = intermedPos;
    }

    private void StopShake()
    {
        hit = false;
        CancelInvoke("StartShake");
        transform.position = initPos;
    }

    // HEALTH //////////////////////////////////////////////////////////////////////

    public void TakeDamage(int dmg)
    {
        if (!dead)
        {
            hit = true;
            currentHealth -= dmg;
            if (currentHealth <= 0)
                Die();
            else
            {
                Debug.Log(name + currentHealth);
                Shake();
            }
        }
    }

    private void Die()
    {
        dead = true;
        tag = "Dead";
        anim.SetTrigger("die");

        foreach (Transform child in transform)
            GameObject.Destroy(child.gameObject);
    }

    // DEBUG //////////////////////////////////////////////////////////////////////

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
