using UnityEngine;
using UnityEngine.UI; // 必須引用 UI 命名空間
public class Health : MonoBehaviour
{
    public float maxHP = 500f;
    public float currentHP;
    [Header("無敵幀設定")]
    public float invincibilityDuration = 1.5f; // 無敵時間長度
    private float _lastDamageTime = -99f;      // 記錄上一次受傷的時間
    [Header("UI 設定")]
    public Slider hpSlider; // 在 Inspector 把人物的 Slider 拖進來
    public bool isPlayer = false; // 勾選此項代表是大俠
    public bool isDead = false;
    private Animator _anim;
    private Collider _collider;
    [Header("死亡設定")]
    public GameObject deathPanel; // 在 Inspector 拖入剛才做的 DeathPanel

    void Start()
    {
        currentHP = maxHP;
        _anim = GetComponent<Animator>(); // 初始化！
        _collider = GetComponent<Collider>(); // 取得碰撞體以便死亡時關閉
       
       // 自動尋找：先找腳本同層，再找子物件 (Canvas)
        if (hpSlider == null)
        {
            hpSlider = GetComponentInChildren<Slider>();
        }
        UpdateUI();
    }

    // 這是受傷 API
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // 【核心邏輯】檢查是否還在無敵時間內
        if (Time.time < _lastDamageTime + invincibilityDuration)
        {
            Debug.Log($"{gameObject.name} 處於無敵狀態，免疫傷害！");
            return; // 直接跳出，不執行扣血
        }

        // 執行扣血
        _lastDamageTime = Time.time; // 更新最後受傷時間
        currentHP -= damage;
        
        Debug.Log($"{gameObject.name} 受傷！扣除 {damage}，剩餘 {currentHP}");

        UpdateUI();
        
        // --- 關鍵修正：嘗試尋找頭上的 Slider 並更新 ---
        // 假設 Slider 是敵人物件的子物件
        Slider healthBar = GetComponentInChildren<Slider>();
        if (healthBar != null)
        {
            healthBar.value = currentHP / maxHP; // 如果 Slider 範圍是 0~1
            // 或者 healthBar.value = currentHP; // 如果 Slider 範圍是 0~MaxHP
        }

        if (currentHP <= 0) Die();
        else
        {
            // 播放受傷動畫 (如果有設定 Hurt 參數)
           if (_anim != null) _anim.SetTrigger("Hurt");
        }
    }
    void UpdateUI()
        {
            if (hpSlider != null)
            {
                // 將 Slider value 設為百分比 (0 到 1)
                hpSlider.value = currentHP / maxHP;
            }
        }
    void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("觸發死亡動畫！");

        // 1. 變紅確認 (視覺偵錯)
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.material.color = Color.red;

        // 2. 徹底斷開 AI 與 攻擊邏輯
        // 我們要確保沒有人能在下一幀再去改 Animator 的參數
        MonoBehaviour[] allScripts = GetComponents<MonoBehaviour>();
        foreach (var script in allScripts)
        {
            if (script != this) script.enabled = false;
        }

        // 3. 處理 Animator：防止它被參數(如 Speed 或 Attack)拉回 Idle
        if (_anim != null)
        {
            // 先把所有觸發器重設，防止積壓的訊號觸發轉場
            _anim.ResetTrigger("Attack");
            _anim.ResetTrigger("Hurt");
            
            // 如果你有用 Float 控制 Speed，強制歸零，防止轉回 Move/Idle
            _anim.SetFloat("Speed", 0); 
            
            // 關鍵動作：直接播放 Die，並且「關閉轉場」
            _anim.Play("Die", 0, 0f);
            
            // 如果他還是會跳回 Idle，請試試這行暴力解：
            // _anim.runtimeAnimatorController = null; // (警告：這會完全失效，除非你只想讓他停在第一幀)
        }

            // 4. 關閉物理特性，防止被推擠或偵測
            if (GetComponent<Collider>()) GetComponent<Collider>().enabled = false;
            if (GetComponent<CharacterController>()) GetComponent<CharacterController>().enabled = false;

            Destroy(gameObject, 5f);

        
        // 如果是大俠，關閉移動控制，避免死掉還能滑行
        if (isPlayer)
        {   
            // 顯示死亡介面
            if (deathPanel != null) deathPanel.SetActive(true);
            GetComponent<HeroActions>().enabled = false;
            // 重力也暫時歸零
            if (GetComponent<SimpleGravity>()) 
                GetComponent<SimpleGravity>().verticalVelocity = 0;
        }

    }
}