using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AIChat : MonoBehaviour
{
    [Header("UI References")]
    public Text dialogueContent;
    public GameObject dialoguePanel;
    [Header("Display Settings")]
    public float typingSpeed = 0.05f;
    public float fastDisplaySpeed = 0.01f;
    
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    private void OnEnable()
    {
        // 订阅对话事件
        GameEvents.OnAIDialogueStart += OnDialogueStart;
        GameEvents.OnAIDialogueResponse += OnDialogueResponse; 
        GameEvents.OnAIDialogueComplete += OnDialogueComplete;
    }

    private void OnDisable()
    {
        // 取消订阅
        GameEvents.OnAIDialogueStart -= OnDialogueStart;
        GameEvents.OnAIDialogueResponse -= OnDialogueResponse;
        GameEvents.OnAIDialogueComplete -= OnDialogueComplete;
    }

    private void OnDialogueStart()
    {
        // 显示对话UI
        dialoguePanel.SetActive(true);
        dialogueContent.text = "思考中...";
    }

    private void OnDialogueResponse(string message)
    {
        // 停止正在进行的打字效果
        if(isTyping)
        {
            StopCoroutine(typingCoroutine);
            isTyping = false;
        }
        
        // 开始新的打字效果
        typingCoroutine = StartCoroutine(TypeText(message, typingSpeed));
    }

    //对话文本打字机效果
    private IEnumerator TypeText(string text, float speed)
    {
        isTyping = true;
        dialogueContent.text = "";
        
        foreach (char letter in text.ToCharArray())
        {
            dialogueContent.text += letter;
            yield return new WaitForSeconds(speed);
        }
        
        isTyping = false;
    }

    // 快速显示完整内容（可由外部调用）
    public void ShowTextImmediately(string text)
    {
        if(isTyping)
        {
            StopCoroutine(typingCoroutine);
            isTyping = false;
        }
        dialogueContent.text = text;
    }

    private void OnDialogueComplete()
    {
        // 隐藏对话UI
        dialoguePanel.SetActive(false);
    }
}
