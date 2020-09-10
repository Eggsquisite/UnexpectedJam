using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Components")]
    private SpriteRenderer sp;
    private Animator anim;
    private Color tmp;

    [Header("Stats")]
    public float speed = 5f;
    public int damage = 10;

    [Header("Collision")]
    public Vector2 offset;
    private bool hit = false;
    

    private bool facingRight = true;

    // Start is called before the first frame update
    void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (sp == null) sp = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        facingRight = transform.rotation.y == 0 ? true : false;

        if (!hit)
            transform.position += facingRight ? (Vector3.right * speed * Time.deltaTime) : (Vector3.left * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (!hit && coll.tag == "Enemy" || coll.tag == "Ground")
        {
            // Hit wall/background
            hit = true;
            tmp = new Color(1f, 1f, 1f, 0.5f);
            sp.color = tmp;
            anim.SetTrigger("hit");

            if (coll.tag == "Enemy")
            {
                Debug.Log("hit: " + coll.name);
                var tmp = transform.position;
                transform.SetParent(coll.transform);
                transform.position = tmp;
                coll.transform.GetComponent<Enemy>().TakeDamage(damage);
            }
        }
    }
}
