using System.Collections;
using System.Collections.Generic;
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
    private ExitGameMenu exitGameMenu; // 退出菜单引用

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

    private bool isLeftHandFist = false;
    private bool isRightHandFist = false;
    private float doubleFistTimer = 0f;
    private const float DOUBLE_FIST_THRESHOLD = 0.5f; // 双手握拳持续时间阈值
    private float singleFistTimer = 0f;
    private const float SINGLE_FIST_THRESHOLD = 0.5f; // 单手握拳持续时间阈值
    private float lastProcessTime;
    private bool waitForHandsRelease = false; // 等待手势释放
    private bool canTriggerNextAction = true; // 是否可以触发下一个动作

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
            numHands: 2 // 修改为检测两只手
        );

        handLandmarker = HandLandmarker.CreateFromOptions(options);
        stopwatch = new Stopwatch();
        stopwatch.Start();

        lastProcessTime = Time.realtimeSinceStartup;

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

        var result = HandLandmarkerResult.Alloc(2);

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
                isLeftHandFist = false;
                isRightHandFist = false;
            }
        }
    }

    private void ProcessHandLandmarks(HandLandmarkerResult result)
    {
        float deltaTime = Time.realtimeSinceStartup - lastProcessTime;
        lastProcessTime = Time.realtimeSinceStartup;

        if (result.handLandmarks == null || result.handLandmarks.Count == 0)
        {
            isFirstFrame = true;
            isLeftHandFist = false;
            isRightHandFist = false;
            doubleFistTimer = 0f;
            singleFistTimer = 0f;
            // 当没有检测到手时，重置等待状态
            waitForHandsRelease = false;
            return;
        }

        // 重置握拳状态
        bool previousLeftFist = isLeftHandFist;
        bool previousRightFist = isRightHandFist;
        isLeftHandFist = false;
        isRightHandFist = false;

        // 处理每只检测到的手
        for (int handIndex = 0; handIndex < result.handLandmarks.Count; handIndex++)
        {
            var landmarks = result.handLandmarks[handIndex].landmarks;
            var handedness = result.handedness[handIndex].categories[0].categoryName;

            // 获取关键点
            var palmLandmark = landmarks[9];
            var indexFingerTip = landmarks[8];
            var pinkyTip = landmarks[20];
            var middleFingerTip = landmarks[12];
            var ringFingerTip = landmarks[16];
            var palmBase = landmarks[0];

            // 计算手掌基准长度
            float palmLength = Vector2.Distance(
                new Vector2(palmBase.x, palmBase.y),
                new Vector2(palmLandmark.x, palmLandmark.y)
            );

            float distanceThreshold = palmLength * 0.9f;

            // 计算各手指到手掌中心的距离
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

            // 检测握拳
            bool isFist =
                indexFingerDist < distanceThreshold
                && middleFingerDist < distanceThreshold
                && ringFingerDist < distanceThreshold
                && pinkyDist < distanceThreshold;

            // 更新左右手握拳状态
            if (handedness.ToLower() == "left")
            {
                isLeftHandFist = isFist;
            }
            else if (handedness.ToLower() == "right")
            {
                isRightHandFist = isFist;
            }

            // 如果这只手没有握拳，且是主控制手（这里假设使用右手），且退出菜单未打开，则处理移动逻辑
            if (!isFist && handedness.ToLower() == "right" && !exitGameMenu.exitMenuUI.activeSelf)
            {
                ProcessHandMovement(landmarks, handIndex == 0);
            }
        }

        // 检查是否需要等待手势释放
        if (waitForHandsRelease)
        {
            // 如果双手都松开了，重置等待状态和计时器
            if (!isLeftHandFist && !isRightHandFist)
            {
                waitForHandsRelease = false;
                canTriggerNextAction = true;
                doubleFistTimer = 0f;
                singleFistTimer = 0f;
            }
            return; // 等待手势释放期间不处理其他手势
        }

        // 检查手势状态
        if (exitGameMenu.exitMenuUI.activeSelf)
        {
            // 在退出菜单打开时的手势检测
            if (isLeftHandFist && isRightHandFist && canTriggerNextAction)
            {
                doubleFistTimer += deltaTime;
                if (doubleFistTimer >= DOUBLE_FIST_THRESHOLD)
                {
                    Debug.Log("检测到双手同时握拳，退出游戏");
                    exitGameMenu.QuitGame();
                    doubleFistTimer = 0f;
                    waitForHandsRelease = true;
                    canTriggerNextAction = false;
                }
            }
            else if ((isLeftHandFist || isRightHandFist) && canTriggerNextAction)
            {
                singleFistTimer += deltaTime;
                if (singleFistTimer >= SINGLE_FIST_THRESHOLD)
                {
                    Debug.Log("检测到单手握拳，重置游戏");
                    exitGameMenu.ResetTargetPosition();
                    singleFistTimer = 0f;
                    waitForHandsRelease = true;
                    canTriggerNextAction = false;
                }
            }
            else
            {
                doubleFistTimer = 0f;
                singleFistTimer = 0f;
            }
        }
        else
        {
            // 在游戏正常运行时的手势检测
            if (isLeftHandFist && isRightHandFist && canTriggerNextAction)
            {
                doubleFistTimer += deltaTime;
                if (doubleFistTimer >= DOUBLE_FIST_THRESHOLD && exitGameMenu != null)
                {
                    Debug.Log("检测到双手同时握拳，打开退出菜单");
                    exitGameMenu.ToggleMenu();
                    doubleFistTimer = 0f;
                    waitForHandsRelease = true;
                    canTriggerNextAction = false;
                }
            }
            else
            {
                doubleFistTimer = 0f;
            }
        }
    }

    private void ProcessHandMovement(
        List<Mediapipe.Tasks.Components.Containers.NormalizedLandmark> landmarks,
        bool isFirstHand
    )
    {
        var palmLandmark = landmarks[9];
        var indexFingerTip = landmarks[8];
        var pinkyTip = landmarks[20];

        // 计算手掌大小
        float currentPalmSize = Vector2.Distance(
            new Vector2(indexFingerTip.x, indexFingerTip.y),
            new Vector2(pinkyTip.x, pinkyTip.y)
        );

        // 转换坐标
        Vector3 screenPos = new Vector3(
            palmLandmark.x * Screen.width,
            (1 - palmLandmark.y) * Screen.height,
            10f
        );

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
        movement.x = -movement.x;

        // 计算前后移动
        float palmSizeDelta = currentPalmSize - previousPalmSize;
        movement.z = palmSizeDelta * depthMoveSpeed;

        // 应用移动
        if (movement.magnitude > movementThreshold)
        {
            targetPosition += movement * moveSpeed * Time.unscaledDeltaTime;
        }

        // 平滑插值到目标位置
        targetObject.transform.position = Vector3.Lerp(
            targetObject.transform.position,
            targetPosition,
            smoothSpeed
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
