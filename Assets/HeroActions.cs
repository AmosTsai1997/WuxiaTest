using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class HeroActions : MonoBehaviour
{
    private Animator _animator;
    private CharacterController _controller;
    private SimpleGravity _gravityScript;  // 引用另外一個重力腳本
    private bool _isGrounded;    // 用來存儲角色是否踩在地上的狀態
    private Vector3 _velocity;   // 用來存儲角色的垂直速度（跳躍與重力的力道）
    public float playerDamage = 34f;    // 大俠攻擊力比較高，三刀一隻
    public float playerAttackRange = 2f;// 攻擊範圍
    public float speed = 5f;            //移動速度
    public float jumpHeight = 2.0f;     // 跳躍高度
    public float turnSpeed = 720f;      //轉向速度

    void Start()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
        _gravityScript = GetComponent<SimpleGravity>();
    }

void Update()
    {
        // 死亡處理
        // 取得 Health 腳本組件
        Health health = GetComponent<Health>();

        // 如果死了，檢查是否按下 R 鍵復活
        if (health != null && health.isDead)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Respawn();
            }
            return; // 死了就不執行下面的移動邏輯
        }
        // 移動處理
        // 1. 計算水平位移 (WASD)
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(h, 0, v).normalized;
        // 準備一個最終的位移向量
    Vector3 finalVelocity = Vector3.zero;

        // 2. 處理水平移動 (參考攝影機方向)
        if (inputDir.magnitude >= 0.1f)
        {
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            // 計算水平速度向量
            Vector3 moveDir = (camForward * v + camRight * h);
            finalVelocity = moveDir * speed;

            // 轉向邏輯
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            
            _animator.SetFloat("Speed", speed);
        }
        else
        {
            _animator.SetFloat("Speed", 0f);
        }

        // 3. 處理垂直移動 (重力) 
        // 注意：這裡必須在 if 外面，且是「疊加」到 finalVelocity 上
        if (_gravityScript != null)
        {
            // 直接把 SimpleGravity 算好的速度填入 Y 軸
            finalVelocity.y = _gravityScript.verticalVelocity;
        }

        // 4. 【全劇終】整個 Update 只能有這一個 Move
        // 這樣水平跟垂直才不會互相打架
        _controller.Move(finalVelocity * Time.deltaTime);

        // 2. 跳躍邏輯 (改為修改 SimpleGravity 的速度)
        if (Input.GetButtonDown("Jump") && _controller.isGrounded)
        {
            if (_gravityScript != null)
            {
                // 直接把跳躍的力量傳給重力腳本裡的垂直速度變數
                // 假設 SimpleGravity 裡那個變數叫 _verticalVelocity (請確認名稱)
                _gravityScript.SetJumpVelocity(jumpHeight); 
            }
            _animator.SetTrigger("Jump");
        }

        // 3. 攻擊邏輯
        if (Input.GetMouseButtonDown(0))
        {
            DoAttack();
        }
    }

    void DoAttack()
    {
        _animator.SetTrigger("Attack");
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, playerAttackRange);
        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Health eHealth = enemy.GetComponent<Health>();
                if (eHealth != null) eHealth.TakeDamage(playerDamage);
            }
        }
    }

    void Respawn()
    {
        Health health = GetComponent<Health>();
        
        // 1. 回復血量
        health.currentHP = health.maxHP;
        health.isDead = false;
        
        // 2. 隱藏死亡介面
        if (health.deathPanel != null)
        {
            health.deathPanel.SetActive(false);
        }

        // 3. 傳送到重生點 (或是當前位置原地復活)
        // 如果你有設定重生點物件，可以用：transform.position = respawnPoint.position;
        // 暫時我們先原地復活，並讓角色站起來
        transform.position = new Vector3(0, 5, 0); // 這裡填入你地圖的起始座標
        
        // 4. 重啟控制與動畫
        GetComponent<Animator>().Rebind(); // 重設動畫狀態
        GetComponent<Animator>().Update(0f);
        this.enabled = true; // 重新啟用 HeroActions 腳本
    }
}