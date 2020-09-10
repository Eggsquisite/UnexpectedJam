using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{

    [SerializeField] float speed = 5f;
    
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
        hit = true;
        anim.SetTrigger("hit");
        Debug.Log("hit: " + coll.name);

        if (coll.tag == "Enemy")
        {
            transform.parent = coll.transform;
        }
    }
}
