using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** 
*   此脚本为控制反向角色回溯的脚本
*   反向角色一经创建就在逆向时间中，需要跟正向角色的Rewind加以区分
*/
public class Rewind_reverse : MonoBehaviour
{
    /*----------------field------------------*/
    //在这个脚本中实现时间回溯
    public ArrayList forwardData;//正向的Data
    public ArrayList rewindData;//逆向Data  
    public bool isRewinding;//是否在回溯中

    public GameObject forwardCharacter;//正向角色


    //private bool isLast = false;//是否已经读取到了栈尾的记录
    private bool isInstantiated;//是否已经被实例化了
    public bool isFinished;//是否所有展示已经结束
    //private bool hasRewinded = false;

    //选择正向播放还是逆向播放。按照逻辑，应该是角色逆转后逆向播放正向数据，最后正向播放正向数据，逆向播放逆向数据。
    public bool isForwardDisplay;//是否要正向播放
    public bool isRewindDisplay;//是否要逆向播放

    //在rewind脚本中，随着unity每次调用FixedUpdate(),就增加1.
    //UI的时间线由currentFrame来确定。选定currentFrame->读取stage->获取状态并展示
    public int currentFrame;//当前帧数

    SpriteRenderer spriteRenderer;
    ObjectStage stage;
    Animator anim;
    Rigidbody2D rb;
    MovementController_reverse mcr;
    Rewind r;

    /*----------------field------------------*/


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("逆向角色初始化中...");
        //初始化需要绑定的组件，将其绑定到逆向角色上
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        mcr = GetComponent<MovementController_reverse>();
        //获取Rewind脚本,并获得Rewind中的bool类，以获取当前游戏进程信息
        forwardCharacter = GameObject.FindWithTag("forwardCharacter");
        r = forwardCharacter.GetComponent<Rewind>();
        currentFrame = r.currentFrame;
        isRewindDisplay = r.isRewindDisplay;
        isForwardDisplay = r.isForwardDisplay;
        isRewinding = r.isRewinding;
        Debug.Log("逆向角色获取的当前帧号："+currentFrame);
        //Debug.Log("是否在逆转时间中："+isRewindDisplay);
        //Debug.Log("是否在正向时间中："+isForwardDisplay);

        forwardData = new ArrayList();
        rewindData = new ArrayList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate(){
        //更新数据
        if(isRewindDisplay && !isForwardDisplay)//当角色被创建时，一定正在逆向时间中活动
        {
            /*-------------逆向存储-----------------------*/
            //开始存储数据
            SaveData();
            currentFrame--;
            if(currentFrame == 0)//已经储存完最后一帧
            {
                Debug.Log("逆向存储结束！现在正向逆向一起回到正向时间完整展示！");
                isRewindDisplay = false;
                isRewinding = false;
                isForwardDisplay = true;
                currentFrame = rewindData.Count;
            }
        }
        if(!isRewindDisplay && isForwardDisplay && !isFinished) {//当所有规划都结束，这时正向展示
            /*-------------逆向展示-----------------------*/
            currentFrame--;
            stage = LoadData();//获取数据
            if(stage != null){
                ShowData(stage);//展示
            }
            if(currentFrame == 0)//当前帧号已经是数组的末尾
            {
                Debug.Log("逆向结束！完整游戏流程已结束！");
                isRewindDisplay = false;
                isForwardDisplay = false;
                isFinished = true;
                Restore();
            }
            
        }
    }


    //存储Rewind数据
    public void SaveData()
    {
        ObjectStage Stage = new ObjectStage();
        Stage.position = transform.position;//位置
        Stage.sprite = spriteRenderer.sprite;//精灵
        Stage.faceDirection = mcr.faceDirection;//朝向
        Stage.velocity = rb.velocity;//速度
        Stage.currentFrame = currentFrame;//当前帧号
        
        Debug.Log("逆向角色存储数据，帧号：" + currentFrame);
        //将当前记录添加到末尾
        rewindData.Add(Stage);
    }

    //读取数据
    public ObjectStage LoadData()
    {
        // if (RewindData.Count <= 0)//没有记录不允许倒流
        // {
        //     return null;
        // }
        // if (RewindData.Count > 1)//没到栈尾时直接pop
        // {
        //     return (ObjectStage)RewindData.Pop();
        // }
        // else//栈尾的数据即第一个数据不能消除
        // {
        //     isLast = true;
        //     return (ObjectStage)RewindData.Peek();
        // }
        Debug.Log("加载rewindData，帧号：" + currentFrame);
        return (ObjectStage)rewindData[currentFrame];
    }

    //展示回溯
    void ShowData(ObjectStage currentStage)
    {
        //Debug.Log("展示回溯");
        rb.simulated = false;//关闭物理引擎
        anim.enabled = false;//停止动画播放

        this.transform.position = currentStage.position;
        spriteRenderer.sprite = currentStage.sprite;
        this.transform.localScale = new Vector3(currentStage.faceDirection, 1, 1);
        rb.velocity = currentStage.velocity;
    }

    //恢复正常时间线
    public void Restore()
    {
        //mc.faceDirection = stage.faceDirection;
        rb.velocity = new Vector2(0,0);
        anim.SetBool("isJumping",false);
        anim.SetBool("isFalling",false);
        anim.SetBool("isIdle",true);
        anim.SetBool("isRunning",false);
        anim.enabled = true;
        rb.simulated = true;
        Debug.Log("回溯完成，逆向角色恢复中...");

        //终于搞清楚人物会消失的原因了 scale改变后它的x向量并没有恢复，因此在这里恢复
        //直接恢复为(1,1,1),因为人物一开始就是(1,1,1)
        transform.localScale = new Vector3(1, 1, 1);

        //spriteRenderer.color = forwardColor;

        //isRewinding = false;
    }

    // private void OnTriggerEnter2D(Collider2D collision){
    //     if(collision.tag == "ReverseGate" && !isInstantiated){//判断是否与逆转门发生碰撞,并且只能触发一次
    //         //进入逆转时间，开始逆向播放正向数据。并且这时控制权交到了新创建的逆向角色身上。
    //         isRewindDisplay = true;
    //         isRewinding = true;
    //         Debug.Log("进入逆转门，切换人物控制，并逆向展示正向数据...");
    //         //创建逆向角色
    //     }
    // }
}