using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;
using AI.Assistant.Data;

public class APIManager : Singleton<APIManager>
{
    #region 配置参数
    [Header("API Settings")]
    public string siliconEndpoint = "https://api.siliconflow.cn/v1/chat/completions";
    public string siliconApiKey = "sk-ylgalqevviwxjblnpuzqxfgmvujzgajxfsolvqzuhckrlxmz";
    public string siliconModel = "Pro/deepseek-ai/DeepSeek-V3";
    public bool enableThinking = true;
    public int thinkingBudget = 4096;
    public float minP = 0.05f;
    public float temperature = 0.7f;
    public float topP = 0.7f;
    public int topK = 50;
    public float frequencyPenalty = 0.5f;
    public int n = 1;
    public string[] stop = new string[0];
    public int maxTokens = 1000;
    public bool showDebugLogs = true;
    #endregion

    #region 文本对话API
    // 日志前缀
    private const string LOG_PREFIX = "[AI API]";

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

    private IEnumerator SendTextMessageCoroutine(string message, Action<string> onResponse, Action<string> onError)
    {
        Debug.Log($"{LOG_PREFIX} 准备请求参数 - 模型: {siliconModel}, 温度: {temperature}, 最大token数: {maxTokens}");

        var requestData = new AIRequest
        {
            model = siliconModel,
            messages = new[] {
                new AIMessage {
                    role = "user",
                    content = message
                }
            },
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

        if (request.result != UnityWebRequest.Result.Success)
        {
            string errorMsg = $"{LOG_PREFIX} 请求失败: {request.error}";
            Debug.LogError(errorMsg);
            onError?.Invoke(errorMsg);
            yield break;
        }

        string responseJson = request.downloadHandler.text;
        Debug.Log($"{LOG_PREFIX} 收到原始响应: {responseJson}");

        try
        {
            var response = JsonUtility.FromJson<AIResponse>(responseJson);
            if (response.choices != null && response.choices.Length > 0)
            {
                string contentText = response.choices[0].message.content;
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
}
    #endregion

