using UnityEngine;
using Mediapipe.Tasks.Vision.HandLandmarker;

namespace GestureControl.Data
{
    /// <summary>
    /// 手势数据类
    /// </summary>
    public class HandGestureData
    {
        public HandLandmarkerResult RawResult { get; set; } // 原始检测结果
        
        // 手势状态
        public bool IsLeftHandFist { get; set; }
        public bool IsRightHandFist { get; set; }
        
        // 手掌位置信息
        public Vector2 LeftPalmPosition { get; set; }
        public Vector2 RightPalmPosition { get; set; }
        
        // 手掌移动速度
        public Vector2 LeftPalmVelocity { get; set; }
        public Vector2 RightPalmVelocity { get; set; }
        
        // 手势持续时间
        public float DoubleFistDuration { get; set; }
        public float SingleFistDuration { get; set; }
        
        // 手势识别时间戳
        public float DetectionTime { get; set; }

        // 目标位置(平滑移动用)
        public Vector3 TargetPosition { get; set; }  

        // 上一帧手掌位置  
        public Vector3 PreviousPalmPosition { get; set; }

        // 上一帧手掌大小
        public float PreviousPalmSize { get; set; }  
        
        // 是否是第一帧标志    
        public bool IsFirstFrame { get; set; }           
    }
}

