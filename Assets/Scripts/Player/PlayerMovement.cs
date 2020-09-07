using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Horizontal Movement")]
    [SerializeField] float moveSpeed = 15f;
    Vector2 direction;
    bool facingRight = true;

    [Header("Components")]
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Physics")]
    [SerializeField] float maxSpeed = 7f;
    [SerializeField] float linearDrag = 4f;

    // Start is called before the first frame update
    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (anim == null) anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        direction = new Vector2(Input.GetAxisRaw("Horizontal"), transform.position.y);
    }

    private void FixedUpdate()
    {
        Movement(direction.x);
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
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
    }

    void ModifyPhysics()
    {
        bool changingDirections = (direction.x > 0 && rb.velocity.x < 0) || (direction.x < 0 && rb.velocity.x > 0);

        if (Mathf.Abs(direction.x) < 0.4f || changingDirections)
            rb.drag = linearDrag;
        else
            rb.drag = 0;
    }
}
