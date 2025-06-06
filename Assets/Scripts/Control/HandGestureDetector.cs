using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Tasks.Vision.Core;
using System.Collections;
using Debug = UnityEngine.Debug;
using GestureControl.Data;

namespace GestureControl.Detector
{
    /// <summary>
    /// 手势检测器 - 负责从摄像头捕获图像并识别手势
    /// </summary>
    public class HandGestureDetector : MonoBehaviour
    {
        [SerializeField] private TextAsset handLandmarkerModel; // 手部关键点检测模型
        [SerializeField] private RawImage videoScreen;          // 显示摄像头画面的UI组件
        [SerializeField] private int width = 1920;             // 摄像头分辨率宽度
        [SerializeField] private int height = 1080;              // 摄像头分辨率高度
        [SerializeField] private int fps = 30;                // 摄像头帧率

        private HandLandmarker handLandmarker;  // MediaPipe手部关键点检测器
        private WebCamTexture webCamTexture;    // 摄像头纹理
        private Stopwatch stopwatch;            // 用于视频帧时间戳

        private bool isLeftHandFist = false;
        private bool isRightHandFist = false;
        private bool waitForHandsRelease = false;

        // 手势检测完成事件
        // 修改事件委托类型
        public System.Action<HandGestureData> OnHandGestureDetected;
        private void Start()
        {
            // 参数校验
            if (handLandmarkerModel == null)
            {
                Debug.LogError("请设置手部检测模型文件！");
                enabled = false;
                return;
            }

            if (WebCamTexture.devices.Length == 0)
            {
                Debug.LogError("未找到摄像头设备！");
                enabled = false;
                return;
            }

            // 初始化摄像头
            webCamTexture = new WebCamTexture(WebCamTexture.devices[0].name, width, height, fps);
            videoScreen.texture = webCamTexture;
            webCamTexture.Play();

            // 配置手部关键点检测器
            var options = new HandLandmarkerOptions(
                baseOptions: new Mediapipe.Tasks.Core.BaseOptions(modelAssetBuffer: handLandmarkerModel.bytes),
                runningMode: RunningMode.VIDEO,  // 视频模式
                numHands: 2                     // 检测两只手
            );

            handLandmarker = HandLandmarker.CreateFromOptions(options);
            stopwatch = new Stopwatch();
            stopwatch.Start();

            // 开始检测循环
            StartCoroutine(DetectHandLandmarks());
        }

        /// <summary>
        /// 手部关键点检测协程
        /// </summary>
        private IEnumerator DetectHandLandmarks()
        {
            // 等待摄像头初始化完成
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

                // 从摄像头捕获当前帧
                textureFrame.ReadTextureOnCPU(
                    webCamTexture,
                    flipHorizontally: false,  // 水平不翻转
                    flipVertically: true      // 垂直翻转(匹配Unity坐标系)
                );
                using var image = textureFrame.BuildCPUImage();

                // 检测手部关键点
                if (handLandmarker.TryDetectForVideo(
                    image,
                    stopwatch.ElapsedMilliseconds,  // 当前帧时间戳
                    null,
                    ref result))
                {
                    ProcessHandLandmarks(result);  // 处理检测结果
                    
                }
                else
                {
                    // 未检测到手部时重置状态
                    isLeftHandFist = false;
                    isRightHandFist = false;
                    waitForHandsRelease = false;
                }
            }
        }

        /// <summary>
        /// 处理手部关键点检测结果
        /// </summary>
        /// <param name="result">检测结果</param>
        private void ProcessHandLandmarks(HandLandmarkerResult result)
        {
            // 创建手势数据实例
            var gestureData = new HandGestureData
            {
                RawResult = result,
                DetectionTime = Time.time,
                IsLeftHandFist = isLeftHandFist,
                IsRightHandFist = isRightHandFist
            };

            // 无检测结果时重置状态
            if (result.handLandmarks == null || result.handLandmarks.Count == 0)
            {
                isLeftHandFist = false;
                isRightHandFist = false;
                waitForHandsRelease = false;
                gestureData.IsLeftHandFist = false;
                gestureData.IsRightHandFist = false;
                OnHandGestureDetected?.Invoke(gestureData);
                return;
            }

            // 处理检测结果并填充手势数据
            // 重置握拳状态
            isLeftHandFist = false;
            isRightHandFist = false;

            // 处理每只检测到的手
            foreach (var handIndex in result.handLandmarks)
            {
                var landmarks = handIndex.landmarks;  // 手部关键点
                var handedness = result.handedness[0].categories[0].categoryName;  // 左右手标识

                // 获取关键点位置
                var palmLandmark = landmarks[9];      // 手掌中心点
                var indexFingerTip = landmarks[8];    // 食指尖
                var pinkyTip = landmarks[20];         // 小指尖
                var middleFingerTip = landmarks[12];   // 中指尖
                var ringFingerTip = landmarks[16];     // 无名指尖

                // 计算手掌大小作为距离阈值基准
                float palmLength = Vector2.Distance(
                    new Vector2(landmarks[0].x, landmarks[0].y),
                    new Vector2(palmLandmark.x, palmLandmark.y)
                );

                float distanceThreshold = palmLength * 0.9f;  // 握拳判定阈值

                // 判断是否握拳(指尖到手掌中心的距离小于阈值)
                bool isFist = 
                    Vector2.Distance(new Vector2(indexFingerTip.x, indexFingerTip.y), new Vector2(palmLandmark.x, palmLandmark.y)) < distanceThreshold &&
                    Vector2.Distance(new Vector2(middleFingerTip.x, middleFingerTip.y), new Vector2(palmLandmark.x, palmLandmark.y)) < distanceThreshold &&
                    Vector2.Distance(new Vector2(ringFingerTip.x, ringFingerTip.y), new Vector2(palmLandmark.x, palmLandmark.y)) < distanceThreshold &&
                    Vector2.Distance(new Vector2(pinkyTip.x, pinkyTip.y), new Vector2(palmLandmark.x, palmLandmark.y)) < distanceThreshold;

                // 更新左右手握拳状态
                if (handedness.ToLower() == "left") 
                {
                    isLeftHandFist = isFist;
                    gestureData.IsLeftHandFist = isFist;
                    gestureData.LeftPalmPosition = new Vector2(palmLandmark.x, palmLandmark.y);
                }
                else if (handedness.ToLower() == "right") 
                {
                    isRightHandFist = isFist;
                    gestureData.IsRightHandFist = isFist;
                    gestureData.RightPalmPosition = new Vector2(palmLandmark.x, palmLandmark.y);
                }
            }

            // 触发手势检测事件
            OnHandGestureDetected?.Invoke(gestureData);
        }

        private void OnDestroy()
        {
            // 释放资源
            if (webCamTexture != null) webCamTexture.Stop();
            handLandmarker?.Close();
        }
    }
}