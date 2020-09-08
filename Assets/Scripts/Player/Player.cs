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
    private float baseFallMultiplier;

    [Header("Collision")]
    [SerializeField] float groundLength = 0.5f;
    [SerializeField] Vector3 colliderOffset;
     public bool onGround = false;

    [Header("Combat")]
    [SerializeField] float attackRange = 0.25f;
    [SerializeField] float attackMoveSpeedMult = 0.25f;
    public bool archer = false;
    public bool soldier = false;

    private int attackCombo = 0;
    private bool attacking = false;
    private bool aiming = false;

    // Start is called before the first frame update
    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        baseMaxSpeed = maxSpeed;
        baseFallMultiplier = fallMultiplier;
    }

    // Update is called once per frame
    void Update()
    {
        onGround = Physics2D.Raycast(transform.position + colliderOffset, Vector2.down, groundLength, groundLayer) 
            || Physics2D.Raycast(transform.position - colliderOffset, Vector2.down, groundLength, groundLayer);

        if (Input.GetButtonDown("Jump"))
            jumpTimer = Time.time + jumpDelay;

        if (onGround)
        {
            if (Input.GetMouseButtonDown(0) && attackCombo == 0)
                MeleeOne();
            else if (Input.GetMouseButtonDown(0) && attackCombo == 1)
                MeleeTwo();
        }

        if (archer)
        {
            if (Input.GetMouseButtonDown(1) && !aiming)
                Aim();
            else if (Input.GetMouseButtonUp(1) && aiming)
                Fire();
        }

        if (attacking || aiming)
            movement = new Vector2(0, 0);
        else if (!attacking)
            movement = new Vector2(Input.GetAxisRaw("Horizontal"), transform.position.y);

        if (!attacking)
        {
            if ((movement.x > 0 && !facingRight) || (movement.x < 0 && facingRight))
                Flip();
        }
    }

    private void FixedUpdate()
    {
        Movement(movement.x);

        if (jumpTimer > Time.time && onGround && !attacking && !aiming)
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
        anim.SetTrigger("jump");
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpTimer = 0f;
    }

    void Flip()
    {
        facingRight = !facingRight;
        //transform.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
        transform.localScale = new Vector3(facingRight ? 1 : -1, 1, 1);
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

    /// <summary>
    //  COMBAT 
    /// </summary>
    /// 

    private void MeleeOne()
    {
        maxSpeed = baseMaxSpeed * attackMoveSpeedMult;
        attacking = true;
        anim.SetBool("meleeOne", true);
    }

    private void MeleeTwo()
    {
        anim.SetBool("meleeOne", false);
        anim.SetBool("meleeTwo", true);
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
        maxSpeed = baseMaxSpeed;
        anim.SetBool("meleeOne", false);
        anim.SetBool("meleeTwo", false);
        anim.SetBool("fire", false);
        anim.SetBool("aim", false);
    }

    private void Aim()
    {
        aiming = true;
        anim.Play("drawback");
        anim.SetBool("aim", true);
        movement = new Vector2(0, 0);
        fallMultiplier = baseFallMultiplier / 2;
    }

    private void Reholster()
    {
        aiming = false;
        anim.SetBool("aim", false);
    }

    private void Fire()
    {
        aiming = false;
        fallMultiplier = baseFallMultiplier;
        anim.SetBool("aim", false);
        anim.SetBool("fire", true);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + colliderOffset, transform.position + Vector3.down * groundLength);
        Gizmos.DrawLine(transform.position - colliderOffset, transform.position + Vector3.down * groundLength);
    }
}
