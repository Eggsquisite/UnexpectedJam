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
    private bool pauseMovement = false, movementCD = false, facingRight = true;

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

    [Header("Collision")]
    public LayerMask groundLayer;
    public Vector3 groundOffset;
    private bool followPlayer;
    public float followRange;
    public float groundLength;
    private RaycastHit2D ground;
    private RaycastHit2D rightWall;
    private RaycastHit2D leftWall;
    private RaycastHit2D rightLedge;
    private RaycastHit2D leftLedge;

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
        if (dead)
            return;

        anim.SetFloat("direction", Mathf.Abs(direction));

        RaycastHit2D hit = Physics2D.Raycast(transform.position + offset, transform.position + Vector3.right * attackDistance, playerLayer);
        if (hit.collider != null && hit.collider.tag == "Player")
            AnimateAttack();

        if (movementCD)
            MovementCD();

        Flip();

    }

    void FixedUpdate()
    {
        if (dead)
            return;

        if (!pauseMovement)
            Movement(direction);

        rightLedge = Physics2D.Raycast(new Vector2(transform.position.x + groundOffset.x, transform.position.y), Vector2.down, groundLength);
        Debug.DrawRay(new Vector2(transform.position.x + groundOffset.x, transform.position.y), Vector2.down, Color.blue);
        if (rightLedge.collider == null) { 
            direction = -1;
            Debug.Log(rightLedge.collider);
        }

        leftLedge = Physics2D.Raycast(new Vector2(transform.position.x - groundOffset.x, transform.position.y), Vector2.down, groundLength);
        Debug.DrawRay(new Vector2(transform.position.x - groundOffset.x, transform.position.y), Vector2.down, Color.blue);
        if (leftLedge.collider == null)
            direction = 1;

        //leftLedge = Physics2D.Raycast(new Vector2(transform.position.x - groundOffset.x, transform.position.y), Vector2.down, groundLength);
        // Debug.DrawLine(new Vector2(transform.position.x - groundOffset.x, transform.position.y), Vector2.down, Color.red);
        //if (leftLedge.collider == null)
        //direction = 1;
    }

    void Flip()
    {
        if (rb.velocity.x > 0.01f && !facingRight || rb.velocity.x < -0.01f && facingRight)
        {
            facingRight = !facingRight;
            transform.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
            //transform.localScale = new Vector3(facingRight ? 1 : -1, 1, 1);
        }
    }

    // MOVEMENT ////////////////////////////////////////////////////////////////////

    void Movement(float direction)
    {
        if (direction > 0)
            rb.AddForce(Vector2.right * moveSpeed);
        else if (direction < 0)
            rb.AddForce(Vector2.left * moveSpeed);

        if (Mathf.Abs(rb.velocity.x) > maxSpeed) { 
            rb.velocity = new Vector2(Mathf.Sign(direction) * maxSpeed, rb.velocity.y);
        } 
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

    void EdgeChecks()
    { 
    
    }

    // COLLISIONS AND RAYCASTING ///////////////////////////////////////////////////



    // COMBAT //////////////////////////////////////////////////////////////////////

    private void AnimateAttack()
    {
        pauseMovement = true;
        rb.velocity = Vector2.zero;
        anim.SetTrigger("attack");
    }

    private void Attack()
    {
            pauseMovement = true;
            rb.velocity = Vector2.zero;
            Collider2D[] playersHit = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);

            foreach (Collider2D player in playersHit)
                player.GetComponent<Player>().TakeDamage(damage);
    }

    private void Shake()
    {
        hit = true;
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
        rb.velocity = Vector3.zero; 

        foreach (Transform child in transform)
            GameObject.Destroy(child.gameObject);
    }

    // DEBUG //////////////////////////////////////////////////////////////////////

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        Gizmos.DrawLine(transform.position + offset, transform.position + Vector3.right * attackDistance);

        //Gizmos.DrawLine(transform.position + groundOffset, transform.position + groundOffset + Vector3.down * groundLength);
        //Gizmos.DrawLine(transform.position - groundOffset, transform.position - groundOffset + Vector3.down * groundLength);

    }
}
