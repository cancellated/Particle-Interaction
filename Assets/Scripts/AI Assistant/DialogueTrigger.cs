using AI.Assistant.API;
using AI.Assistant.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Assistant
{
    public class DialogueTrigger : MonoBehaviour
    {
        [TextArea]
        public string presetInstruction = "";

        private bool hasTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered) return;
            if (other.CompareTag("Player"))
            {
                hasTriggered = true;
                // 通知UI显示对话面板
                GameEvents.TriggerAIDialogueStart();
                // 发送预设指令到AI
                APIManager.Instance.SendTextMessage(presetInstruction, OnAIResponse);
            }
        }

        private void OnAIResponse(string aiReply)
        {
            // 通知UI显示AI回复
            GameEvents.TriggerAIDialogueResponse(aiReply);
        }
    }
}