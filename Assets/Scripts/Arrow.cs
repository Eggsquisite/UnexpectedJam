using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Stats")]
    public float speed = 5f;
    public int damage = 10;
    
    Animator anim;
    bool facingRight = true;
    bool hit = false;

    // Start is called before the first frame update
    void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
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
        if (!hit)
        {
            // Hit wall/background
            hit = true;
            anim.SetTrigger("hit");

            if (coll.tag == "Enemy")
            {
                Debug.Log("hit: " + coll.name);
                transform.SetParent(coll.transform);
                coll.transform.GetComponent<Enemy>().TakeDamage(damage);
            }
        }
    }
}
