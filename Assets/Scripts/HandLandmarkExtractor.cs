using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Mediapipe.Tasks.Vision.Core;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class HandLandmarkExtractor : MonoBehaviour
{
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

    private WebCamTexture webCamTexture;
    private HandLandmarker handLandmarker;
    private Stopwatch stopwatch;
    private List<Vector3> landmarkPositions = new List<Vector3>();

    [SerializeField]
    private GameObject debugMarkerPrefab;
    private List<GameObject> debugMarkers = new List<GameObject>();

    private void Start()
    {
        if (handLandmarkerModel == null)
        {
            Debug.LogError("请设置手部检测模型文件！");
            enabled = false;
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
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

        // 初始化调试标记
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

        // 开始检测循程
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
                // 如果没有检测到手，隐藏所有标记
                foreach (var marker in debugMarkers)
                {
                    marker.SetActive(false);
                }
                landmarkPositions.Clear();
            }
        }
    }

    private void ProcessHandLandmarks(HandLandmarkerResult result)
    {
        landmarkPositions.Clear();

        if (result.handLandmarks == null || result.handLandmarks.Count == 0)
        {
            foreach (var marker in debugMarkers)
            {
                marker.SetActive(false);
            }
            return;
        }

        var landmarks = result.handLandmarks[0].landmarks;

        for (int i = 0; i < landmarks.Count; i++)
        {
            var landmark = landmarks[i];

            // 将MediaPipe坐标转换为屏幕坐标
            Vector3 screenPos = new Vector3(
                landmark.x * Screen.width,
                (1 - landmark.y) * Screen.height,
                10f
            );

            // 将屏幕坐标转换为世界坐标
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
            landmarkPositions.Add(worldPos);

            // 更新调试标记
            if (debugMarkerPrefab != null && i < debugMarkers.Count)
            {
                debugMarkers[i].transform.position = worldPos;
                debugMarkers[i].SetActive(true);
            }
        }

#if UNITY_EDITOR
        Debug.Log($"检测到{landmarks.Count}个关键点");
        Debug.Log($"手腕位置: {landmarkPositions[0]}");
        Debug.Log($"食指指尖: {landmarkPositions[8]}");
        Debug.Log($"手掌中心: {landmarkPositions[9]}");
#endif
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

        foreach (var marker in debugMarkers)
        {
            if (marker != null)
            {
                Destroy(marker);
            }
        }
    }

    // 获取特定关键点的世界坐标
    public Vector3 GetLandmarkPosition(int index)
    {
        if (index >= 0 && index < landmarkPositions.Count)
        {
            return landmarkPositions[index];
        }
        return Vector3.zero;
    }

    // 获取所有关键点的位置
    public List<Vector3> GetAllLandmarkPositions()
    {
        return new List<Vector3>(landmarkPositions);
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
