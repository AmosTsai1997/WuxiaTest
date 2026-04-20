using UnityEngine;
using TMPro;

public class NPCInteraction : MonoBehaviour
{
    [Header("UI 元件")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject actionButtons; // 包含「接受」與「拒絕」的按鈕組

    [Header("NPC 設定")]
    public string npcName = "神祕少女";
    public string questInquiry = "這位少俠，村外有哥布林 Boss 作亂，你能幫幫我嗎？";

    [Header("任務設定 (Eenmy、Boss 生成)")]
    public GameObject enemyPrefab;    // 拖入你的 Enemy Prefab
    public GameObject bossPrefab;    // 拖入你的 Boss Prefab
    public Vector3 spawnPosition = new Vector3(27f, 5f, 32f); //BOSS 出現點
   // public Transform spawnPoint;     // 拖入一個空的 GameObject 作為生成點

    private bool isPlayerNearby = false;
    private bool isTalking = false;

    void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    void Update()
    {
        // 靠近且按下 F，切換對話
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
        {
            ToggleDialogue();
        }

        // 如果正在對話，鏡頭看向 NPC
        if (isTalking)
        {
            FocusCameraOnNPC();
        }
    }

    void ToggleDialogue()
    {
        isTalking = !isTalking; // 切換狀態
        dialoguePanel.SetActive(isTalking);

        if (isTalking)
        {
            dialogueText.text = $"【{npcName}】: {questInquiry}";
            if (actionButtons != null) actionButtons.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            EndDialogue();
        }
    }

    void FocusCameraOnNPC()
    {
        Vector3 targetDir = (transform.position + Vector3.up * 1.5f) - Camera.main.transform.position;
        if (targetDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);
            Camera.main.transform.rotation = Quaternion.Slerp(Camera.main.transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    // 當按下「接受」按鈕時呼叫
    public void OnAcceptQuest()
    {   
        // 1. 找到 Player 並取得等級
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        int playerLevel = 1;
        if (player != null)
        {
            playerLevel = player.GetComponent<HeroActions>().level;
        }

        dialogueText.text = $"【{npcName}】: 喔！看來少俠已達 {playerLevel} 等，村外的敵人也變強了，請小心！";
        if (actionButtons != null) actionButtons.SetActive(false);

        // 2. 生成敵人並同步等級
        if (playerLevel/5 != 0 && enemyPrefab != null && spawnPosition != null)
        {
            GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            // 呼叫敵人的初始化函式，把大俠的等級傳進去
            EnemyLevel eLevel = spawnedEnemy.GetComponent<LevelSystem>();
            if (eLevel != null)
            {
                eLevel.InitializeEnemy(playerLevel);
            }

            Debug.Log($"生成了 {playerLevel} 等的強力敵人！");
        }
        // 生成 Boss
        else 
        {
            if (bossPrefab != null && spawnPosition != null)
            {
                Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
                Debug.Log("哥布林 Boss 已降臨！");
            }
                
        }

        Invoke("EndDialogue", 2f);


    }

    public void OnDeclineQuest()
    {
        dialogueText.text = $"【{npcName}】: 既然少俠另有要務，那就不強求了。";
        if (actionButtons != null) actionButtons.SetActive(false);
        Invoke("EndDialogue", 2f);
    }

    void EndDialogue()
    {
        isTalking = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNearby = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            EndDialogue();
        }
    }

    public void CompleteQuest()
    {
        // 尋找場景中的大俠
        HeroActions hero = GameObject.FindGameObjectWithTag("Player").GetComponent<HeroActions>();
        if (hero != null)
        {
            hero.AddXP(500); // 一次給 500 經驗，讓測試時能快速跳等
        }
    }
}