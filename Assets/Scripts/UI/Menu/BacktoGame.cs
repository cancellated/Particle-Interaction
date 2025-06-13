using UnityEngine;

namespace UI.Menu.Button
{
    /// <summary>
    /// 返回游戏按钮，点击时关闭菜单
    /// </summary>
    public class ReturnButton : MonoBehaviour
    {
        /// <summary>
        /// 按钮点击事件，关闭菜单
        /// </summary>
        public void OnReturnButtonClick()
        {
            GameEvents.TriggerMenuSet(false);
        }
    }
}
