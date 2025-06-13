using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using AI.Assistant.Data;
using StarterAssets;


/// <summary>
/// AIChat 负责管理AI对话UI的显示、选项按钮交互以及与事件系统的通信
/// </summary>
public class AIChat : MonoBehaviour
{
    #region 参数配置
    [Header("UI组件")]
    public CanvasGroup ChatGroup; //对话系统UI界面
    public CanvasGroup OptionGroups; //选项
    public Text dialogueText; // 显示AI回复文本
    public Button option1Button; // 选项1按钮
    public Button option2Button; // 选项2按钮
    public Button option3Button; // 选项3按钮
    public Button endButton;     // 结束对话按钮

    public float typingSpeed = 0.05f; // 打字机效果速度

    private Coroutine typingCoroutine; // 当前打字协程
    private bool isTyping = false;     // 是否正在打字
    private AIJsonReply currentReply;  // 当前AI回复数据
    [Header("玩家输入")]
    public Player playerInput;

    public StarterAssetsInputs starterAssetsInputs;
    #endregion

    #region 生命周期管理
    void Start()
    {
        // 绑定按钮点击事件
        option1Button.onClick.AddListener(() => OnOptionSelected(option1Button.GetComponentInChildren<Text>().text));
        option2Button.onClick.AddListener(() => OnOptionSelected(option2Button.GetComponentInChildren<Text>().text));
        option3Button.onClick.AddListener(() => OnOptionSelected(option3Button.GetComponentInChildren<Text>().text));
        endButton.onClick.AddListener(OnEndDialogue);
        ChatGroup.gameObject.SetActive(false);  //初始隐藏
    }

    void Awake()
    {
        GameEvents.OnAIDialogueStart += HandleDialogueStart;
        GameEvents.OnAIDialogueResponse += HandleDialogueResponse;
    }

    void OnDestroy()
    {
        GameEvents.OnAIDialogueStart -= HandleDialogueStart;
        GameEvents.OnAIDialogueResponse -= HandleDialogueResponse;
    }
    #endregion

    #region 事件处理
    /// <summary>
    /// 对话开始时禁用玩家控制器并解锁光标
    /// </summary>
    private void HandleDialogueStart()
    {
        ChatGroup.gameObject.SetActive(true);
        OptionGroups.gameObject.SetActive(false);

        if (playerInput != null)
        {
            playerInput.GamePlay.Disable();
            playerInput.Menu.Enable();

            // 进入对话时
            // if (playerInput != null)
            // {
            //     playerInput.GamePlay.Disable();
            //     playerInput.Menu.Enable();
            // }
            // Cursor.lockState = CursorLockMode.None;
            // Cursor.visible = true;

            // 禁用第一人称控制器脚本
            if (starterAssetsInputs != null)
            {
                starterAssetsInputs.enabled = false;
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;



            // 结束对话时
            // if (playerInput != null)
            // {
            //     playerInput.Menu.Disable();
            //     playerInput.GamePlay.Enable();
            // }
            // Cursor.lockState = CursorLockMode.Locked;
            // Cursor.visible = false;

            // 启用第一人称控制器脚本
            if (starterAssetsInputs != null)
            {
                starterAssetsInputs.enabled = true;
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    

    /// <summary>
    /// 处理AI回复事件，解析JSON并启动打字机效果
    /// </summary>
    /// <param name="message">AI返回的消息内容</param>
    private void HandleDialogueResponse(string message)
    {
        AIJsonReply reply = null;
        try
        {
            reply = JsonUtility.FromJson<AIJsonReply>(message);
        }
        catch
        {
            Debug.LogWarning("AI回复不是有效的JSON格式，直接显示原文。");
        }

        currentReply = reply;

        // 如果正在打字，先停止
        if (isTyping && typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            isTyping = false;
        }

        // 启动打字机效果
        if (reply != null && !string.IsNullOrEmpty(reply.text))
        {
            typingCoroutine = StartCoroutine(TypeText(reply.text, typingSpeed, reply));
        }
        else
        {
            typingCoroutine = StartCoroutine(TypeText(message, typingSpeed, null));
        }
    }
    /// <summary>
    /// 选项按钮点击事件，发送选项内容到AI
    /// </summary>
    private void OnOptionSelected(string optionText)
    {
        SetOptionsActive(false);
        // 通过事件系统发送用户选项
        GameEvents.TriggerAIDialogueSend(optionText);
    }

    /// <summary>
    /// 结束按钮点击事件，通知对话结束
    /// </summary>
    private void OnEndDialogue()
    {
        ChatGroup.gameObject.SetActive(false);
        GameEvents.TriggerAIDialogueComplete();
        if (playerInput != null)
        {
            playerInput.Menu.Disable();
            playerInput.GamePlay.Enable();
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    #endregion

    #region 解析AI回复并打出文字

    /// <summary>
    /// 打字机效果显示文本，结束后显示选项
    /// </summary>
    private IEnumerator TypeText(string text, float speed, AIJsonReply reply)
    {
        isTyping = true;
        dialogueText.text = "";
        SetOptionsActive(false);

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(speed);
        }

        isTyping = false;

        // 打字结束后显示选项
        if (reply != null)
        {
            option1Button.GetComponentInChildren<Text>().text = reply.option1;
            option2Button.GetComponentInChildren<Text>().text = reply.option2;
            option3Button.GetComponentInChildren<Text>().text = reply.option3;
            SetOptionsActive(true);
        }
        else
        {
            SetOptionsActive(false);
        }
    }

    /// <summary>
    /// 控制选项按钮和结束按钮的显示/隐藏
    /// </summary>
    private void SetOptionsActive(bool active)
    {
        OptionGroups.gameObject.SetActive(active);
    }
    #endregion
    void Update()
    {
        // 只有在对话界面激活且选项组激活时才响应按键
        if (ChatGroup.gameObject.activeSelf && OptionGroups.gameObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                option1Button.onClick.Invoke();
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                option2Button.onClick.Invoke();
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                option3Button.onClick.Invoke();
                return;
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                endButton.onClick.Invoke();
                return;
            }
        }
    }
}
