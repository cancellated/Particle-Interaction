using System.Collections.Generic;
using Mediapipe.Tasks.Vision.HandLandmarker;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandGestureController : MonoBehaviour
{
    [Header("MediaPipe设置")]
    public HandLandmarkerRunner handLandmarkerRunner;

    [Header("手势设置")]
    public float moveSensitivity = 10f; // 鼠标移动灵敏度
    public float clickThreshold = 0.05f; // 点击判定阈值

    private Vector3 previousPalmPosition;
    private bool isFirstFrame = true;
    private bool isPinching = false;

    private void OnEnable()
    {
        handLandmarkerRunner.OnHandLandmarkerOutput.AddListener(OnHandLandmarkDetected);
    }

    private void OnDisable()
    {
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
        var palmCenter = landmarks[9];
        Vector3 currentPalmPosition = new Vector3(palmCenter.x, palmCenter.y, palmCenter.z);

        if (isFirstFrame)
        {
            previousPalmPosition = currentPalmPosition;
            isFirstFrame = false;
            return;
        }

        // 计算手势移动量并转换为鼠标移动
        Vector3 palmMovement = currentPalmPosition - previousPalmPosition;
        Mouse.current.WarpCursorPosition(Mouse.current.position.ReadValue() + 
            new Vector2(palmMovement.x * moveSensitivity, -palmMovement.y * moveSensitivity));

        // 检测捏合手势(拇指和食指距离)
        var thumbTip = landmarks[4];
        var indexTip = landmarks[8];
        float pinchDistance = Vector3.Distance(
            new Vector3(thumbTip.x, thumbTip.y, thumbTip.z),
            new Vector3(indexTip.x, indexTip.y, indexTip.z));

        // 触发鼠标点击
        if (pinchDistance < clickThreshold && !isPinching)
        {
            isPinching = true;
            Mouse.current.leftButton.Click();
        }
        else if (pinchDistance >= clickThreshold)
        {
            isPinching = false;
        }

        previousPalmPosition = currentPalmPosition;
    }
}
