using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;

public class APIManager : Singleton<APIManager>
{
    [Header("API Settings")]
    public string siliconEndpoint = "https://api.silicon.com/v1/chat/completions";
    public string siliconApiKey = "your-api-key-here";
    public string siliconModel = "gpt-4";
    public float temperature = 0.7f;
    public float topP = 1.0f;
    public int maxTokens = 1000;
    public bool showDebugLogs = true;

    #region 文本对话API
    // 在类顶部添加日志前缀常量
    private const string LOG_PREFIX = "[AI API]";
    
    // 修改SendTextMessage方法
    public void SendTextMessage(string message, Action<string> onResponse, Action<string> onError = null)
    {
        if (string.IsNullOrEmpty(message))
        {
            string errorMsg = $"{LOG_PREFIX} 错误: 消息内容不能为空";
            Debug.LogError(errorMsg);
            onError?.Invoke(errorMsg);
            return;
        }
    
        Debug.Log($"{LOG_PREFIX} 开始处理用户消息: {message}");
        StartCoroutine(SendTextMessageCoroutine(message, onResponse, onError));
    }
    
    // 修改SendTextMessageCoroutine方法
    private IEnumerator SendTextMessageCoroutine(string message, Action<string> onResponse, Action<string> onError)
    {
        Debug.Log($"{LOG_PREFIX} 准备发送请求，模型: {siliconModel}, 温度: {temperature}, 最大token数: {maxTokens}");

        if (showDebugLogs)
        {
            Debug.Log($"[API] 准备发送文本消息: {message}");
        }

        var requestData = new AIRequest
        {
            model = siliconModel,
            messages = new[] {
                new AIMessage {
                    role = "user",
                    content = new ContentPart[] {
                        new() {
                            type = "text",
                            text = message
                        }
                    }
                }
            },
            temperature = temperature,
            top_p = topP,
            max_tokens = maxTokens,
            stream = false
        };

        string jsonData = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using UnityWebRequest request = new(siliconEndpoint, "POST")
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerBuffer(),
            timeout = 30
        };
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {siliconApiKey}");

        // 在请求发送前添加日志
        Debug.Log($"{LOG_PREFIX} 正在发送请求到: {siliconEndpoint}");
        Debug.Log($"{LOG_PREFIX} 请求内容: {jsonData}");

        yield return request.SendWebRequest();

        // 添加响应状态日志
        Debug.Log($"{LOG_PREFIX} 请求完成，状态: {request.result}, 响应码: {request.responseCode}");

        if (request.result != UnityWebRequest.Result.Success)
        {
            string errorMsg = $"{LOG_PREFIX} 请求失败: {request.error}";
            Debug.LogError(errorMsg);
            onError?.Invoke(errorMsg);
            yield break;
        }

        string responseJson = request.downloadHandler.text;
        if (showDebugLogs)
        {
            Debug.Log($"[API] 收到响应: {responseJson}");
        }

        try
        {
            var response = JsonUtility.FromJson<AIResponse>(responseJson);
            if (response.choices != null && response.choices.Length > 0)
            {
                string contentText = "";
                if (response.choices[0].message.content != null)
                {
                    foreach (var part in response.choices[0].message.content)
                    {
                        if (part.type == "text" && !string.IsNullOrEmpty(part.text))
                        {
                            contentText += part.text;
                        }
                    }
                }
                onResponse?.Invoke(contentText);
            }
            else
            {
                onError?.Invoke("API返回了空响应");
            }
        }
        catch (Exception ex)
        {
            string errorMsg = $"[API] 解析响应失败: {ex.Message}";
            Debug.LogError(errorMsg);
            onError?.Invoke(errorMsg);
        }
    }
    #endregion

    #region 数据模型
    [Serializable]
    private class AIRequest
    {
        public string model;
        public AIMessage[] messages;
        public float temperature;
        public float top_p;
        public int max_tokens;
        public bool stream;
    }

    [Serializable]
    private class AIMessage
    {
        public string role;
        public ContentPart[] content;
    }

    [Serializable]
    private class ContentPart
    {
        public string type;
        public string text;
        public ImageUrl image_url;
    }

    [Serializable]
    private class ImageUrl
    {
        public string url;
        public string detail;
    }

    [Serializable]
    private class AIResponse
    {
        public Choice[] choices;
    }

    [Serializable]
    private class Choice
    {
        public AIMessage message;
    }
    #endregion
}
