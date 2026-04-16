using UnityEngine;
using UnityEngine.UI; // 必須引入 UI 命名空間
using System.Collections; // 為了使用協程 (Coroutine)

public class Enemy : MonoBehaviour
{
    [Header("數值設定")]
    public float maxHealth = 100f; // 增加最大血量設定
    public float currentHealth;
    public int xpValue = 20;
    public float gravity = 9.81f;       // 重力強度
    private float _verticalVelocity;    // 當前的垂直速度


    [Header("UI 元件")]
    public Image hpFillImage; // 拖拽你的 Fill Image 到這裡

    [Header("視覺效果")]
    public MeshRenderer meshRenderer; // 為了讓怪物閃紅光
    private Color originalColor;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        // 記錄原本的顏色
        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }
        UpdateHealthBar();
    }

    void Update()
    {
        // 讓血條永遠面對鏡頭 (Billboard 效果)
        if (hpFillImage != null && Camera.main != null)
        {
            hpFillImage.transform.parent.parent.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
        }
    }

    public void TakeDamage(float damage, PlayerLevel player)
    {
        if (isDead) return; // 如果死了就不要再計算傷害

        currentHealth -= damage;
        Debug.Log($"怪物受傷！剩餘血量: {currentHealth}");

        // 1. 更新血條
        UpdateHealthBar();

        // 2. 播放受擊特效 (閃紅光)
        if (gameObject.activeInHierarchy) // 確保物件還活著才能跑協程
        {
            StartCoroutine(FlashDamageEffect());
        }

        if (currentHealth <= 0)
        {
            Die(player);
        }
    }

    void UpdateHealthBar()
    {
        if (hpFillImage != null)
        {
            // 計算百分比 0~1
            float fillAmount = currentHealth / maxHealth;
            hpFillImage.fillAmount = fillAmount;
        }
    }

    // 協程：閃爍紅色特效
    IEnumerator FlashDamageEffect()
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = Color.red; // 變紅
            yield return new WaitForSeconds(0.1f); // 等待 0.1 秒
            meshRenderer.material.color = originalColor; // 變回原色
        }
    }

    void Die(PlayerLevel player)
    {
        if (isDead) return;
        isDead = true;
        
        // --- 關鍵修正：關閉 AI 邏輯，防止死後亂動 ---
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false; 

        player.AddExperience(xpValue);
        Debug.Log($"擊敗怪物！獲得經驗值: {xpValue}");

        StartCoroutine(DeathSequence());
    }

    // 協程：倒地並在幾秒後消失
    IEnumerator DeathSequence()
    {
        // 關閉血條
        if (hpFillImage != null) hpFillImage.transform.parent.parent.gameObject.SetActive(false);

        // 強制停止動畫機的干擾
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null) {
            anim.SetFloat("Speed", 0);
            anim.enabled = false; // 如果是用程式碼旋轉(Euler 90)，就直接關掉 Animator 防止它跟旋轉衝突
        }

        // 簡單的武俠式倒地：向後旋轉 90 度
        Quaternion targetRotation = transform.rotation * Quaternion.Euler(90f, 0, 0);
        float duration = 0.5f; // 倒地持續時間
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null; // 等待下一幀
        }
        transform.rotation = targetRotation; // 確保完全倒下

        // 關閉碰撞體，防止玩家卡在屍體上
        if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false;

        // 等待 3 秒後，怪物屍體消失
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}