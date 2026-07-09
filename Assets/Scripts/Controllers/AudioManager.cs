using UnityEngine;
using UnityEngine.SceneManagement; // 訂閱場景載入事件用

public class AudioManager:MonoBehaviour
{
    public static AudioManager Instance;
    private AudioSource backGroundMusic;
    private AudioSource soundEffect;

    [Header("各場景背景音樂")]
    public AudioClip forestBGM;    // 村莊（Forest）
    public AudioClip largeMapBGM;  // 大地圖
    public AudioClip dungeonBGM;   // 地下城

    [Range(0f,1f)]
    public float bgmVolume = 0.5f;

    [Header("音效設定")]
    [Range(0f, 1f)]
    public float seVolume = 1.0f;  // 音效音量

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 建立播放用的 AudioSource（merge 時曾遺失這段，導致 backGroundMusic 為 null，
        // PlayBGM 在 sceneLoaded 事件中拋出 NullReferenceException，
        // 連帶中斷後面 GameManager.OnSceneLoaded 的玩家出生點設定）
        backGroundMusic = gameObject.AddComponent<AudioSource>();
        backGroundMusic.loop = true;
        backGroundMusic.playOnAwake = false;

        soundEffect = gameObject.AddComponent<AudioSource>();
        soundEffect.loop = false;
        soundEffect.playOnAwake = false;
    }


    void OnEnable()
    {
        // 訂閱「場景載入完成」事件 —— 跟 GameManager 用的是同一個廣播源
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // 退訂，老規矩成對出現
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 進什麼場景放什麼歌。遊戲一開始載入初始場景時也會觸發，
        // 所以開場音樂不用另外寫
        switch (scene.name)
        {
            case "Dungeon":
                PlayBGM(dungeonBGM);
                break;
            case "LargeMap":
                PlayBGM(largeMapBGM);
                break;
            default: // Forest 和其他場景
                PlayBGM(forestBGM);
                break;
        }
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
