using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Animator anim;
    //[SerializeField] PlayerMovement pm;

    [Header("Combat")]
    [SerializeField] float attackRange = 0.25f;
    [SerializeField] float moveSpeedMult = 0.25f;

    private int attackCombo = 0;
    private float baseMaxSpeed;

    private void Start()
    {
        //baseMaxSpeed = pm.GetMoveSpeed();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && attackCombo == 0)
            MeleeOne();
        else if (Input.GetMouseButtonDown(0) && attackCombo == 1)
            MeleeTwo();
    }

    private void MeleeOne()
    {
        //pm.SetMoveSpeed(baseMaxSpeed * moveSpeedMult);
        anim.SetBool("meleeOne", true);
    }

    private void MeleeTwo()
    {
        //pm.SetMoveSpeed(baseMaxSpeed * moveSpeedMult);
        anim.SetBool("meleeOne", false);
        anim.SetBool("meleeTwo", true);
    }

    public void SetAttackCombo(int attackNum)
    {
        attackCombo = attackNum;
    }

    public void ResetAttack()
    {
        attackCombo = 0;
        //pm.SetMoveSpeed(baseMaxSpeed);
        anim.SetBool("meleeOne", false);
        anim.SetBool("meleeTwo", false);
    }
}
