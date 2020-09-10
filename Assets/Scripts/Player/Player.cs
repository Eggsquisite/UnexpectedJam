using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Horizontal Movement")]
    [SerializeField] float moveSpeed = 15f;
    private Vector2 movement;
    private bool facingRight = true;

    [Header("Vertical Movement")]
    [SerializeField] float jumpDelay = 0.25f;
    [SerializeField] float jumpForce = 15f;
    private float jumpTimer;

    [Header("Components")]
    [SerializeField] Animator anim;
    [SerializeField] LayerMask groundLayer;
    private Rigidbody2D rb;

    [Header("Physics")]
    [SerializeField] float maxSpeed = 7f;
    [SerializeField] float linearDrag = 4f;
    [SerializeField] float gravity = 1f;
    [SerializeField] float fallMultiplier = 5f;
    private float baseMaxSpeed;

    [Header("Collision")]
    [SerializeField] float groundLength = 0.5f;
    [SerializeField] Vector3 colliderOffset;
     public bool onGround = false;

    [Header("Player Stats")]
    public int maxHealth = 50;
    private int currentHealth;

    [Header("Combat")]
    public GameObject arrow;
    public Transform attackPoint;
    public LayerMask enemyLayers;
    public bool archer = false;
    public bool soldier = false;
    public float attackCD = 1f;
    public float attackRange = 0.25f;
    public int lightDmg = 10;
    public int heavyDmg = 25;

    private int attackCombo = 0;
    private float attackTimer = 0f;
    private bool aiming = false;
    private bool attacking = false;
    private bool attackReady = true;

    // Start is called before the first frame update
    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        baseMaxSpeed = maxSpeed;
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        onGround = Physics2D.Raycast(transform.position + colliderOffset, Vector2.down, groundLength, groundLayer) 
            || Physics2D.Raycast(transform.position - colliderOffset, Vector2.down, groundLength, groundLayer);

        // Jump delay before hitting the ground
        if (Input.GetButtonDown("Jump")) jumpTimer = Time.time + jumpDelay;

        // Melee attack for both characters
        if (onGround) MeleeAttack();

        // Attack for archer
        if (archer && attackReady) ArcherAttack();

        // Cooldown for attacks
        if (!attackReady) AttackCooldown();

        // Stop movement when attacking/aiming
        if (attacking || aiming) movement = new Vector2(0, 0);
        else if (!attacking || !aiming) movement = new Vector2(Input.GetAxisRaw("Horizontal"), 0);

        // Disallow character flipping when attacking (does not apply to archer shooting)
        if (!attacking)
            if ((movement.x > 0 && !facingRight) || (movement.x < 0 && facingRight))
                Flip();
    }

    private void FixedUpdate()
    {
        // Character movement with direction
        Movement(movement.x);

        if (jumpTimer > Time.time && onGround && !attacking)
            Jump();

        ModifyPhysics();
    }

    void Movement(float direction)
    {   
        rb.AddForce(Vector2.right * direction * moveSpeed);

        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);

        anim.SetFloat("horizontal", Mathf.Abs(movement.x));
        anim.SetFloat("vertical", Mathf.Sign(rb.velocity.y));
    }

    void Jump()
    {
        rb.drag = 0;
        jumpTimer = 0f;
        anim.SetTrigger("jump");
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
        //transform.localScale = new Vector3(facingRight ? 1 : -1, 1, 1);
    }

    void ModifyPhysics()
    {
        bool changingDirections = (movement.x > 0 && rb.velocity.x < 0) || (movement.x < 0 && rb.velocity.x > 0);

        if (onGround)
        {
            anim.SetBool("falling", false);
            anim.SetBool("land", true);
            if (Mathf.Abs(movement.x) < 0.4f || changingDirections)
                rb.drag = linearDrag;
            else
                rb.drag = 0;

            rb.gravityScale = 0;
        }
        else
        {
            anim.SetBool("falling", true);
            anim.SetBool("land", false);
            rb.gravityScale = gravity;
            rb.drag = linearDrag * 0.15f;
            if (rb.velocity.y < 0)
                rb.gravityScale = gravity * fallMultiplier;
            else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
                rb.gravityScale = gravity * (fallMultiplier / 2);
        }
    }

    ///
    /// HEALTH MANAGEMENT ///
    /// 

    public void TakeDamage(int damage)
    {
        // launch the player in the direction
        currentHealth -= damage;
        // player hurt sound

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        anim.SetTrigger("death");
        // Destroy gameObject 
    }

    /// <summary>
    //  COMBAT 
    /// </summary>
    /// 

    private void MeleeAttack()
    {
        if (Input.GetMouseButtonDown(0) && attackCombo == 0)
            MeleeOne();
        else if (Input.GetMouseButtonDown(0) && attackCombo == 1)
            MeleeTwo();
    }


    private void MeleeOne()
    {
        attacking = true;
        anim.SetBool("meleeOne", true);
    }

    private void MeleeTwo()
    {
        attackReady = false;
        anim.SetBool("meleeOne", false);
        anim.SetBool("meleeTwo", true);
    }

    private void LightAttack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
            enemy.GetComponent<Enemy>().TakeDamage(lightDmg);
    }

    private void HeavyAttack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
            enemy.GetComponent<Enemy>().TakeDamage(heavyDmg);
    }

    private void SetAttackCombo(int attackNum)
    {
        attackCombo = attackNum;
    }

    private void ResetAttack()
    {
        aiming = false;
        attackCombo = 0;
        attacking = false;
        anim.SetBool("meleeOne", false);
        anim.SetBool("meleeTwo", false);
    }

    // ARCHER //

    private void ArcherAttack()
    {
        if (Input.GetMouseButtonDown(1) && !aiming)
            Aim();
        //else if (Input.GetMouseButtonUp(1) && aiming)
            //Fire();
    }

    private void Aim()
    {
        aiming = true;
        anim.Play("drawback");
        anim.SetTrigger("aim");
    }

    private void Fire()
    {
        aiming = false;
        anim.ResetTrigger("aim");
        Instantiate(arrow, attackPoint.position, transform.rotation);
    }

    private void AttackCooldown()
    {
        if (attackTimer < attackCD)
            attackTimer += Time.deltaTime;
        else if (attackTimer >= attackCD)
        {
            attackTimer = 0f;
            attackReady = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + colliderOffset, transform.position + Vector3.down * groundLength);
        Gizmos.DrawLine(transform.position - colliderOffset, transform.position + Vector3.down * groundLength);

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
