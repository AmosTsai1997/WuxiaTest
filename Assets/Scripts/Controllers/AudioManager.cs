using UnityEngine;

public class AudioManager:MonoBehaviour
{
    public static AudioManager Instance;
    private AudioSource backGroundMusic;
    private AudioSource soundEffect;

    [Header("背景音樂設定")]
    public AudioClip startBGM;

    [Range(0f,1f)]
    public float bgmVolume = 0.5f;

    [Header("音效設定")]
    [Range(0f, 1f)]
    public float sfxVolume = 1.0f;  // 音效音量

    void Awake()
    {
        if( Instance != null && Instance !=this )
        {
            Destroy(gameObject);
            return;
        }
    }
}
