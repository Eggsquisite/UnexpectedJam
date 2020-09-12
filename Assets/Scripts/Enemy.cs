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

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float maxSpeed = 20f;
    public float direction = 0f;
    public float pauseCooldown = 2f;
    private float pauseTimer = 0f;
    private bool pauseMovement = false, movementCD = false;

    [Header("Combat")]
    public LayerMask playerLayer;
    public Vector3 offset;
    public float attackRange = 0.5f;
    public float attackDistance = 1f;
    public int damage = 10;
    public float shakeMagnitude = 0.015f, shakeDuration = 0.5f;
    public bool dead = false;

    private Vector3 initPos;
    private bool hit = false;

    [Header("SFX")]
    public AudioClip hurtSFX;
    public AudioClip attackSFX;


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
        anim.SetFloat("direction", Mathf.Abs(direction));

        RaycastHit2D hit = Physics2D.Raycast(transform.position + offset, transform.position + Vector3.right * attackDistance, playerLayer);
        if (hit.collider != null && hit.collider.tag == "Player")
            AnimateAttack();

        if (movementCD)
            MovementCD();
    }

    void FixedUpdate()
    {
        if (!pauseMovement)
            Movement(direction);
    }

    // MOVEMENT ////////////////////////////////////////////////////////////////////

    void Movement(float direction)
    {
        if (direction > 0)
            rb.AddForce(Vector2.right * moveSpeed);
        else if (direction < 0)
            rb.AddForce(Vector2.left * moveSpeed);

        if (rb.velocity.x > maxSpeed)
            rb.velocity = new Vector2(Mathf.Sign(direction) * maxSpeed, rb.velocity.y);
    }

    void MovementCD()
    {
        if (pauseTimer < pauseCooldown)
            pauseTimer += Time.deltaTime;
        else if (pauseTimer >= pauseCooldown)
        {
            pauseTimer = 0f;
            movementCD = false;
        }
    }

    void ResumingMovement()
    {
        movementCD = true;
        pauseMovement = false;
    }

    // COMBAT //////////////////////////////////////////////////////////////////////

    private void AnimateAttack()
    {
        pauseMovement = true;
        rb.velocity = Vector2.zero;
        anim.SetTrigger("attack");
    }

    private void Attack()
    {
        if (!movementCD)
        {
            pauseMovement = true;
            rb.velocity = Vector2.zero;
            Collider2D[] playersHit = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);

            foreach (Collider2D player in playersHit)
                player.GetComponent<Player>().TakeDamage(damage);
        }
        else
            return;
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
        //Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        Gizmos.DrawLine(transform.position + offset, transform.position + Vector3.right * attackDistance);
    }
}
