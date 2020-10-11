using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rewind : MonoBehaviour
{
    //在这个脚本中实现时间回溯
    public Stack RewindData;//用FILO的栈来存储值
    public bool isRewinding = false;//是否在回溯中

    GameObject rewindCharacter;//创建一个新的角色 回溯到过去

    //Color forwardColor = Color.red;
    //Color rewindColor = Color.blue;

    private bool isLast = false;//是否已经读取到了栈尾的记录
    private bool isInstantiated = false;//是否已经被实例化了

    SpriteRenderer spriteRenderer;
    ObjectStage stage;
    Animator anim;
    Rigidbody2D rb;
    MovementController mc;


    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        stage = new ObjectStage();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        mc = GetComponent<MovementController>();

        rewindCharacter = (GameObject)Resources.Load("Prefabs/reverse_character");//资源加载
        if(rewindCharacter == null){
            Debug.Log("加载失败！");
        }
        RewindData = new Stack();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Q) && isRewinding==false)
        {  
            isRewinding = true;
            if(!isInstantiated){
                isInstantiated = true;
                InstantiatePrefab();
            }
            //spriteRenderer.color = rewindColor;
        }
    }

    void FixedUpdate(){
        if(isRewinding)//正在倒流中
        {
            stage = LoadData();//获取数据
            if(stage != null){
                ShowData(stage);//展示
            }
            if(isLast)//已经展示完了最后一个
            {
                isRewinding = false;
                isLast = false;
                Restore();
            }
        }
        else{
            //Debug.Log("存储数据");
            SaveData();
        }
    }


    //存储Rewind数据
    public void SaveData()
    {
        ObjectStage Stage = new ObjectStage();
        Stage.position = transform.position;//位置
        Stage.sprite = spriteRenderer.sprite;//精灵
        Stage.faceDirection = mc.faceDirection;//朝向
        Stage.velocity = rb.velocity;//速度
        //Debug.Log(Stage.sprite);
        RewindData.Push(Stage);
    }

    //读取数据
    public ObjectStage LoadData()
    {
        if (RewindData.Count <= 0)//没有记录不允许倒流
        {
            return null;
        }
        if (RewindData.Count > 1)//没到栈尾时直接pop
        {
            return (ObjectStage)RewindData.Pop();
        }
        else//栈尾的数据即第一个数据不能消除
        {
            isLast = true;
            return (ObjectStage)RewindData.Peek();
        }
    }

    //展示回溯
    void ShowData(ObjectStage currentStage)
    {
        //Debug.Log("展示回溯");
        rb.simulated = false;//关闭物理引擎
        anim.enabled = false;//停止动画播放

        rewindCharacter.transform.position = currentStage.position;
        spriteRenderer.sprite = currentStage.sprite;
        rewindCharacter.transform.localScale = new Vector3(currentStage.faceDirection, 1, 1);
        rb.velocity = currentStage.velocity;
    }

    //恢复正常时间线
    public void Restore()
    {
        //再绑定回去
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        mc = GetComponent<MovementController>();

        mc.faceDirection = stage.faceDirection;
        anim.enabled = true;
        rb.simulated = true;

        //spriteRenderer.color = forwardColor;

        isRewinding = false;
    }

    void InstantiatePrefab(){
        //若从来没有被实例化 则实例化逆向时间线对象
        rewindCharacter = Instantiate(rewindCharacter);
        rewindCharacter.name = "reversedCharacter";
        rewindCharacter.transform.position = rb.transform.position;

        //将回溯所绑定的元素绑定到reverse_character上
        spriteRenderer = rewindCharacter.GetComponent<SpriteRenderer>();
        anim = rewindCharacter.GetComponent<Animator>();
        rb = rewindCharacter.GetComponent<Rigidbody2D>();
        mc = rewindCharacter.GetComponent<MovementController>();
        
    }
}
