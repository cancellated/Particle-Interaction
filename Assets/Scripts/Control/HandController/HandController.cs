using System.Collections;
using System.Collections.Generic;
using Mediapipe.Tasks.Vision.HandLandmarker;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Debug = UnityEngine.Debug;
using GestureControl.Detector;
using GestureControl.Data;

namespace GestureControl.Controller
{
    /// <summary>
    /// 手势控制器 - 负责将手势识别结果转换为控制指令
    /// </summary>
    public class HandController : MonoBehaviour
    {
        [SerializeField] private GameObject targetObject;  // 要控制的目标对象
        [SerializeField] private ExitGameMenu exitGameMenu; // 退出菜单引用
        [SerializeField] private Camera mainCamera;         // 主摄像机
        [SerializeField] private HandDetector handGestureDetector; // 手势检测器

        private GestureType currentGesture;
        private InputActionAsset gestureActions;

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
            // 初始化输入系统
            gestureActions = Resources.Load<InputActionAsset>("GestureControls");
            gestureActions.Enable();
            
            // 绑定手势事件
            GameEvents.OnHandGestureDetected += OnGestureDetected;
        }

        private void OnGestureDetected(HandData data)
        {
            // 获取当前手势类型
            var newGesture = AnalyzeGesture(data);
            
            // 如果是左手移动手势

            
            // 更新当前手势状态
            if(newGesture != currentGesture)
            {
                currentGesture = newGesture;
            }
        }

        private GestureType AnalyzeGesture(HandData data)
        {
            // 复杂手势分析逻辑
            if(data.IsLeftHandFist && data.IsRightHandFist) 
                return GestureType.DoubleFist;
            // ... 其他手势判断
            return GestureType.None;
        }
        
        private void OnDestroy()
        {
            // 注销回调防止内存泄漏
            if (handGestureDetector != null)
            {

            }

        }
    }
}
