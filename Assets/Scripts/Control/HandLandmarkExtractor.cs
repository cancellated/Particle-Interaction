using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Mediapipe.Tasks.Vision.Core;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

// 手部关键点检测器 - 使用MediaPipe实时检测手部21个关键点
public class HandLandmarkExtractor : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;  // 主摄像机，用于坐标转换
    [SerializeField] private RawImage videoScreen; // 显示摄像头画面的UI组件
    [SerializeField] private TextAsset handLandmarkerModel; // MediaPipe手部检测模型文件
    
    // 摄像头参数配置
    [SerializeField] private int width = 1280;   // 摄像头采集宽度
    [SerializeField] private int height = 720;  // 摄像头采集高度
    [SerializeField] private int fps = 30;      // 摄像头帧率

    private WebCamTexture webCamTexture;        // 摄像头纹理
    private HandLandmarker handLandmarker;     // MediaPipe手部检测器
    private Stopwatch stopwatch;               // 用于视频模式时间戳
    private readonly List<Vector3> landmarkPositions = new(); // 存储关键点位置

    [SerializeField] private GameObject debugMarkerPrefab; // 调试用标记预制体
    private List<GameObject> debugMarkers = new List<GameObject>(); // 所有调试标记

    private void Start()
    {
        // 检查模型文件是否存在
        if (handLandmarkerModel == null)
        {
            Debug.LogError("请设置手部检测模型文件！");
            enabled = false;
            return;
        }

        // 获取主摄像机
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // 初始化摄像头设备
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogError("未找到摄像头设备！");
            enabled = false;
            return;
        }

        // 创建并启动摄像头
        webCamTexture = new WebCamTexture(WebCamTexture.devices[0].name, width, height, fps);
        videoScreen.texture = webCamTexture;
        webCamTexture.Play();

        // 配置MediaPipe手部检测器
        var options = new HandLandmarkerOptions(
            baseOptions: new Mediapipe.Tasks.Core.BaseOptions(
                modelAssetBuffer: handLandmarkerModel.bytes // 加载模型数据
            ),
            runningMode: RunningMode.VIDEO, // 视频模式(连续帧)
            numHands: 1 // 检测单手
        );

        // 创建检测器实例
        handLandmarker = HandLandmarker.CreateFromOptions(options);
        stopwatch = new Stopwatch();
        stopwatch.Start(); // 开始计时

        // 初始化21个关键点调试标记
        if (debugMarkerPrefab != null)
        {
            for (int i = 0; i < 21; i++)
            {
                var marker = Instantiate(debugMarkerPrefab, Vector3.zero, Quaternion.identity);
                marker.name = $"LandmarkMarker_{i}";
                marker.SetActive(false);
                debugMarkers.Add(marker);
            }
        }

        // 启动检测协程
        StartCoroutine(DetectHandLandmarks());
    }

    // 持续检测手部关键点的协程
    private IEnumerator DetectHandLandmarks()
    {
        // 等待摄像头初始化完成
        yield return new WaitUntil(() => webCamTexture.width > 16);

        var waitForEndOfFrame = new WaitForEndOfFrame();
        
        // 创建纹理帧用于MediaPipe处理
        using var textureFrame = new Mediapipe.Unity.Experimental.TextureFrame(
            webCamTexture.width,
            webCamTexture.height,
            TextureFormat.RGBA32
        );

        // 预分配检测结果内存
        var result = HandLandmarkerResult.Alloc(1);

        // 主检测循环
        while (enabled)
        {
            yield return waitForEndOfFrame;

            // 从摄像头读取纹理并垂直翻转(适配MediaPipe坐标系)
            textureFrame.ReadTextureOnCPU(
                webCamTexture,
                flipHorizontally: false,
                flipVertically: true
            );
            
            // 构建CPU图像供MediaPipe处理
            using var image = textureFrame.BuildCPUImage();

            // 执行手部关键点检测
            if (handLandmarker.TryDetectForVideo(
                    image,
                    stopwatch.ElapsedMilliseconds, // 使用时间戳
                    null,
                    ref result
                ))
            {
                ProcessHandLandmarks(result); // 处理检测结果
            }
            else
            {
                // 未检测到手部时隐藏所有标记
                foreach (var marker in debugMarkers)
                {
                    marker.SetActive(false);
                }
                landmarkPositions.Clear();
            }
        }
    }

    // 处理检测结果并更新关键点位置
    private void ProcessHandLandmarks(HandLandmarkerResult result)
    {
        landmarkPositions.Clear(); // 清空上一帧数据

        // 检查是否有有效结果
        if (result.handLandmarks == null || result.handLandmarks.Count == 0)
        {
            foreach (var marker in debugMarkers)
            {
                marker.SetActive(false);
            }
            return;
        }

        // 获取第一个检测到的手的关键点
        var landmarks = result.handLandmarks[0].landmarks;

        // 处理每个关键点
        for (int i = 0; i < landmarks.Count; i++)
        {
            var landmark = landmarks[i];

            // 将MediaPipe坐标(0-1范围)转换为屏幕坐标
            Vector3 screenPos = new Vector3(
                landmark.x * Screen.width, // X坐标
                (1 - landmark.y) * Screen.height, // Y坐标(翻转)
                10f // Z深度
            );

            // 转换为世界坐标
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
            landmarkPositions.Add(worldPos);

            // 更新调试标记位置和可见性
            if (debugMarkerPrefab != null && i < debugMarkers.Count)
            {
                debugMarkers[i].transform.position = worldPos;
                debugMarkers[i].SetActive(true);
            }
        }

#if UNITY_EDITOR
        // 编辑器下输出关键点信息
        Debug.Log($"检测到{landmarks.Count}个关键点");
        Debug.Log($"手腕位置: {landmarkPositions[0]}");
        Debug.Log($"食指指尖: {landmarkPositions[8]}");
        Debug.Log($"手掌中心: {landmarkPositions[9]}");
#endif
    }

    // 清理资源
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

        // 销毁所有调试标记
        foreach (var marker in debugMarkers)
        {
            if (marker != null)
            {
                Destroy(marker);
            }
        }
    }

    // 获取指定索引的关键点世界坐标
    public Vector3 GetLandmarkPosition(int index)
    {
        if (index >= 0 && index < landmarkPositions.Count)
        {
            return landmarkPositions[index];
        }
        return Vector3.zero;
    }

    // 获取所有关键点位置
    public List<Vector3> GetAllLandmarkPositions()
    {
        return new List<Vector3>(landmarkPositions);
    }

    /*
    手部关键点索引对照表：
    0: 手腕
    1-4: 大拇指（从掌根到指尖）
    5-8: 食指
    9-12: 中指
    13-16: 无名指
    17-20: 小指
    */
}
