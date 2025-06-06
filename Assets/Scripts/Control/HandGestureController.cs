using System.Collections;
using System.Collections.Generic;
using Mediapipe.Tasks.Vision.HandLandmarker;
using UnityEngine;
using Debug = UnityEngine.Debug;
using GestureControl.Detector;

namespace GestureControl.Controller
{
    /// <summary>
    /// 手势控制配置参数
    /// </summary>
    public static class Config
    {
        public static float MoveSpeed = 15.0f;       // 基础移动速度
        public static float DepthMoveSpeed = 20.0f;  // 前后移动速度
        public static float SmoothSpeed = 0.1f;       // 移动平滑系数
        public static float MovementThreshold = 0.0005f; // 移动检测阈值
    }

    /// <summary>
    /// 手势控制器 - 负责将手势识别结果转换为游戏对象控制
    /// </summary>
    public class HandGestureController : MonoBehaviour
    {
        [SerializeField] private GameObject targetObject;  // 要控制的目标对象
        [SerializeField] private ExitGameMenu exitGameMenu; // 退出菜单引用
        [SerializeField] private Camera mainCamera;         // 主摄像机
        [SerializeField] private HandGestureDetector handGestureDetector; // 手势检测器

        private Vector3 targetPosition;       // 目标位置(平滑移动用)
        private Vector3 previousPalmPos;     // 上一帧手掌位置
        private float previousPalmSize;      // 上一帧手掌大小
        private bool isFirstFrame = true;    // 是否是第一帧标志

        private void Start()
        {
            // 参数校验
            if (targetObject == null)
            {
                Debug.LogError("请设置目标物体！");
                enabled = false;
                return;
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    Debug.LogError("未找到主相机！");
                    enabled = false;
                    return;
                }
            }

            if (handGestureDetector == null)
            {
                Debug.LogError("请设置手势检测器！");
                enabled = false;
                return;
            }

            // 注册手势检测回调
            handGestureDetector.OnHandGestureDetected += ProcessHandLandmarks;
        }

        /// <summary>
        /// 处理手势识别结果
        /// </summary>
        /// <param name="result">手势识别结果</param>
        private void ProcessHandLandmarks(HandLandmarkerResult result)
        {
            // 使用Config中的配置参数控制目标对象
            // 例如: Config.MoveSpeed, Config.DepthMoveSpeed等
        }

        private void OnDestroy()
        {
            // 注销回调防止内存泄漏
            if (handGestureDetector != null)
                handGestureDetector.OnHandGestureDetected -= ProcessHandLandmarks;
        }
    }
}
