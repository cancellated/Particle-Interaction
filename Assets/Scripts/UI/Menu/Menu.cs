using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu {
    public class Menu : MonoBehaviour
    {
        #region 参数配置
        public GameObject menuUI; // 菜单UI面板
        public Player playerInput; // Player输入系统实例
        #endregion

        #region 生命周期管理
        private void Start()
        {
            OnMenuSet(false);
        }
        void OnEnable()
        {
            GameEvents.OnMenuSet += OnMenuSet;
        }

        void OnDisable()
        {
            GameEvents.OnMenuSet -= OnMenuSet;
        }
        #endregion

        #region 事件响应
        /// <summary>
        /// 根据事件参数显示或隐藏菜单
        /// </summary>
        void OnMenuSet(bool show)
        {
            if (menuUI != null)
                menuUI.SetActive(show);

            if (playerInput != null)
            {
                if (show)
                {
                    playerInput.GamePlay.Disable();
                    playerInput.Boat.Disable();
                    playerInput.Menu.Enable();
                }
                else
                {
                    playerInput.Menu.Disable();
                    playerInput.GamePlay.Enable();
                }
            }
        }
        #endregion
    }
}