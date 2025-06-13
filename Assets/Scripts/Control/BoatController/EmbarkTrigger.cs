using UnityEngine;
using System;

namespace Boat.Embark
{
    /// <summary>
    /// 只负责检测上下船触发条件，通过事件通知管理器
    /// </summary>
    public class EmbarkTrigger : MonoBehaviour
    {
        // 上船触发事件
        public event Action OnCanEmbark;
        // 下船触发事件
        public event Action OnCanDisembark;
        // 离开上船区事件
        public event Action OnEmbarkExit;
        // 离开下船区事件
        public event Action OnDisembarkExit;

        // 由管理器同步当前是否在船上
        public bool onBoat = false;

        void OnTriggerEnter(Collider other)
        {
            // 玩家接近船，且当前不在船上
            if (other.CompareTag("Boat") && !onBoat)
            {
                OnCanEmbark?.Invoke();
            }
            // 乘船时，船接近建筑
            else if (onBoat && other.CompareTag("Building"))
            {
                OnCanDisembark?.Invoke();
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Boat") && !onBoat)
            {
                OnEmbarkExit?.Invoke();
            }
            else if (onBoat && other.CompareTag("Building"))
            {
                OnDisembarkExit?.Invoke();
            }
        }
    }
}
