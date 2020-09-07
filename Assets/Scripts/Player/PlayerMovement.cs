using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Horizontal Movement")]
    [SerializeField] float moveSpeed = 15f;
    Vector2 movement;
    bool facingRight = true;

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

    [Header("Collision")]
     public bool onGround = false;
    [SerializeField] float groundLength = 0.5f;
    [SerializeField] Vector3 colliderOffset;

    // Start is called before the first frame update
    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        onGround = Physics2D.Raycast(transform.position + colliderOffset, Vector2.down, groundLength, groundLayer) 
            || Physics2D.Raycast(transform.position - colliderOffset, Vector2.down, groundLength, groundLayer);

        if (Input.GetButtonDown("Jump"))
            jumpTimer = Time.time + jumpDelay;

        movement = new Vector2(Input.GetAxisRaw("Horizontal"), transform.position.y);
    }

    private void FixedUpdate()
    {
        Movement(movement.x);

        if (jumpTimer > Time.time && onGround)
            Jump();

        ModifyPhysics();
    }

    void Movement(float direction)
    {   
        rb.AddForce(Vector2.right * direction * moveSpeed);


        if ((direction > 0 && !facingRight) || (direction < 0 && facingRight))
            Flip();

        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);

        anim.SetFloat("horizontal", Mathf.Abs(rb.velocity.x));
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

    public void SetMaxSpeed(float newSpeed)
    {
        maxSpeed = newSpeed;
    }

    public float GetMaxSpeed()
    {
        return maxSpeed;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + colliderOffset, transform.position + Vector3.down * groundLength);
        Gizmos.DrawLine(transform.position - colliderOffset, transform.position + Vector3.down * groundLength);
    }
}
