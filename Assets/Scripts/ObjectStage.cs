using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectStage
{
    //定义一个类ObjectStage用来放置每个时间点的记录
    public Vector3 position { get; set; }//位置
    public Vector3 velocity { get; set; }//速度
    public Sprite sprite { get; set; }//精灵
    public float faceDirection { get; set; }//朝向
    public int currentFrame {get; set;}//尝试用帧而不是用时间戳来记录
}
