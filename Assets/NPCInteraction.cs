using UnityEngine;
using TMPro; // 記得引入

public class NPCInteraction : MonoBehaviour
{
    [Header("數值設定")]
    public float maxHealth = 100f; // 增加最大血量設定
    private float currentHealth;
    public int xpValue = 20;
    public float gravity = 9.81f;       // 重力強度
    private float _verticalVelocity;    // 當前的垂直速度

    [Header("UI 元件")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TMP_InputField playerInput;

    [Header("NPC 設定")]
    public string npcName = "神祕劍客";
    public string defaultGreeting = "這位少俠，找在下有何貴幹？";

    private bool isPlayerNearby = false;

    void Start()
    {
        dialoguePanel.SetActive(false); // 預設關閉對話框
    }

    void Update()
    {
        // 玩家在附近且按下 F 鍵觸發對話
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
        {
            ToggleDialogue();
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
        }
    }

}