using UnityEngine;
using TMPro; // 記得引入

public class NPCInteraction : MonoBehaviour
{
    [Header("數值設定")]
    private Health healthScript; // 引用 Health 腳本
    public int xpValue = 20;

    [Header("UI 元件")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TMP_InputField playerInput;

    [Header("NPC 設定")]
    public string npcName = "神祕少女";
    public string defaultGreeting = "這位少俠，找我有何事？";
    private bool isPlayerNearby = false;

    void Start()
    {   
        dialoguePanel.SetActive(false); // 預設關閉對話框
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
    }

    void ToggleDialogue()
    {
        bool isActive = !dialoguePanel.activeSelf;
        dialoguePanel.SetActive(isActive);

        if (isActive)
        {
            dialogueText.text = $"【{npcName}】: {defaultGreeting}";
            // 開啟對話時解鎖滑鼠，方便輸入
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {   
            // 關閉對話時鎖定滑鼠
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
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