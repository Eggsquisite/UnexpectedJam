using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public enum PlayerClass{ Archer, Soldier};
    public PlayerClass playerClass;

    [Header("Horizontal Movement")]
    public float moveSpeed = 15f;
    public float dashSpeed = 20f;
    public float dashMaxCD = 2f;
    public DashState dashState;

    public enum DashState 
    { 
        Ready,
        Dashing,
        Cooldown
    };

    private Vector2 movement;
    private float dashTimer = 0f;
    private bool facingRight = true, dashing = false;

    [Header("Vertical Movement")]
    public float jumpDelay = 0.25f;
    public float jumpForce = 15f;
    private float jumpTimer = 0f;

    [Header("Components")]
    [SerializeField] Animator anim;
    [SerializeField] LayerMask groundLayer;
    private Rigidbody2D rb;

    [Header("Physics")]
    public float maxSpeed = 7f;
    public float linearDrag = 4f;
    public float gravity = 1f;
    public float fallMultiplier = 5f;

    [Header("Collision")]
    public float groundLength = 0.5f;
    public Vector3 colliderOffset;
    public bool onGround = false;

    [Header("Player Stats")]
    public int maxHealth = 50;
    private int currentHealth;

    [Header("Combat")]
    public GameObject arrow;
    public Transform attackPoint;
    public LayerMask enemyLayers;
    public float attackCD = 1f;
    public float attackRange = 0.25f;
    public int lightDmg = 10;
    public int heavyDmg = 25;

    private int attackCombo = 0;
    private float attackTimer = 0f;
    private bool aiming = false, attacking = false, attackReady = false, iFrames = false, dead = false;

    // Start is called before the first frame update
    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (dead)
            return;

        /// MOVEMENT INPUT ///
        
        // Stop movement when attacking/aiming
        if (attacking || aiming) 
            movement = new Vector2(0, 0);
        else if (!attacking || !aiming && !dashing) 
            movement = new Vector2(Input.GetAxisRaw("Horizontal"), 0);

        onGround = Physics2D.Raycast(transform.position + colliderOffset, Vector2.down, groundLength, groundLayer) 
            || Physics2D.Raycast(transform.position - colliderOffset, Vector2.down, groundLength, groundLayer);

        // Disallow character flipping when attacking (does not apply to archer shooting)
        if (!attacking && !dashing) 
            Flip();

        // Jump delay before hitting the ground
        if (Input.GetButtonDown("Jump") && !dashing) 
            jumpTimer = Time.time + jumpDelay;

        DashInput();

        /// COMBAT INPUT ///

        // Cooldown for attacks
        if (!attackReady) 
            AttackCooldown();

        // Melee attack for both characters
        if (onGround && !dashing) 
            MeleeCombo();

        // Attack for archer
        if (playerClass == PlayerClass.Archer && attackReady && !dashing) 
            ArcherAttack();
    }

    private void FixedUpdate()
    {
        if (dead)
            return;

        // Character movement with direction
        Movement(movement.x);

        if (dashing)
            Dashing();

        if (jumpTimer > Time.time && onGround && !attacking && !dashing)
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
        //rb.drag = 0;
        jumpTimer = 0f;
        anim.SetTrigger("jump");
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    void DashInput()
    {
        switch (dashState)
        {
            case DashState.Ready:
                var isDashKeyDown = Input.GetKeyDown(KeyCode.LeftShift);
                if (isDashKeyDown && onGround)
                {
                    anim.SetTrigger("dash");
                    anim.SetBool("dashing", true);
                    dashState = DashState.Dashing;
                }
                break;
            case DashState.Dashing:

                dashing = true;
                //Dashing();

                break;
            case DashState.Cooldown:
                dashing = false;
                anim.ResetTrigger("dash");
                anim.SetBool("dashing", false);

                if (dashTimer < dashMaxCD)
                    dashTimer += Time.deltaTime;
                else if (dashTimer >= dashMaxCD)
                {
                    dashTimer = 0f;
                    dashState = DashState.Ready;
                }
                break;
        }
    }

    void Dashing()
    {
        if (facingRight)
            rb.AddForce(Vector2.right * dashSpeed);
        else
            rb.AddForce(Vector2.left * dashSpeed);
    }

    // Called during animation
    void DashDone()
    {
        dashState = DashState.Cooldown;
    }

    void SetIframes(int status)
    {
        iFrames = status == 0 ? false : true;
    }

    void Flip()
    {
        if ((Input.GetAxisRaw("Horizontal") > 0 && !facingRight) || (Input.GetAxisRaw("Horizontal") < 0 && facingRight))
        {
            facingRight = !facingRight;
            transform.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
            //transform.localScale = new Vector3(facingRight ? 1 : -1, 1, 1);
        }
    }

    void ModifyPhysics()
    {
        bool changingDirections = (movement.x > 0 && rb.velocity.x < 0) || (movement.x < 0 && rb.velocity.x > 0);

        if (onGround)
        {
            anim.SetBool("falling", false);
            anim.SetBool("land", true);
            rb.gravityScale = 0;

            if (Mathf.Abs(movement.x) < 0.4f || changingDirections)
                rb.drag = linearDrag;
            else
                rb.drag = 0;
        }
        else
        {
            rb.gravityScale = gravity;
            rb.drag = linearDrag * 0.15f;
            anim.SetBool("land", false);
            anim.SetBool("falling", true);

            if (rb.velocity.y < 0)
                rb.gravityScale = gravity * fallMultiplier;
            else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
                rb.gravityScale = gravity * (fallMultiplier / 2);
        }
    }

    ///
    /// HEALTH MANAGEMENT ////////////////////////////////////////////////////////////////////////
    /// 

    public void TakeDamage(int damage)
    {
        if (!iFrames)
        {
            // launch the player in the direction
            currentHealth -= damage;
            // player hurt sound

            if (currentHealth <= 0)
                Die();
        }
    }

    private void Die()
    {
        dead = true;
        tag = "Dead";
        iFrames = true;
        anim.SetTrigger("death");
        rb.velocity = Vector3.zero;
        // Destroy gameObject 
    }

    ///  COMBAT ////////////////////////////////////////////////////////////////////////

    private void MeleeCombo()
    {
        if (Input.GetMouseButtonDown(0) && attackCombo == 0 && attackReady)
            MeleeOne();
        else if (Input.GetMouseButtonDown(0) && attackCombo == 1)
            MeleeTwo();
    }

    private void MeleeOne()
    {
        attacking = true;
        ResetAttackCD();
        anim.SetBool("meleeOne", true);
    }

    private void MeleeTwo()
    {
        attacking = true;
        ResetAttackCD();
        anim.SetBool("meleeOne", false);
        anim.SetBool("meleeTwo", true);
    }

    private void SetAttackCombo(int attackNum)
    {
        attackCombo = attackNum;
    }

    private void ResetAttackCD()
    {
        attackTimer = 0f;
        attackReady = false;
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

    private void AttackReset()
    {
        aiming = false;
        attackCombo = 0;
        dashing = false;
        attacking = false;
        anim.SetBool("meleeOne", false);
        anim.SetBool("meleeTwo", false);
    }

    private void FlipStatus(int status)
    {
        attacking = status == 0 ? false : true;
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

    // ARCHER //////////////////////////////////////////////////////////////////////

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
        ResetAttackCD();
        anim.ResetTrigger("aim");
        Instantiate(arrow, attackPoint.position, transform.rotation);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Gizmos.DrawLine(transform.position + colliderOffset, transform.position + colliderOffset + Vector3.down * groundLength);
        //Gizmos.DrawLine(transform.position - colliderOffset, transform.position - colliderOffset + Vector3.down * groundLength);

        //Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
