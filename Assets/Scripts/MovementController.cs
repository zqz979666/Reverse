﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//控制正向角色的MovementController，使用Rewind中的bool来控制操控哪一个
public class MovementController : MonoBehaviour
{

    public Rigidbody2D rb;
    public float speed = 10f;
    public float jumpForce = 10f;
    public Animator anim;
    public LayerMask ground;
    public Collider2D col;
    public float faceDirection = 1f;

    Rewind r;
    // Start is called before the first frame update
    void Start()
    {
        //获取正向角色身上绑定的Rewind脚本
        r = GetComponent<Rewind>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //只有当没有正向展示或者逆向展示的时候才能控制正向角色
        //即游戏开始到进入逆转门之前的这一段时间
        if(!r.isForwardDisplay && !r.isRewindDisplay)
        {
            Movement();
            SwitchAnim();    
        }
        
    }

    //控制移动的函数
    void Movement(){
        //获取横向输入的值：-1~1
        float horizontalMove = Input.GetAxis("Horizontal");
        //获取横向输入的值：-1、0、1
        faceDirection = Input.GetAxisRaw("Horizontal");
        
        //向左或向右移动
        if(horizontalMove != 0)
        {
            //给rigidBody一个速度
            rb.velocity = new Vector2(horizontalMove * speed * Time.deltaTime, rb.velocity.y);
            //启动动画CharacterRun
            anim.SetBool("isRunning",true);
            anim.SetBool("isIdle",false);
        }
        if(horizontalMove == 0 && !anim.GetBool("isFalling"))
        {
            //取消动画CharacterRun
            anim.SetBool("isRunning",false);
            anim.SetBool("isIdle",true);
        }
        if(faceDirection != 0 )
        {
            //控制角色朝向
            transform.localScale = new Vector3(faceDirection,1,1);
        } else {
            faceDirection = 1;
        }


        //控制人物跳跃,给一个向上的力
        if(Input.GetButtonDown("Jump")){
            rb.velocity = new Vector2(rb.velocity.x , jumpForce * Time.deltaTime);
            anim.SetBool("isJumping",true);
        }
    }

    void SwitchAnim(){
        anim.SetBool("isIdle",false);
        if(anim.GetBool("isJumping")){
            //若y轴的速度小于0，即向下坠落时
            if(rb.velocity.y < 0){
                anim.SetBool("isJumping",false);
                anim.SetBool("isFalling",true);
                anim.SetBool("isIdle",false);
                anim.SetBool("isRunning",false);
            } 
        }
        if(anim.GetBool("isFalling")){
            if(col.IsTouchingLayers(ground)){
                anim.SetBool("isJumping",false);
                anim.SetBool("isFalling",false);
                anim.SetBool("isIdle",true);
                anim.SetBool("isRunning",false);
            }
        }



    }

}
