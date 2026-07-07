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
    public float seVolume = 1.0f;  // 音效音量

    void Awake()
    {
        if( Instance != null && Instance !=this )
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        backGroundMusic = gameObject.AddComponent<AudioSource>();
        backGroundMusic.loop = true;
        backGroundMusic.playOnAwake = false;

        soundEffect = gameObject.AddComponent<AudioSource>();
        soundEffect.loop = false;
        soundEffect.playOnAwake = false;

    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (backGroundMusic.clip == clip && backGroundMusic.isPlaying)
            return;
        backGroundMusic.clip = clip;
        backGroundMusic.volume = bgmVolume;
        backGroundMusic.Play();
    }
    public void StopBGM()
    {
        backGroundMusic.Stop();
    }

    public void PlaySE(AudioClip clip)
    {
        if (clip == null) return;
        soundEffect.PlayOneShot(clip, seVolume);
    }
}
