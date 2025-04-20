using System.Collections.Generic;
using Mediapipe.Tasks.Vision.HandLandmarker;
using UnityEngine;

public class HandGestureController : MonoBehaviour
{
    [Header("MediaPipe设置")]
    public HandLandmarkerRunner handLandmarkerRunner; // 场景中的HandLandmarker组件

    [Header("控制设置")]
    public GameObject targetObject; // 要控制的3D物体
    public float moveSpeed = 5f; // 移动速度
    public float gestureThreshold = 0.1f; // 手势判定阈值

    private Vector3 previousPalmPosition;
    private bool isFirstFrame = true;

    private void OnEnable()
    {
        // 订阅手势识别结果事件
        handLandmarkerRunner.OnHandLandmarkerOutput.AddListener(OnHandLandmarkDetected);
    }

    private void OnDisable()
    {
        // 取消订阅
        handLandmarkerRunner.OnHandLandmarkerOutput.RemoveListener(OnHandLandmarkDetected);
    }

    private void OnHandLandmarkDetected(HandLandmarkerResult result)
    {
        if (result.handLandmarks == null || result.handLandmarks.Count == 0)
        {
            isFirstFrame = true;
            return;
        }

        var landmarks = result.handLandmarks[0].landmarks;

        // 计算手掌中心点(使用第9号关键点作为手掌中心)
        var palmCenter = landmarks[9];
        Vector3 currentPalmPosition = new Vector3(palmCenter.x, palmCenter.y, palmCenter.z);

        if (isFirstFrame)
        {
            previousPalmPosition = currentPalmPosition;
            isFirstFrame = false;
            return;
        }

        // 计算手掌移动方向和距离
        Vector3 palmMovement = currentPalmPosition - previousPalmPosition;

        // 判断手势方向并移动物体
        if (Mathf.Abs(palmMovement.x) > gestureThreshold)
        {
            // 左右移动
            float moveX = palmMovement.x * moveSpeed * Time.deltaTime;
            targetObject.transform.Translate(moveX, 0, 0);
        }

        if (Mathf.Abs(palmMovement.y) > gestureThreshold)
        {
            // 上下移动
            float moveY = palmMovement.y * moveSpeed * Time.deltaTime;
            targetObject.transform.Translate(0, moveY, 0);
        }

        if (Mathf.Abs(palmMovement.z) > gestureThreshold)
        {
            // 前后移动
            float moveZ = -palmMovement.z * moveSpeed * Time.deltaTime; // 注意Z轴方向取反
            targetObject.transform.Translate(0, 0, moveZ);
        }

        previousPalmPosition = currentPalmPosition;
    }
}
