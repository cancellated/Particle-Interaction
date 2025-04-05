using UnityEngine;

public class AutoCollisionSound : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // 尝试获取碰撞物体上的 EachObjSound 组件
        EachObjSound eachObjSound = collision.gameObject.GetComponent<EachObjSound>();

        if (eachObjSound != null && eachObjSound.collisionSound != null)
        {
            // 播放该立方体指定的音效
            audioSource.PlayOneShot(eachObjSound.collisionSound);
        }
    }
}
