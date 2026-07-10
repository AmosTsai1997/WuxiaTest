using UnityEngine;
using System.Collections; // 必須引入，協程才能運作

public class BossEnemyAI : MonoBehaviour
{   
    private Health healthScript; // 引用 Health 腳本   
    public Transform player;          // 拖入大俠
    public float patrolSpeed = 2f;    // 巡邏速度
    public float chaseSpeed = 4f;     // 追擊速度
    public float detectRange = 8f;    // 發現距離
    public float attackRange = 1.5f;  // 攻擊距離
    public float damage = 20f; // 設定敵人的攻擊力
    public float attackCooldown = 2.0f; // 這裡定義冷卻時間，2.0 代表兩秒砍一次
    private float _nextAttackTime; // 這是用來記錄時間的變數，不需要 public
    private Vector3 _targetPoint;     // 隨機巡邏點
    private float _patrolTimer;       // 巡邏計時器
    private CharacterController _controller;
    private Animator _animator;
    [Header("【技能：大跳躍】")]
    public float jumpSkillCooldown = 20f; // 跳躍技能冷卻時間
    private float _nextJumpTime;          // 下一次可跳躍的時間
    private bool _isPerformingJump = false; // 是否正在執行跳躍技能的鎖定開關

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();

        if (_animator == null)
        {
            Debug.LogError("在子物件中也找不到 Animator！請確認皮膚模組上有掛組件。");
        }

