using UnityEngine;
using UnityEngine.UI; 

public class Health : MonoBehaviour
{
    public float maxHP ;
    public float currentHP{ get; private set; }
    [Header("UI 設定")]
    public Slider hpSlider; // 在 Inspector 把人物的 Slider 拖進來
    public bool isPlayer = false; // 勾選此項代表是大俠
    public bool isDead = false;
    private Animator _anim;
    private Collider _collider;
    [Header("死亡設定")]
    public GameObject deathPanel; // 在 Inspector 拖入剛才做的 DeathPanel
    [Header("獎勵設定")]
    public int expReward = 50;

    void Start()
    {
        currentHP = maxHP;
        _anim = GetComponent<Animator>(); // 呼叫動畫，初始化！
        _collider = GetComponent<Collider>(); // 取得碰撞體以便死亡時關閉

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
            Debug.Log("觸發死亡動畫！");
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
            // 專業做法：死掉時解鎖滑鼠，否則玩家沒辦法點擊 UI (如果有按鈕的話)
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            GetComponent<HeroActions>().enabled = false;
            // 重力也暫時歸零
            if (GetComponent<SimpleGravity>()) 
             {
                GetComponent<SimpleGravity>().verticalVelocity = 0;
             }            
            gameObject.SetActive(false);
        }
        // 如果這隻是怪物，且是被玩家打死的
        if (!isPlayer)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                LevelSystem ls = player.GetComponent<LevelSystem>();
                if (ls != null)
                {
                    ls.AddXP(expReward); // 給玩家經驗值
                }
            }

        }
    }
    public void ResetHealth()
    {
        isDead = false;
        currentHP = maxHP; // 因為是在 Health.cs 內部，所以可以修改 private set 的變數
        
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }
    }
    public void SetHealthFromLevel(float newMaxHP)
    {
        maxHP = newMaxHP;
        currentHP = maxHP; // 這裡在自己腳本內，一定有權限改
        Debug.Log($"血量已根據等級更新為：{maxHP}");
    }
}