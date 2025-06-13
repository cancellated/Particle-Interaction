using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu {
    public class Menu : MonoBehaviour
    {
        public GameObject menuUI; // 菜单UI面板
        public Player playerInput; // Player输入系统实例

        void OnEnable()
        {
            GameEvents.OnMenuSet += OnMenuSet;
        }

        void OnDisable()
        {
            GameEvents.OnMenuSet -= OnMenuSet;
        }

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

        /// <summary>
        /// 返回游戏按钮调用，隐藏菜单
        /// </summary>
        public void HideMenu()
        {
            GameEvents.TriggerMenuSet(false);
        }
    }
}