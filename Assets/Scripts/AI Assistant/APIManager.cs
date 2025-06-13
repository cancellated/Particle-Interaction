using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;
using AI.Assistant.Data;


namespace AI.Assistant.API
{ 
    /// <summary>
    /// APIManager 负责与AI后端接口通信，发送用户消息并获取AI回复，支持上下文记忆
    /// </summary>
    public class APIManager : Singleton<APIManager>
    {
        #region 配置参数
        [Header("API Settings")]
        public string siliconEndpoint = "https://api.siliconflow.cn/v1/chat/completions"; // API接口地址
        public string siliconApiKey = "sk-ylgalqevviwxjblnpuzqxfgmvujzgajxfsolvqzuhckrlxmz"; // API密钥
        public string siliconModel = "deepseek-ai/DeepSeek-V3"; // 使用的AI模型
        public bool enableThinking = true; // 是否启用思考模式
        public int thinkingBudget = 4096; // 思考预算
        public float minP = 0.05f; // 最小概率
        public float temperature = 0.7f; // 采样温度
        public float topP = 0.7f; // top-p采样
        public int topK = 50; // top-k采样
        public float frequencyPenalty = 0.5f; // 频率惩罚
        public int n = 1; // 返回结果数量
        public string[] stop = new string[0]; // 停止词
        public int maxTokens = 512; // 最大token数
        public bool showDebugLogs = true; // 是否显示调试日志

        // 聊天历史，便于上下文记忆
        private readonly AIChatHistory chatHistory = new();
        // 日志前缀
        private const string LOG_PREFIX = "[AI API]";
        #endregion

        #region 生命周期管理
        void OnEnable()
        {
            GameEvents.OnAIDialogueStart += HandleAIDialogueStart;
            GameEvents.OnAIDialogueSend += HandleAIDialogueSend;
            GameEvents.OnAIDialogueComplete += HandleAIDialogueComplete;
        }
        void OnDisable()
        {
            GameEvents.OnAIDialogueStart -= HandleAIDialogueStart;
            GameEvents.OnAIDialogueSend -= HandleAIDialogueSend;
            GameEvents.OnAIDialogueComplete -= HandleAIDialogueComplete;
        }
        #endregion

        #region 消息收发与解析
        /// <summary>
        /// 发送文本消息到AI接口，获取AI回复（带上下文）
        /// </summary>
        /// <param name="message">用户输入的消息</param>
        /// <param name="onResponse">AI回复回调</param>
        /// <param name="onError">错误回调</param>
        public void SendTextMessage(string message, Action<string> onResponse, Action<string> onError = null)
        {
            Debug.Log($"{LOG_PREFIX} 开始处理用户消息: {message}");
            if (string.IsNullOrEmpty(message))
            {
                string errorMsg = $"{LOG_PREFIX} 错误: 消息内容不能为空";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
                return;
            }
            StartCoroutine(SendTextMessageCoroutine(message, onResponse, onError));
        }

        /// <summary>
        /// 协程方式发送文本消息到AI接口
        /// </summary>
        /// <param name="message">用户输入的消息</param>
        /// <param name="onResponse">AI回复回调</param>
        /// <param name="onError">错误回调</param>
        private IEnumerator SendTextMessageCoroutine(string message, Action<string> onResponse, Action<string> onError)
        {
            Debug.Log($"{LOG_PREFIX} 准备请求参数 - 模型: {siliconModel}, 温度: {temperature}, 最大token数: {maxTokens}");

            // 1. 构建消息列表（system prompt + 历史 + 当前用户消息）
            List<AIMessage> messages = new()
            {
                // system prompt
                new AIMessage
                {
                    role = "system",
                    content = "请严格以如下JSON格式回复：" +
                    "{\"text\": \"正文内容\", \"option1\": \"选项1\", \"option2\": \"选项2\", \"option3\": \"选项3\"}。" +
                    "正文约100字，语气轻快活泼，选项每个10-15字。不要输出多余内容。"
                },
                new AIMessage {
                    role = "system",
                    content = "你收到的消息中，role为user的消息为当前消息，role为assistant的为历史消息，请结合历史消息和当前消息回复。"

                }
            
            };

            // 添加历史消息
            messages.AddRange(chatHistory.GetRecent(5));

            // 当前用户消息
            messages.Add(new AIMessage
            {
                role = "user",
                content = message
            });

            // 2. 构建请求数据
            var requestData = new AIRequest
            {
                model = siliconModel,
                messages = chatHistory.GetRecent(10),
                stream = false,
                max_tokens = maxTokens,
                enable_thinking = enableThinking,
                thinking_budget = thinkingBudget,
                min_p = minP,
                temperature = temperature,
                top_p = topP,
                top_k = topK,
                frequency_penalty = frequencyPenalty,
                n = n,
                stop = stop
            };

            // 3. 序列化为JSON
            string jsonData = JsonUtility.ToJson(requestData);
            Debug.Log($"{LOG_PREFIX} 请求JSON数据: {jsonData}");

            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            using UnityWebRequest request = new(siliconEndpoint, "POST")
            {
                uploadHandler = new UploadHandlerRaw(bodyRaw),
                downloadHandler = new DownloadHandlerBuffer(),
                timeout = 30
            };
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {siliconApiKey}");

            Debug.Log($"{LOG_PREFIX} 正在发送请求到: {siliconEndpoint}");
            yield return request.SendWebRequest();

            Debug.Log($"{LOG_PREFIX} 请求完成，状态: {request.result}, 响应码: {request.responseCode}");

            // 检查请求结果
            if (request.result != UnityWebRequest.Result.Success)
            {
                string errorMsg = $"{LOG_PREFIX} 请求失败: {request.error}";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
                yield break;
            }

            // 获取响应内容
            string responseJson = request.downloadHandler.text;
            Debug.Log($"{LOG_PREFIX} 收到原始响应: {responseJson}");

            try
            {
                // 解析AI响应
                var response = JsonUtility.FromJson<AIResponse>(responseJson);
                if (response.choices != null && response.choices.Length > 0)
                {
                    string contentText = response.choices[0].message.content;
                    // 把AI回复加入历史
                    chatHistory.Add("assistant", contentText);
                    Debug.Log($"{LOG_PREFIX} 解析成功，AI回复内容: {contentText}");
                    onResponse?.Invoke(contentText);
                }
                else
                {
                    string errorMsg = $"{LOG_PREFIX} API返回了空响应";
                    Debug.LogError(errorMsg);
                    onError?.Invoke(errorMsg);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"{LOG_PREFIX} 解析响应失败: {ex.Message}";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
            }
        }
        #endregion

        #region 事件处理
        public void HandleAIDialogueStart()
        { 
        }
        private void HandleAIDialogueSend(string userText)
        {
            SendTextMessage(userText,
                response => GameEvents.TriggerAIDialogueResponse(response),
                error => Debug.LogError(error));
        }
        public void HandleAIDialogueComplete()
        {
            chatHistory.Clear();
        }
        #endregion

    }
}
