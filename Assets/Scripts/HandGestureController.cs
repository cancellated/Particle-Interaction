using System.Collections;
using System.Diagnostics;
using Mediapipe.Tasks.Vision.Core;
using Mediapipe.Tasks.Vision.HandLandmarker;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class HandGestureController : MonoBehaviour
{
    [SerializeField]
    private GameObject targetObject;

    [SerializeField]
    private float moveSpeed = 15.0f;

    [SerializeField]
    private float depthMoveSpeed = 20.0f; // 前后移动的速度

    [SerializeField]
    private float smoothSpeed = 0.1f; // 平滑系数

    [SerializeField]
    private float movementThreshold = 0.0005f; // 移动阈值

    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private RawImage videoScreen;

    [SerializeField]
    private TextAsset handLandmarkerModel;

    [SerializeField]
    private int width = 1280;

    [SerializeField]
    private int height = 720;

    [SerializeField]
    private int fps = 30;

    private HandLandmarker handLandmarker;
    private WebCamTexture webCamTexture;
    private Vector3 targetPosition; // 目标位置
    private Vector3 previousPalmPos;
    private float previousPalmSize;
    private bool isFirstFrame = true;
    private Stopwatch stopwatch;

    private void Start()
    {
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

        if (handLandmarkerModel == null)
        {
            Debug.LogError("请设置手部检测模型文件！");
            enabled = false;
            return;
        }

        // 初始化摄像头
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogError("未找到摄像头设备！");
            enabled = false;
            return;
        }

        webCamTexture = new WebCamTexture(WebCamTexture.devices[0].name, width, height, fps);
        videoScreen.texture = webCamTexture;
        webCamTexture.Play();

        // 初始化HandLandmarker
        var options = new HandLandmarkerOptions(
            baseOptions: new Mediapipe.Tasks.Core.BaseOptions(
                modelAssetBuffer: handLandmarkerModel.bytes
            ),
            runningMode: RunningMode.VIDEO,
            numHands: 1
        );

        handLandmarker = HandLandmarker.CreateFromOptions(options);
        stopwatch = new Stopwatch();
        stopwatch.Start();

        // 开始检测循环
        StartCoroutine(DetectHandLandmarks());
    }

    private IEnumerator DetectHandLandmarks()
    {
        // 等待摄像头准备就绪
        yield return new WaitUntil(() => webCamTexture.width > 16);

        var waitForEndOfFrame = new WaitForEndOfFrame();
        using var textureFrame = new Mediapipe.Unity.Experimental.TextureFrame(
            webCamTexture.width,
            webCamTexture.height,
            TextureFormat.RGBA32
        );

        var result = HandLandmarkerResult.Alloc(1);

        while (enabled)
        {
            yield return waitForEndOfFrame;

            // 读取并翻转图像
            textureFrame.ReadTextureOnCPU(
                webCamTexture,
                flipHorizontally: false,
                flipVertically: true
            );
            using var image = textureFrame.BuildCPUImage();

            // 检测手部关键点
            if (
                handLandmarker.TryDetectForVideo(
                    image,
                    stopwatch.ElapsedMilliseconds,
                    null,
                    ref result
                )
            )
            {
                ProcessHandLandmarks(result);
            }
            else
            {
                isFirstFrame = true;
            }
        }
    }

    private void ProcessHandLandmarks(HandLandmarkerResult result)
    {
        if (result.handLandmarks == null || result.handLandmarks.Count == 0)
        {
            isFirstFrame = true;
            return;
        }

        var landmarks = result.handLandmarks[0].landmarks;

        // 使用手掌中心点（第9个关键点）作为控制点
        var palmLandmark = landmarks[9];
        var indexFingerTip = landmarks[8]; // 食指尖
        var pinkyTip = landmarks[20]; // 小指尖
        var middleFingerTip = landmarks[12]; // 中指尖
        var ringFingerTip = landmarks[16]; // 无名指尖
        var palmBase = landmarks[0]; // 手腕

        // 计算手掌基准长度（手腕到手掌中心的距离）
        float palmLength = Vector2.Distance(
            new Vector2(palmBase.x, palmBase.y),
            new Vector2(palmLandmark.x, palmLandmark.y)
        );

        // 检查所有手指是否都靠近手掌中心
        float distanceThreshold = palmLength * 0.9f;

        // 计算每个手指尖到手掌中心的距离
        float indexFingerDist = Vector2.Distance(
            new Vector2(indexFingerTip.x, indexFingerTip.y),
            new Vector2(palmLandmark.x, palmLandmark.y)
        );
        float middleFingerDist = Vector2.Distance(
            new Vector2(middleFingerTip.x, middleFingerTip.y),
            new Vector2(palmLandmark.x, palmLandmark.y)
        );
        float ringFingerDist = Vector2.Distance(
            new Vector2(ringFingerTip.x, ringFingerTip.y),
            new Vector2(palmLandmark.x, palmLandmark.y)
        );
        float pinkyDist = Vector2.Distance(
            new Vector2(pinkyTip.x, pinkyTip.y),
            new Vector2(palmLandmark.x, palmLandmark.y)
        );

        // 如果所有手指都靠近手掌中心，判定为握拳
        if (
            indexFingerDist < distanceThreshold
            && middleFingerDist < distanceThreshold
            && ringFingerDist < distanceThreshold
            && pinkyDist < distanceThreshold
        )
        {
            Debug.Log("检测到握拳手势，停止移动");
            return;
        }

        // 计算手掌大小（使用食指尖到小指尖的距离）
        float currentPalmSize = Vector2.Distance(
            new Vector2(indexFingerTip.x, indexFingerTip.y),
            new Vector2(pinkyTip.x, pinkyTip.y)
        );

        // 将MediaPipe坐标（0-1范围）转换为屏幕坐标
        Vector3 screenPos = new Vector3(
            palmLandmark.x * Screen.width,
            (1 - palmLandmark.y) * Screen.height,
            10f
        );

        // 将屏幕坐标转换为世界坐标
        Vector3 currentPalmPos = mainCamera.ScreenToWorldPoint(screenPos);

        if (isFirstFrame)
        {
            previousPalmPos = currentPalmPos;
            previousPalmSize = currentPalmSize;
            targetPosition = targetObject.transform.position;
            isFirstFrame = false;
            return;
        }

        // 计算移动向量
        Vector3 movement = currentPalmPos - previousPalmPos;
        movement.x = -movement.x; // 反转X轴移动方向

        // 计算前后移动（基于手掌大小变化）
        float palmSizeDelta = currentPalmSize - previousPalmSize;
        movement.z = palmSizeDelta * depthMoveSpeed;

        // 只有当移动量超过阈值时才更新目标位置
        if (movement.magnitude > movementThreshold)
        {
            // 计算新的目标位置
            targetPosition += movement * moveSpeed * Time.deltaTime;
        }

        // 平滑插值到目标位置
        targetObject.transform.position = Vector3.Lerp(
            targetObject.transform.position,
            targetPosition,
            smoothSpeed
        );

        // 添加调试日志
        Debug.Log(
            $"手掌位置: {currentPalmPos}, 移动方向: {movement}, 手掌大小变化: {palmSizeDelta}"
        );

        // 更新前一帧数据
        previousPalmPos = currentPalmPos;
        previousPalmSize = currentPalmSize;
    }

    private void OnDestroy()
    {
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
        }

        if (handLandmarker != null)
        {
            handLandmarker.Close();
        }
    }

    /*
    关键点索引对照表：
    0: 手腕
    1-4: 大拇指（从掌根到指尖）
    5-8: 食指
    9-12: 中指
    13-16: 无名指
    17-20: 小指
    */
}
