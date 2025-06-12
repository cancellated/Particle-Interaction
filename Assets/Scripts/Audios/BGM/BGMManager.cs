using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BGMManager 负责循环播放背景音乐列表中的音乐
/// </summary>
public class BGMManager : Singleton<BGMManager>
{
    public AudioClip[] bgmClips; // 背景音乐列表
    public AudioSource audioSource; // 关联的AudioSource组件
    private int currentIndex = 0; // 当前播放的音乐索引

    private void Start()
    {
        // 如果没有音乐或AudioSource未设置，则不播放
        if (bgmClips.Length == 0 || audioSource == null)
            return;

        PlayCurrentBGM();
    }

    private void Update()
    {
        // 如果当前音乐播放完毕，播放下一首
        if (!audioSource.isPlaying)
        {
            NextBGM();
        }
    }

    /// <summary>
    /// 播放当前索引的BGM
    /// </summary>
    private void PlayCurrentBGM()
    {
        audioSource.clip = bgmClips[currentIndex];
        audioSource.Play();
    }

    /// <summary>
    /// 切换到下一首BGM并播放（循环）
    /// </summary>
    private void NextBGM()
    {
        currentIndex = (currentIndex + 1) % bgmClips.Length;
        PlayCurrentBGM();
    }
}
