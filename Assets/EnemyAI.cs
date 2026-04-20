using UnityEngine;

public class EnemyAI : MonoBehaviour
{   private Health healthScript; // 引用 Health 腳本   
    private SimpleGravity gravity,verticalVelocity; // SimpleGravity 腳本 
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
        
        // 取得當前動畫狀態的資訊
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
    
        // 如果現在正在播放攻擊動畫，就「鎖定」邏輯，不准做別的事
        // 注意：這裡的 "Attack01" 要跟你的 Animator 狀態名稱一致
        if (stateInfo.IsName("Attack01") && stateInfo.normalizedTime < 1.0f)
        {
            _animator.SetFloat("Speed", 0); // 攻擊時腳步停下
            return; // 直接跳出 Update，不執行後面的追逐或巡邏
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
        _animator.SetFloat("Speed", 0.5f); // 假設 0.5 是走路動畫
    }

    void ChasePlayer()
    {
        MoveTowards(player.position, chaseSpeed);
        _animator.SetFloat("Speed", 1f); // 假設 1 是跑步動畫
    }

    void AttackPlayer()
    {
        if (Time.time >= _nextAttackTime)
        {
            _animator.SetFloat("Speed", 0);
            
            // 1. 先播放攻擊動畫 (讓玩家看到前搖)
            _animator.SetTrigger("Attack"); 
            
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