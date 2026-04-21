using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections; // 必須引用這個才能使用協程

public class GameManager : MonoBehaviour
{
    public GameObject tutorialPanel; // 拖入你的 TutorialPanel
    public Button startButton;      // 拖入你的 StartButton
    public GameObject enemyPrefab;
    public static GameManager Instance;

    [Header("玩家設定")]
    public GameObject playerPrefab;
    public Vector3 defaultSpawnPosition = new Vector3(25f, 5f, 27f); // 抬高 Y 軸防止掉落
    
    [Header("小怪重生設定")]
    public Vector3 enemySpawnPos = new Vector3(29f, 2f, 30f); // 初始地圖小怪出生點
    public float respawnInterval = 60f; // 重生間隔（秒）
    private Coroutine enemyTimer; // 用來記錄協程，方便切換場景時停止
    
    [Header("Boss生成設定")]
    public GameObject bossPrefab;
    public Vector3 bossSpawnPosition = new Vector3(432f, 30f, 432f);

    void Awake()
    {
        // 1. 遊戲一啟動就凍結時間
        Time.timeScale = 0f;
        
        // 2. 確保提示視窗是開啟的
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }

        // 3. 綁定按鈕點擊事件
        if (startButton != null)
        {
            startButton.onClick.AddListener(CloseTutorial);
        }

        // 確保切換場景時，這個 Manager 不會消失
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void CloseTutorial()
    {
        // 4. 恢復時間流逝
        Time.timeScale = 1f;

        // 5. 關閉視窗
        tutorialPanel.SetActive(false);
        
        Debug.Log("遊戲正式開始！");
    }

    private void OnEnable()
    {
        // 註冊場景載入事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 取消註冊，防止記憶體洩漏
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 每次載入場景後會自動執行此函式
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"場景 {scene.name} 載入完成！");

        // --- 決定當前場景的出生點 ---
        Vector3 spawnPos = defaultSpawnPosition; // 預設為 (27, 5, 30)

        if (scene.name == "LargeMap")
        {
            spawnPos = new Vector3(429f, 28f, 429f); // 大地圖的特定座標
            
            // 進入大地圖時，停止初始地圖的小怪重生計時
            if (enemyTimer != null) StopCoroutine(enemyTimer);
            
            SpawnQuestEnemies();
        }
        else
        {
            // 如果回到初始地圖，啟動每分鐘重生小怪的計時器
            if (enemyTimer != null) StopCoroutine(enemyTimer); // 先停止舊的，防止疊加
            enemyTimer = StartCoroutine(RespawnEnemyRoutine());
        }

        // 1. 處理玩家生成與位置設定
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null && playerPrefab != null)
        {
            // 如果場景中沒有大俠，就生成一個
            player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        }
        else if (player != null)
        {
            // 如果大俠已存在（DontDestroyOnLoad），強行校準到該場景的出生點
            CharacterController cc = player.GetComponent<CharacterController>();
            
            // 關鍵：必須先關閉 CC 才能移動位置，否則會被物理引擎拉回原點
            if (cc != null) cc.enabled = false; 
            
            player.transform.position = spawnPos;
            
            if (cc != null) cc.enabled = true;
        }
    }
    // --- 小怪重生協程 ---
    IEnumerator RespawnEnemyRoutine()
    {
        while (true) // 無限循環
        {
            // 在指定位置生成小怪
            if (enemyPrefab != null)
            {
                Instantiate(enemyPrefab, enemySpawnPos, Quaternion.identity);
                Debug.Log("一分鐘到了，新的小怪已在初始地圖重生！");
            }
            
            // 等待 60 秒
            yield return new WaitForSeconds(respawnInterval);
        }
    }
    void SpawnQuestEnemies()
    {   
        if (bossPrefab == null) return;

        GameObject boss = Instantiate(bossPrefab, bossSpawnPosition, Quaternion.identity);
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            LevelSystem playerLevelSys = player.GetComponent<LevelSystem>();
            LevelSystem bossLevelSys = boss.GetComponent<LevelSystem>();

            // 修正：確保兩者組件都在，且 boss 物件已生成
            if (playerLevelSys != null && bossLevelSys != null)
            {
                int pLevel = playerLevelSys.currentLevel;
                bossLevelSys.SetLevel(pLevel);
                Debug.Log($"已動態生成等級 {pLevel} 的哥布林 Boss");
            }
        }
    }
}