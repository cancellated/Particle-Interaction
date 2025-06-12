using AI.Assistant.API;
using UnityEngine;


public class AIDialogueTrigger : MonoBehaviour
{
    public string triggerMessage; // 进入该区域时AI要处理的消息

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 触发AI对话开始事件（显示“思考中...”）
            GameEvents.TriggerAIDialogueStart();

            // 调用APIManager发送消息
            APIManager.Instance.SendTextMessage(
                triggerMessage,
                (response) =>
                {
                    // 传递AI的回复
                    GameEvents.TriggerAIDialogueResponse(response);
                },
                (error) =>
                {
                    GameEvents.TriggerAIDialogueResponse("发生错误：" + error);
                    GameEvents.TriggerAIDialogueComplete();
                }
            );

            this.enabled = false; // 只触发一次
        }
    }
}
