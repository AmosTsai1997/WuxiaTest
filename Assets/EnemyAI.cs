using UnityEngine;

public class EnemyAI : MonoBehaviour
{
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
    private float _verticalVelocity;

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
        // --- 如果 Enemy 腳本判定死亡，就不跑 AI ---
        Enemy healthScript = GetComponent<Enemy>();
        if (healthScript != null && healthScript.currentHealth <= 0) return;

        ApplyGravity();
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
            // 這裡設定一個冷卻，避免瘋狂攻擊
            _animator.SetTrigger("Attack"); 
            // 讓敵人面向玩家
            Vector3 lookPos = player.position - transform.position;
            lookPos.y = 0;
            transform.rotation = Quaternion.LookRotation(lookPos);
            
            // 簡單判斷：如果砍下去時玩家還在範圍內，就扣血
            if (Vector3.Distance(transform.position, player.position) <= attackRange + 0.5f)
            {
                Health playerHealth = player.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
            }
            _nextAttackTime = Time.time + attackCooldown;
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
            _controller.Move(direction * moveSpeed * Time.deltaTime + new Vector3(0, _verticalVelocity, 0) * Time.deltaTime);
        }
    }

    void GetNewPatrolPoint()
    {
        float randomX = Random.Range(-10f, 10f);
        float randomZ = Random.Range(-10f, 10f);
        _targetPoint = transform.position + new Vector3(randomX, 0, randomZ);
    }

    void ApplyGravity()
    {
        if (_controller.isGrounded) _verticalVelocity = -0.5f;
        else _verticalVelocity -= 9.81f * Time.deltaTime;
    }
}