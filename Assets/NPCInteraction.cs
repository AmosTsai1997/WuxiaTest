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
        int playerLevel = player.GetComponent<LevelSystem>().currentLevel;
        
        dialogueText.text = $"【{npcName}】: 喔！看來少俠已達 {playerLevel} 等，村外的敵人也變強了，請小心！";
        if (actionButtons != null) actionButtons.SetActive(false);

    // 生成普通敵人並同步等級
    if (enemyPrefab != null) // Vector3 不能檢查 null，直接檢查 Prefab 即可
    {
        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        
        // 直接修改組件內的數值
        spawnedEnemy.GetComponent<LevelSystem>().SetLevel(playerLevel);
        
        Debug.Log($"生成了 {playerLevel} 等的強力敵人！");
    }

    // 生成 Boss 並同步等級
    if (bossPrefab != null)
    {
        GameObject spawnedBoss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        
        spawnedBoss.GetComponent<LevelSystem>().SetLevel(playerLevel);
        
        Debug.Log($"哥布林 Boss (等級: {playerLevel}) 已降臨！");
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
    {   // 1. 直接在大俠身上找 LevelSystem
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {   
            // 2. 嘗試取得等級系統組件
            LevelSystem lvSys = player.GetComponent<LevelSystem>();

            if (lvSys != null)
            {
                // 3. 呼叫公開方法傳入 500 經驗值
                lvSys.AddXP(500);
                Debug.Log("任務完成！成功發放 500 經驗值給大俠。");
            }
            else
            {
                Debug.LogError("找不到 LevelSystem 組件，請確認 Player 物件上有掛載腳本。");
            }
        }
    }
}