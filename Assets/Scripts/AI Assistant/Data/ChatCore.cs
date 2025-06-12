namespace AI.Assistant.Data
{
    using System;
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



}

