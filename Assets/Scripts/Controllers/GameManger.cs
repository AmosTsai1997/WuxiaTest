using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections; // 必須引用這個才能使用協程

public class GameManager : MonoBehaviour
{   
    public Health playerHealth; // 在 Inspector 把大俠拖進來
    public GameObject tutorialPanel; // 拖入你的 TutorialPanel
    public Button startButton;      // 拖入你的 StartButton
    public static GameManager Instance;

    [Header("玩家設定")]
    public GameObject playerPrefab;
    public Vector3 villageSpawnPos = new Vector3(25f, 7f, 27f); // 抬高 Y 軸防止掉落
    public Vector3 largeMapSpawnPos = new Vector3(440f, 25f, 415f);

    [Header("初始地圖重生設定")]
    public GameObject villageEnemyPrefab;
    public Vector3 villageEnemyPos = new Vector3(20f, 2f, 30f);
    public float villageRespawnTime = 60f;

    [Header("大地圖隨機生成設定")]
    // 修改：改成 GameObject 陣列，這樣可以在 Inspector 設定數量（例如設定為 2，各放一種怪）
    public GameObject[] largeMapEnemyPrefabs;
    public Vector3 largeMapCenter = new Vector3(444f, 22f, 410f);
    public float spawnRadius = 50f; // 生成半徑，根據你的地圖大小調整
    public float largeMapRespawnTime = 60f;

    private Coroutine villageTimer;
    private Coroutine largeMapTimer;
    
    [Header("Boss生成設定")]
    public GameObject bossPrefab;
    public Vector3 bossSpawnPosition = new Vector3(432f, 30f, 432f);

    void Awake()
    {
        // 單例查重必須放最前面
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return; // 我是複製品，到此為止，不准碰下面的東西
        }
        // 1. 遊戲一啟動就凍結時間
        Time.timeScale = 0f;
        // 2. 確保提示視窗是開啟的
        if (tutorialPanel != null) tutorialPanel.SetActive(true);
        // 3. 綁定按鈕點擊事件
        if (startButton != null) startButton.onClick.AddListener(CloseTutorial);
    }

    void Update()
    {
        // 1. 動態尋找場景中的大俠
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            Health health = player.GetComponent<Health>();

            // 2. 如果大俠死透了，且按下 R
            if (health != null && health.isDead)
            {
                // 畫面上可以印個提示，確認 GameManager 有在跑
                // Debug.Log("偵測到大俠死亡，等待按下 R 重生..."); 

                if (Input.GetKeyDown(KeyCode.R))
                {
                    Debug.Log("重生中...");
                    RestartGame();
                }
            }
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
        // 停止所有正在運行的生成計時器，避免跨場景疊加
        StopAllCoroutines();
        // --- 決定初始場景的出生點 ---
        Vector3 spawnPos = villageSpawnPos;
        Debug.Log($"場景 {scene.name} 載入完成！");

        if (scene.name == "LargeMap")
        {
            spawnPos = largeMapSpawnPos;
            SpawnQuestEnemies();
            // 啟動大地圖隨機生成計時器
            largeMapTimer = StartCoroutine(LargeMapSpawnRoutine());
        }
        else
        {
            // 啟動初始地圖固定點生成計時器
            villageTimer = StartCoroutine(VillageSpawnRoutine());
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) 
        {
            HandlePlayerSpawn(spawnPos);

            // 死亡重生
            Health h = player.GetComponent<Health>();
            if (h != null && h.isDead) 
            {
                h.ResetHealth();
                // 建議：將玩家移回出生點，防止卡在死掉的位置
                HandlePlayerSpawn(spawnPos);
                // 恢復控制權
                HeroActions ha = player.GetComponent<HeroActions>();
                if (ha != null) ha.enabled = true;
            }
        }
    }
    
    // --- 初始地圖：固定點重生 ---
    IEnumerator VillageSpawnRoutine()
    {
        while (true)
        {
            if (villageEnemyPrefab != null) Instantiate(villageEnemyPrefab, villageEnemyPos, Quaternion.identity);
            yield return new WaitForSeconds(villageRespawnTime);
        }
    }

    // --- 大地圖：隨機範圍重生 ---
    IEnumerator LargeMapSpawnRoutine()
    {
       while (true)
        {
            // 檢查陣列是否有怪且不為空
            if (largeMapEnemyPrefabs != null && largeMapEnemyPrefabs.Length > 0)
            {
                // 1. 隨機選取陣列中的其中一個敵人 Prefab
                int randomIndex = Random.Range(0, largeMapEnemyPrefabs.Length);
                GameObject selectedEnemy = largeMapEnemyPrefabs[randomIndex];

                if (selectedEnemy != null)
                {
                    // 2. 計算隨機位置
                    Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                    Vector3 randomSpawnPos = new Vector3(
                        largeMapCenter.x + randomCircle.x,
                        largeMapCenter.y ,
                        largeMapCenter.z + randomCircle.y
                    );

                    // 3. 生成選中的怪
                    Instantiate(selectedEnemy, randomSpawnPos, Quaternion.identity);
                    Debug.Log($"大地圖生成了：{selectedEnemy.name} 於 {randomSpawnPos}");
                }
            }
            yield return new WaitForSeconds(largeMapRespawnTime);
        }
    }

    void HandlePlayerSpawn(Vector3 pos)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null && playerPrefab != null)
        {
            player = Instantiate(playerPrefab, pos, Quaternion.identity);
        }
        else if (player != null)
        {   
            SimpleGravity sg = player.GetComponent<SimpleGravity>();
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false; 
            player.transform.position = pos;
            if (cc != null) cc.enabled = true;
            if (sg != null) // 加上 null 檢查，身上沒掛 SimpleGravity 也不會報錯
            {
                sg.gravity = 9.81f;
                sg.verticalVelocity = 0f; // 【新增】清掉累積的下墜速度。
                // 沒這行的話，如果玩家是在「墜落中」被傳送，
                // 會帶著超高速度瞬間砸穿新場景的地板
            }
        }
    }

    public void RestartGame()
    {
        // 1. 取得目前場景名稱
        string currentSceneName = SceneManager.GetActiveScene().name;

        // 2. 執行場景載入
        SceneManager.LoadScene(currentSceneName);
    }
    void SpawnQuestEnemies()
    {   
        if (bossPrefab == null) return;
        GameObject boss = Instantiate(bossPrefab, bossSpawnPosition, Quaternion.identity);
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            LevelSystem pLevel = player.GetComponent<LevelSystem>();
            LevelSystem bLevel = boss.GetComponent<LevelSystem>();
            if (pLevel != null && bLevel != null) bLevel.SetLevel(pLevel.currentLevel);
        }
    }
}