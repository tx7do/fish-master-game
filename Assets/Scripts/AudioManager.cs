using UnityEngine;

/**
 * 音效和bgm控制类
 * 挂在各个scene的ScriptHolder上
 **/
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public bool IsMute { get; private set; }

    public AudioSource bgmAudioSource; // 背景音乐
    public AudioClip seaWaveClip; // 音效
    public AudioClip goldClip;
    public AudioClip rewardClip;
    public AudioClip fireClip;
    public AudioClip changeClip;
    public AudioClip lvUpClip;

    protected void Awake()
    {
        Instance = this;
        IsMute = (PlayerPrefs.GetInt("mute", 0) != 0);
        DoMute();
    }

    public void SwitchMuteState(bool isOn)
    {
        IsMute = !isOn;
        DoMute();
    }

    private void DoMute()
    {
        if (IsMute)
        {
            bgmAudioSource.Pause(); // 背景音乐pause
        }
        else
        {
            bgmAudioSource.Play(); // 背景音乐播放
        }
    }

    public void PlayEffectSound(AudioClip clip)
    {
        if (!IsMute)
        {
            AudioSource.PlayClipAtPoint(clip, new Vector3(0, 0, -5)); // 播放音效
        }
    }
}