using UnityEngine;
using TMPro; // 記得引入

public class NPCInteraction : MonoBehaviour
{
    [Header("數值設定")]
    private Health healthScript; // 引用 Health 腳本
    private Camera mainCamera;
    private Quaternion originalCameraRotation;
    public Transform cameraTarget;
    private bool isTalking = false;
    public int xpValue = 20;

    [Header("UI 元件")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject actionButtons; // 包含「接受」與「拒絕」的按鈕組

    [Header("NPC 設定")]
    public string npcName = "神祕少女";
    public string questInquiry = "這位少俠，請幫幫我？";

    [Header("任務設定")]
    public GameObject enemyPrefab;    // 拖入你的強大敵人 Prefab
    public Vector3 spawnPosition = new Vector3(27f, 5f, 32f);
    private bool isPlayerNearby = false;

    void Start()
    {   
        dialoguePanel.SetActive(false); // 預設關閉對話框
        mainCamera = Camera.main;
        healthScript = GetComponent<Health>();
    }

    void Update()
    {
        // 玩家在附近且按下 F 鍵觸發對話
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
        {
            // 如果 NPC 還活著才能對話
            if (healthScript != null && healthScript.currentHP > 0)
            {
                ToggleDialogue();
            }
        }
        // 檢查是否死亡（直接問 Health 腳本）
        if (healthScript != null && healthScript.currentHP <= 0 && dialoguePanel.activeSelf)
        {
            PerformDeath();
            dialoguePanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        // 當正在對話時，讓相機平滑看向 NPC
        if (isTalking)
        {
            FocusCameraOnNPC();
        }
    }
    void FocusCameraOnNPC()
    {
        // 計算從相機到 NPC 的方向（稍微往上看一點，對準臉部）
        Vector3 targetDir = (transform.position + Vector3.up * 1.5f) - mainCamera.transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(targetDir);
        
        // 平滑轉向
        mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRotation, Time.deltaTime * 5f);
    }
    void ToggleDialogue()
    {
        bool isActive = !dialoguePanel.activeSelf;
        dialoguePanel.SetActive(isTalking);

        if (isTalking)
        {   
            // 讓相機緩慢轉向 NPC
            Camera.main.transform.LookAt(transform.position + Vector3.up * 1.5f);
            originalCameraRotation = mainCamera.transform.rotation;
            dialogueText.text = $"【{npcName}】: {questInquiry}";
            actionButtons.SetActive(true); // 顯示任務選項
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            EndDialogue();
        }
    }
public void OnAcceptQuest()
    {
        dialogueText.text = $"【{npcName}】: 萬事拜託了！他在座標 (20, 2, 20) 等著你。";
        actionButtons.SetActive(false);
        
        // 生成強大敵人
        if (enemyPrefab != null)
        {
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            Debug.Log("強大敵人已降臨！");
        }
        
        // 延遲兩秒關閉對話框
        Invoke("EndDialogue", 2f);
    }

    public void OnDeclineQuest()
    {
        dialogueText.text = $"【{npcName}】: 既然少俠另有要務，在下也不便強求...";
        actionButtons.SetActive(false);
        Invoke("EndDialogue", 2f);
    }

    void EndDialogue()
    {
        isTalking = false;
        dialoguePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // 觸發偵測
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("偵測到物體進入範圍：" + other.name);
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            Debug.Log("靠近 NPC，按 F 對話");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("你按下了 F 鍵！");
            isPlayerNearby = false;
            dialoguePanel.SetActive(false);
            // 離開範圍時確保滑鼠被鎖定（防止對話框強制開啟時離開）
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    public void TakeDamage(float amount)
    {
        // 不要自己扣血，叫 Health 腳本去扣
        if (healthScript != null)
        {
            healthScript.TakeDamage(amount); 
        }
    }
    void PerformDeath()
        {
            // 處理 NPC 死亡邏輯（例如播動畫、消失）
            Debug.Log(gameObject.name + " 倒下了");
            Destroy(gameObject, 2f); // 2秒後移除
        }
}