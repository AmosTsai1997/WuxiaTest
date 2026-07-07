using UnityEngine;
using UnityEngine.UI; 
using System.Collections.Generic; // 必須引用以使用 List

public class Health : MonoBehaviour
{   
    // 用來記錄所有 Renderer 及其原始材質的結構
    private struct RendererMaterialMap
    {
        public Renderer renderer;
        public Material originalMat;
    }
    private List<RendererMaterialMap> _materialRecords = new List<RendererMaterialMap>();
    public float maxHP ;
    public float currentHP{ get; private set; }
    [Header("UI 設定")]
    public Slider hpSlider; // 在 Inspector 把人物的 Slider 拖進來
    public bool isPlayer = false; // 勾選此項代表是大俠
    public bool isDead = false;

    [Header("死亡設定")]
    public GameObject deathPanel; // 在 Inspector 拖入剛才做的 DeathPanel
    public int expReward = 50;

    private Animator _anim;
    private Collider _collider;

    void Start()
    {
        currentHP = maxHP;
        _anim = GetComponent<Animator>(); // 呼叫動畫，初始化！
        if (_anim == null) _anim = GetComponentInChildren<Animator>(); // 找子物件
        _collider = GetComponent<Collider>(); // 取得碰撞體以便死亡時關閉

        // 啟動時記錄所有子物件的原始材質 (包含未啟用物件)
        _materialRecords.Clear();
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in allRenderers)
        {
            // 記錄 Renderer 及其 sharedMaterial (原始資源)
            _materialRecords.Add(new RendererMaterialMap { 
                renderer = r, 
                originalMat = r.sharedMaterial 
            });
        }
        // 初始化 UI 設定
        InitUI();
        UpdateUI();
    }
    void InitUI()
    {
        // 如果是小怪且沒手動指定 Slider，嘗試找尋子物件中的血條 (血條頂在頭上那種)
        if (!isPlayer && hpSlider == null)
        {
            hpSlider = GetComponentInChildren<Slider>();
        }

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP; // 這裡直接用數值，不用百分比更精確
            hpSlider.value = currentHP;
        }
    }
    // 這是受傷 API
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // 執行扣血
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        Debug.Log($"{gameObject.name} 受傷！扣除 {damage}，剩餘 {currentHP}");

        UpdateUI();

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
                // 關鍵：這行能確保 Slider 的刻度與你當前的最大血量一致
                hpSlider.maxValue = maxHP;
                hpSlider.value = currentHP;
            }
        }
    void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. 變紅確認 (視覺偵錯)
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.material.color = Color.red;
        // 2. 徹底關閉其他行為（防止怪物死掉還在走路或攻擊）
        MonoBehaviour[] allScripts = GetComponents<MonoBehaviour>();
        foreach (var script in allScripts)
        {   
            // 不要關掉自己，否則 Destroy(gameObject, 5f) 可能會失效
            if (script != this) script.enabled = false;
        }
        // 3. 處理動畫
        if (_anim == null) _anim = GetComponentInChildren<Animator>(); // 雙重檢查
        
        if (_anim != null)
        {
            // 先把所有觸發器重設，防止積壓的訊號觸發轉場
            _anim.ResetTrigger("Attack");
            _anim.ResetTrigger("Hurt");
            // 如果你有用 Float 控制 Speed，強制歸零，防止轉回 Move/Idle
            _anim.SetFloat("Speed", 0);

            // 關鍵動作：直接播放 Die，並且「關閉轉場」
            _anim.SetTrigger("Die");
            Debug.Log($"<color=cyan>[Health]</color> {gameObject.name} 成功觸發 Die Animator Trigger。");
        }
        else
        {
            Debug.LogWarning($"<color=red>[Health 警告]</color> {gameObject.name} 倒下了");
        }

        // 4. 關閉物理與碰撞（防止屍體擋路或重複受傷）
        if (_collider != null) _collider.enabled = false;
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // 5.分類處理，如果是大俠，關閉移動控制，避免死掉還能滑行
        if (isPlayer)
        {   
            HandlePlayerDeath();
        }
        else
        {
            HandleEnemyDeath();
        }
    }

    void HandlePlayerDeath()
    {
        if (deathPanel != null) deathPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (GetComponent<SimpleGravity>()) 
            GetComponent<SimpleGravity>().verticalVelocity = 0;

        // 禁用攻擊與移動，但「不要」SetActive(false)
        GetComponent<HeroActions>().enabled = false;
            
        // 注意：大俠通常「不」執行 Destroy，也不執行 SetActive(false) 否則動畫播不出來
        // 如果你一定要大俠消失，請用延遲：Invoke("HidePlayer", 3f);
        Debug.Log("請按下 R 鍵重新開始");
    }

    void HandleEnemyDeath()
    {
        // 發放經驗值
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            LevelSystem ls = player.GetComponent<LevelSystem>();
            if (ls != null) ls.AddXP(expReward);
        }

        // 這裡設定 5 秒後刪除怪物物件
        Destroy(this.gameObject, 5f);
        Debug.Log($"{gameObject.name} 將在 5 秒後消失");
    }
    //  復活邏輯
    public void ResetHealth()
    {           
        isDead = false;
        currentHP = maxHP;

        // 1. 先開啟 CharacterController (必須最先)
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) 
        {
            cc.enabled = true;
        }

        // 2. 重置重力腳本
        SimpleGravity gravity = GetComponent<SimpleGravity>();
        if (gravity != null) 
        {
            gravity.enabled = true;
            gravity.verticalVelocity = 0f; // 歸零，防止繼承死前的重力速度
        }


        // 1. 恢復顏色：還原成 Start 時記錄的顏色
        foreach (RendererMaterialMap record in _materialRecords)
        {
            // 死亡動畫播放期間有些子物件可能被刪，先確認還活著
            if (record.renderer != null)
            {
                record.renderer.material = record.originalMat;
            }
        }

        // 2. 恢復 UI
        UpdateUI();

        // 3. 恢復物理與腳本
        if (GetComponent<Collider>()) GetComponent<Collider>().enabled = true;
        if (GetComponent<CharacterController>()) GetComponent<CharacterController>().enabled = true;
        
        HeroActions actions = GetComponent<HeroActions>();
        if (actions != null) actions.enabled = true;

        // 4. 強力重置動畫
        if (_anim == null) _anim = GetComponentInChildren<Animator>();
        if (_anim != null) 
        {
            _anim.enabled = true;
            _anim.Rebind();
            _anim.Play("Idle", 0, 0f); // 強制跳回 Idle 狀態
        }

        if (deathPanel != null) deathPanel.SetActive(false);
        Debug.Log("大俠已滿血復活！");
    }
    public void SetHealthFromLevel(float newMaxHP)
    {
        maxHP = newMaxHP;
        currentHP = maxHP; // 這裡在自己腳本內，一定有權限改
        Debug.Log($"血量已根據等級更新為：{maxHP}");
    }
}