using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class InteractableObject : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void OnHitByRay()
    {
        if (audioSource != null)
        {
            audioSource.Stop(); // 先停止，确保可以重复播放
            audioSource.Play(); // 播放AudioSource里选定的音频
        }
    }
}
