using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class Rewind : MonoBehaviour
{
    /*----------------field------------------*/
    //在这个脚本中实现时间回溯
    public ArrayList forwardData;//正向的Data
    public ArrayList rewindData;//逆向Data  由于正向角色既需要正向读也需要反向读，因此改为ArrayList类存储数据
    public bool isRewinding = false;//是否在回溯中

    GameObject rewindCharacter;//创建一个逆向时间中的新的角色
    GameObject followCamera;//跟踪摄像机

    Color forwardColor = new Color(220f/255f,20f/255f,60f/255f,0.3f);
    Color rewindColor = new Color(65f/255f,105f/255f,225f/255f,0.3f);
    Color forwardColor_noalpha = new Color(220f/255f,20f/255f,60f/255f,1f);
    Color rewindColor_noalpha = new Color(65f/255f,105f/255f,225f/255f,1f);
    
    //UI Text信息
    public Text information;
    public Text time;

    //private bool isLast = false;//是否已经读取到了栈尾的记录
    private bool isInstantiated = false;//是否已经被实例化了
    public bool isFinished = false;//是否所有展示已经结束
    //private bool hasRewinded = false;

    //选择正向播放还是逆向播放。按照逻辑，应该是角色逆转后逆向播放正向数据，最后正向播放正向数据，逆向播放逆向数据。
    public bool isForwardDisplay = false;//是否要正向播放
    public bool isRewindDisplay = false;//是否要逆向播放

    //在rewind脚本中，随着unity每次调用FixedUpdate(),就增加1.
    //UI的时间线由currentFrame来确定。选定currentFrame->读取stage->获取状态并展示
    public int currentFrame;//当前帧数

    private AudioSource rewindAudio;//音频
    GameObject[] bgpics;//所有背景图片

    SpriteRenderer spriteRenderer;
    ObjectStage stage;
    Animator anim;
    Rigidbody2D rb;
    MovementController mc;

    /*----------------field------------------*/


    // Start is called before the first frame update
    void Start()
    {
        //初始化需要绑定的组件，将其绑定到正向角色上
        spriteRenderer = GetComponent<SpriteRenderer>();
        //stage = new ObjectStage();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        mc = GetComponent<MovementController>();

        followCamera = GameObject.FindWithTag("FollowCamera");
        bgpics = GameObject.FindGameObjectsWithTag("BackgroundPic");
        if(followCamera){
            Debug.Log("初始化Camera成功！");
        }
        information.text = "";

        //绑定RewindAudio
        rewindAudio = GetComponent<AudioSource>();

        //初始化currentFrame为0
        currentFrame = 0;

        //资源加载 reverse_character
        rewindCharacter = (GameObject)Resources.Load("Prefabs/reverse_character");
        if(rewindCharacter == null){
            Debug.Log("加载失败！");
        }
        
        forwardData = new ArrayList();
        rewindData = new ArrayList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate(){
        if(isRewindDisplay && !isForwardDisplay)//正在逆向时间中活动
        {
            /*-------------逆向展示-----------------------*/
            //逆向展示，当前帧号每次自减1
            currentFrame--;

            stage = LoadData();//获取数据
            if(stage != null){
                ShowData(stage);//展示
            }
            if(currentFrame == forwardData.Count - 70){
                information.text = "";
            }
            if(currentFrame == 1)//已经展示完最后一帧
            {
                Debug.Log("逆向展示结束！现在回到正向时间完整展示！");
                isRewindDisplay = false;
                isRewinding = false;
                isForwardDisplay = true;
                followCamera.GetComponent<CinemachineVirtualCamera>().Follow = this.transform;
                Debug.Log("重新聚焦正向角色...");
                information.color = forwardColor_noalpha;
                information.text = "规划结束，开始执行！";
                
                foreach(GameObject bgpic in bgpics){
                bgpic.GetComponent<SpriteRenderer>().color = forwardColor;
            }

            }

        }
        if(!isRewindDisplay && isForwardDisplay) {//当所有规划都结束，这时正向展示
            /*-------------正向展示-----------------------*/
            //正向展示，这时继续当前帧号继续自增1
            currentFrame++;
            stage = LoadData();//获取数据
            if(stage != null){
                ShowData(stage);//展示
            }
            if(currentFrame == 70){
                information.text = "";
            }
            if(currentFrame == forwardData.Count)//当前帧号已经是数组的末尾
            {
                Debug.Log("正向结束！完整游戏流程已结束！");
                isRewindDisplay = false;
                isForwardDisplay = false;
                isFinished = true;
                information.color = Color.white;
                information.text = "游戏结束！";
                Restore();
            }
            
        }
        if(!isRewindDisplay && !isForwardDisplay && !isFinished){//没有在展示，存储数据
            /*-------------存储数据-----------------------*/
            //Debug.Log("存储数据");
            //当前帧号自增1
            currentFrame++;
            //调用SaveData存储数据
            //每1个帧号存储一次，根据计算，每秒约有50帧
            Debug.Log("存储数据，帧号：" + currentFrame + "/每帧间隔："+ Time.deltaTime);
            SaveData(); 
        }

        //通过currentFrame计算出时间
        float currentTime = currentFrame/50f;
        time.text = "currentTime: "+currentTime.ToString();

    }


    //存储Rewind数据
    public void SaveData()
    {
        ObjectStage Stage = new ObjectStage();
        Stage.position = transform.position;//位置
        Stage.sprite = spriteRenderer.sprite;//精灵
        Stage.faceDirection = mc.faceDirection;//朝向
        Stage.velocity = rb.velocity;//速度
        Stage.currentFrame = currentFrame;//当前帧号
        
        //Debug.Log("Current faceDirection:" + Stage.faceDirection);
        //将当前记录添加到末尾
        forwardData.Add(Stage);
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
        return (ObjectStage)forwardData[currentFrame-1];
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
        anim.enabled = true;
        rb.simulated = true;
        Debug.Log("回溯完成，恢复中...");
        foreach(GameObject bgpic in bgpics){
                bgpic.GetComponent<SpriteRenderer>().color = Color.white;
            }

        //终于搞清楚人物会消失的原因了 scale改变后它的x向量并没有恢复，因此在这里恢复
        //直接恢复为(1,1,1),因为人物一开始就是(1,1,1)
        transform.localScale = new Vector3(1, 1, 1);

        //spriteRenderer.color = forwardColor;

        //isRewinding = false;
    }

    void InstantiatePrefab(){
        //若从来没有被实例化 则实例化逆向时间线对象
        rewindCharacter = Instantiate(rewindCharacter);
        rewindCharacter.name = "reversedCharacter";
        rewindCharacter.transform.position = this.transform.position;
        if(rewindCharacter){
            Debug.Log("逆转角色创建成功！");
        }
        
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if(collision.tag == "ReverseGate" && !isInstantiated){//判断是否与逆转门发生碰撞,并且只能触发一次
            //进入逆转时间，开始逆向播放正向数据。并且这时控制权交到了新创建的逆向角色身上。
            isRewindDisplay = true;
            isRewinding = true;
            Debug.Log("进入逆转门，切换人物控制，并逆向展示正向数据...");

            //播放音效
            rewindAudio.Play();
            //展示文字
            information.color = rewindColor_noalpha;
            information.text = "进入逆转时空！";
            //更换背景颜色
            foreach(GameObject bgpic in bgpics){
                bgpic.GetComponent<SpriteRenderer>().color = rewindColor;
            }
            //创建逆向角色
            if(!isInstantiated){
                isInstantiated = true;
                InstantiatePrefab();
            }
            //将相机切换到逆向角色身上
            followCamera.GetComponent<CinemachineVirtualCamera>().Follow = rewindCharacter.transform;
            Debug.Log("跟踪逆转角色...");
        }
    }
}
