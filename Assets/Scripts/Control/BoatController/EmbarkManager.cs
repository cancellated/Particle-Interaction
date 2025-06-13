using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Boat.Embark
{

    /// <summary>
    /// 管理上下船交互与控制权切换，使用Input System的Embark操作
    /// </summary>
    public class EmbarkManager : MonoBehaviour
    {
        #region 数据配置
        // 上下船相关数据上下文，便于数据共享和解耦
        public EmbarkContext context = new();

        // 是否可以上船
        private bool canEmbark = false;
        // 是否可以下船
        private bool canDisembark = false;
        // 当前是否在船上
        private bool onBoat = false;

        // 输入系统实例
        private Player inputActions = new();
        #endregion

        #region 生命周期管理

        void OnEnable()
        {
            // 启用船相关输入，并注册Embark事件
            inputActions.Boat.Enable();
            inputActions.Boat.Embark.performed += OnEmbark;
        }

        void OnDisable()
        {
            // 注销Embark事件，禁用输入
            inputActions.Boat.Embark.performed -= OnEmbark;
            inputActions.Boat.Disable();
        }
        #endregion

        #region 事件处理

        /// <summary>
        /// 上下船操作响应
        /// </summary>
        private void OnEmbark(InputAction.CallbackContext ctx)
        {
            if (canEmbark && !onBoat)
            {
                // 上船：切换控制器，移动玩家到船上指定点，隐藏提示
                context.player.transform.position = context.playerOnBoatPoint.position;
                context.playerController.SetActive(false);
                context.boatController.SetActive(true);
                onBoat = true;
                context.interactButton.gameObject.SetActive(false);
                GameEvents.TriggerEmbark();
            }
            else if (canDisembark && onBoat)
            {
                // 下船：切换控制器，隐藏提示
                context.playerController.SetActive(true);
                context.boatController.SetActive(false);
                onBoat = false;
                context.interactButton.gameObject.SetActive(false);
                GameEvents.TriggerDisembark();
            }
        }

        /// <summary>
        /// 进入触发区域，判断是否可以上/下船并显示提示
        /// </summary>
        void OnTriggerEnter(Collider other)
        {
            // 玩家接近船，且当前不在船上
            if (other.gameObject == context.player && !onBoat)
            {
                canEmbark = true;
                context.interactButton.gameObject.SetActive(true);
                context.interactButton.GetComponentInChildren<Text>().text = "按E上船";
            }
            // 船接近玩家（或其他下船判定），且当前在船上
            if (other.gameObject == context.boat && onBoat)
            {
                canDisembark = true;
                context.interactButton.gameObject.SetActive(true);
                context.interactButton.GetComponentInChildren<Text>().text = "按E下船";
            }
        }

        /// <summary>
        /// 离开触发区域，取消上/下船提示
        /// </summary>
        void OnTriggerExit(Collider other)
        {
            if (other.gameObject == context.player && !onBoat)
            {
                canEmbark = false;
                context.interactButton.gameObject.SetActive(false);
            }
            if (other.gameObject == context.boat && onBoat)
            {
                canDisembark = false;
                context.interactButton.gameObject.SetActive(false);
            }
        }
        #endregion
    }

    /// <summary>
    /// 上下船相关数据上下文，便于数据共享和解耦
    /// </summary>
    public class EmbarkContext
    {
        // 玩家对象
        public GameObject player;
        // 船对象
        public GameObject boat;
        // 玩家上船时应到达的位置
        public Transform playerOnBoatPoint;
        // 玩家控制器
        public GameObject playerController;
        // 船控制器
        public GameObject boatController;
        // 交互提示按钮
        public Button interactButton;
    }
}