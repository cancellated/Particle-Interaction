using UnityEngine;

public class RaycastAudioTrigger : MonoBehaviour
{
    void Update()
    {
        // 检测鼠标左键点击
        if (Input.GetMouseButtonDown(0))
        {
            // 从主相机发射射线
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 射线检测
            if (Physics.Raycast(ray, out hit))
            {
                // 检查击中的物体是否有AudioSource组件
                AudioSource audioSource = hit.collider.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    // 播放音频
                    audioSource.Play();
                }
            }
        }
    }
}