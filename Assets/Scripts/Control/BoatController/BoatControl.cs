using UnityEngine;
using UnityEngine.InputSystem;

namespace Boat.Controller
{
    /// <summary>
    /// BoatController 负责处理船只的移动与转向，结合新输入系统（Input System）
    /// 通过事件驱动启用/禁用控制
    /// </summary>
    public class BoatController : MonoBehaviour
    {
        #region 参数配置
        public float speed = 5f;         // 船只前进/后退速度
        public float turnSpeed = 50f;    // 船只转向速度

        private Player player;           // 输入系统自动生成的输入包装类
        private Vector2 moveInput;       // 存储前后移动输入（y分量）
        private float steerInput;        // 存储转向输入（Steer轴）
        #endregion

        #region 生命周期管理
        void Awake()
        {
            // 初始化输入系统并注册输入事件
            player = new Player();

            // 监听Move输入，记录前后移动值
            player.Boat.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            player.Boat.Move.canceled += ctx => moveInput = Vector2.zero;

            // 监听Steer输入，记录转向值
            player.Boat.Steer.performed += ctx => steerInput = ctx.ReadValue<float>();
            player.Boat.Steer.canceled += ctx => steerInput = 0f;

            // 默认禁用输入
            player.Boat.Disable();
        }

        void OnEnable()
        {
            // 订阅上下船事件
            GameEvents.OnEmbark += EnableBoatControl;
            GameEvents.OnDisembark += DisableBoatControl;
        }

        void OnDisable()
        {
            // 取消订阅上下船事件
            GameEvents.OnEmbark -= EnableBoatControl;
            GameEvents.OnDisembark -= DisableBoatControl;
            // 确保输入被禁用
            player.Boat.Disable();
            player.GamePlay.Enable();
        }
        #endregion

        void Update()
        {
            // 只有启用时才响应输入
            if (!player.Boat.enabled) return;

            // 根据输入控制船只前后移动
            float move = moveInput.y * speed * Time.deltaTime;
            // 根据输入控制船只左右转向
            float turn = steerInput * turnSpeed * Time.deltaTime;

            // 执行移动和转向
            transform.Translate(Vector3.forward * move);
            transform.Rotate(Vector3.up * turn);
        }

        /// <summary>
        /// 启用船只控制
        /// </summary>
        private void EnableBoatControl()
        {
            player.Boat.Enable();
            player.GamePlay.Disable();
        }

        /// <summary>
        /// 禁用船只控制
        /// </summary>
        private void DisableBoatControl()
        {
            player.Boat.Disable();
            player.GamePlay.Enable();
            moveInput = Vector2.zero;
            steerInput = 0f;
        }
    }
}