        // 防呆機制：如果忘記在 Inspector 拖入 Player，就自動用標籤找
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        GetNewPatrolPoint();
    }

    void Update()
    {
        // 獲取怪物自己的 Health 組件（使用你在最上方宣告的變數）
        if (healthScript == null) healthScript = GetComponent<Health>();

        // 如果怪物自己死了，就不跑 AI
        if (healthScript != null && healthScript.currentHP <= 0) 
        {
            _animator.SetFloat("Speed", 0f); // 確保死掉時不會滑步
            return;
        }
        
        // 先確認 player 物件是否存在（防止 MissingReferenceException）
        if (player == null) return;
        
        // 再確認 player 是否還活著（透過 Health 腳本）
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth != null && playerHealth.isDead) 
        {
            // 如果大俠死了，怪物就停止追擊或播勝利動畫
            _animator.SetFloat("Speed", 0f);
            return; 
        }

        // --- 【新增：狀態鎖定】 ---
        // 如果正在施放跳躍技能，直接切斷後續的所有移動與攻擊邏輯
        if (_isPerformingJump) return;

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        // 攻擊動畫（咬擊或揮拳）播放中就鎖定，不准移動
        if ((stateInfo.IsName("Creep|Bite_Action") || stateInfo.IsName("Creep|Punch_Action"))
            && stateInfo.normalizedTime < 1.0f)
        {
            _animator.SetFloat("Speed", 0);
            return;
        }

        // --- 【新增：隨機觸發跳躍技能】 ---
        // 當在追擊或攻擊範圍內（可自行調整條件），且冷卻時間到了，有 30% 機率發動
        if (Time.time >= _nextJumpTime && Vector3.Distance(transform.position, player.position) <= detectRange)
        {
            if (Random.value < 0.3f) // 30% 機率
            {
                StartCoroutine(JumpSkillRoutine());
                return; // 發動後立刻中斷本次 Update
            }
            else
            {
                // 如果這次沒抽中，過 2 秒再抽一次，避免每幀都狂抽
                _nextJumpTime = Time.time + 2f;
            }
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            AttackPlayer();
        }
        else if (distance <= detectRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }
    void Patrol()
    {
        _patrolTimer += Time.deltaTime;
        if (_patrolTimer > 5f || Vector3.Distance(transform.position, _targetPoint) < 1f)
        {
            GetNewPatrolPoint();
            _patrolTimer = 0;
        }

        MoveTowards(_targetPoint, patrolSpeed);
        _animator.SetFloat("Speed", 1f); // 假設 0.5 是走路動畫
    }

    void ChasePlayer()
    {
        MoveTowards(player.position, chaseSpeed);
        _animator.SetFloat("Speed", 4f); // 假設 1 是跑步動畫
    }
    void AttackPlayer()
    {
        if (Time.time >= _nextAttackTime)
        {
            _animator.SetFloat("Speed", 0);

            // 1. 隨機出招：一半機率咬擊（Attack → Bite），一半機率揮拳（Punch）
            //    兩個 Trigger 都走 AnyState 轉換，所以不管當下在哪個狀態都能出招
            string move = (Random.value < 0.5f) ? "Attack" : "Punch";
            _animator.SetTrigger(move);

            // 2. 讓敵人面向玩家
            Vector3 lookPos = player.position - transform.position;
            lookPos.y = 0;
            transform.rotation = Quaternion.LookRotation(lookPos);

            // 3. 重點：不要直接扣血，改用延遲呼叫
            // 假設動畫揮下去的時間大約是 0.5 秒，這 0.5 秒就是大俠的生存空檔
            Invoke("ApplyDamageToPlayer", 0.5f);

            // 4. 設定下次冷卻時間
            _nextAttackTime = Time.time + attackCooldown;
        }
    }
    // --- 【新增：跳躍技能協程】 ---
    IEnumerator JumpSkillRoutine()
    {   

        _isPerformingJump = true; // 開啟鎖定，Update 的其他動作全停
        _animator.SetFloat("Speed", 0); // 速度歸零，防止滑步

        // 1. 播放 Jumpout 動畫
        _animator.SetTrigger("JumpOut");

        // 【注意】這裡要等待 Jumpout 動畫播完（假設動畫長度是 1.2 秒，請依實際調整）
        yield return new WaitForSeconds(1.2f);

        // 2. 增加自身血量 50
        if (healthScript != null)
        {
            // 這裡假設你的 Health 腳本內有 Heal 或類似的方法
            // 如果沒有，可以直接用 healthScript.currentHP += 50f; (但要小心別超過最大血量)
            healthScript.currentHP = Mathf.Min(healthScript.currentHP + 50f, healthScript.maxHP);
            Debug.Log("<color=cyan>Boss 施展了大跳躍，回復了 50 點血量！</color>");
        }

        // 3. 播放 Jumpin 動畫
        _animator.SetTrigger("JumpIn");

        // 【注意】等待 Jumpin 動畫落地的時間（假設是 1.5 秒，請依實際調整）
        yield return new WaitForSeconds(1.5f);

        // 4. 技能結束，解除鎖定，計算下一次冷卻
        _nextJumpTime = Time.time + jumpSkillCooldown;
        _isPerformingJump = false;
    }
    // 建立一個專門處理扣血邏輯的函式
    void ApplyDamageToPlayer()
    {
        // 在真正扣血前，再判定一次距離
        // 如果大俠這 0.5 秒內跑開了，就不會受傷！
        float currentDist = Vector3.Distance(transform.position, player.position);
        if (currentDist <= attackRange + 0.8f) // 給一點點判定的寬容值
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log("<color=red>大俠被擊中了！</color>");
            }
        }
        else
        {
            Debug.Log("<color=green>大俠躲過了怪物的攻擊！</color>");
        }
    }

    void MoveTowards(Vector3 target, float moveSpeed)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0;
        
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.1f);
           
        }

        if (direction.magnitude > 0.1f) // 確保方向向量夠大
        {
         //   Debug.Log("Enemy正在移動中，方向：" + direction); // 如果 Console 沒跳這行，代表邏輯沒跑進來
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.1f);
            
            // 核心位移代碼
            _controller.Move(direction * moveSpeed * Time.deltaTime );
        }
    }

    void GetNewPatrolPoint()
    {
        float randomX = Random.Range(-10f, 10f);
        float randomZ = Random.Range(-10f, 10f);
        _targetPoint = transform.position + new Vector3(randomX, 0, randomZ);
    }

}