namespace AI.Assistant.Data
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class AIRequest
    {
        public string model;
        public AIMessage[] messages;
        public bool stream;
        public int max_tokens;
        public bool enable_thinking;
        public int thinking_budget;
        public float min_p;
        public float temperature;
        public float top_p;
        public int top_k;
        public float frequency_penalty;
        public int n;
        public string[] stop;
    }

    [Serializable]
    public class AIMessage
    {
        public string role;
        public string content;
    }

    [Serializable]
    public class AIResponse
    {
        public Choice[] choices;
    }

    [Serializable]
    public class Choice
    {
        public AIMessage message;
    }

    [Serializable]
    public class AIJsonReply
    {
        public string text;
        public string option1;
        public string option2;
        public string option3;
    }

    #region AI历史记忆功能实现
    [Serializable]
    public class AIChatHistory
    {
        public List<AIMessage> messages = new List<AIMessage>();

        /// <summary>
        /// 添加一条消息到历史
        /// </summary>
        public void Add(string role, string content)
        {
            messages.Add(new AIMessage { role = role, content = content });
        }

        /// <summary>
        /// 获取最近N条消息（防止历史过长）
        /// </summary>
        public AIMessage[] GetRecent(int maxCount)
        {
            if (messages.Count <= maxCount)
                return messages.ToArray();
            return messages.GetRange(messages.Count - maxCount, maxCount).ToArray();
        }
        /// <summary>
        /// 清空历史
        /// </summary>
        public void Clear()
        {
            messages.Clear();
            Debug.Log("本轮对话记录已清空");
        }
    }
    #endregion


}

