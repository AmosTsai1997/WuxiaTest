using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class HeroActions : MonoBehaviour
{
    private Animator _animator;
    private CharacterController _controller;
    private SimpleGravity _gravityScript;
    private LevelSystem _levelSystem; // 引用新的等級系統

    [Header("基礎攻擊力與範圍")]
    public float playerDamage = 30f;    
    public float playerAttackRange = 2f;

    [Header("移動與跳躍")]
    public float speed = 5f;            
    public float jumpHeight = 2.0f; // 修正：0.1f 太低了，改回 2.0f 較合適     
    public float turnSpeed = 720f;      

    [Header("武器系統")]
    public GameObject swordModel; 
    public GameObject bowModel;   
    public string currentWeapon = "None";
    [Header("武器距離設定")]
    public float swordRange = 2.5f; // 劍的距離
    public float bowRange = 15f;    // 弓的距離
    public float arrowWidth = 0.5f; // 讓箭矢的判定半徑變大 (像是一根粗水管)
    [Range(0, 180)]
    public float attackAngle = 60f; // 攻擊角度 (例如正前方 60 度內才算打到)

    [Header("戰鬥節奏設定")]
    public float nextAttackTime = 0.8f; // 下一次可以攻擊的時間
    public float swordCooldown = 1f; // 劍的間隔 (秒)
    public float bowCooldown = 1.2f;   // 弓的間隔 (秒)

   [Header("旋風斬設定")]
    public float whirlwindDuration = 1.5f; // 總執行秒數：1.5s
    public float whirlwindInterval = 0.5f; // 動態攻擊間隔：0.5s
    public float whirlwindRadius = 2.0f;   // 稍微加大範圍確保打擊感
    private bool isWhirlwinding = false;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
        _gravityScript = GetComponent<SimpleGravity>();
        _levelSystem = GetComponent<LevelSystem>(); // 初始化引用
    }

    void Update()
    {   
        Health health = GetComponent<Health>();
        if (health != null && health.isDead)
        {   
            //避免技能持續造傷害
            isWhirlwinding = false; // 解鎖狀態
            StopAllCoroutines();    // 立即停止旋風斬的傷害計時器
            //啟動重生
            if (Input.GetKeyDown(KeyCode.R)) Respawn();
            return; 
        }

        // 1. 處理跳躍
        if (Input.GetButtonDown("Jump") && _controller.isGrounded)
        {
            if (_gravityScript != null)
            {
                _gravityScript.SetJumpVelocity(jumpHeight); 
                _animator.SetTrigger("Jump");
            }
        }
        // 2. 處理移動
        HandleMovement();

        // 4按鍵切換武器 (鍵盤 1 和 2)
        HandleWeaponSwitch();

        // 5. 攻擊邏輯
        if (Input.GetMouseButtonDown(0))
        {
            // 檢查現在時間是否已經超過了「下次可攻擊時間」
            if (Time.time >= nextAttackTime)
            {
                DoAttack();
            }
            else
            {
                Debug.Log("攻擊冷卻中...");
            }
        }

        // 6. 監測 Q 鍵啟動技能
        if (Input.GetKeyDown(KeyCode.Q) && !isWhirlwinding)
        {
            StartCoroutine(WhirlwindRoutine());
        }
    }
    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(h, 0, v).normalized;
        Vector3 finalVelocity = Vector3.zero;

        if (inputDir.magnitude >= 0.1f)
        {
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;
            camForward.y = 0; camRight.y = 0;
            camForward.Normalize(); camRight.Normalize();

            Vector3 moveDir = (camForward * v + camRight * h);
            finalVelocity = moveDir * speed;

            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            _animator.SetFloat("Speed", speed);
        }
        else
        {
            _animator.SetFloat("Speed", 0f);
        }

        if (_gravityScript != null)
        {
            finalVelocity.y = _gravityScript.verticalVelocity;
        }

        _controller.Move(finalVelocity * Time.deltaTime);
    }
    void HandleWeaponSwitch()
    {
        if (_levelSystem == null) return;

        // 按下 1：切換到劍 (需 LV 10)
        if (Input.GetKeyDown(KeyCode.Alpha1) && _levelSystem.currentLevel >= 10)
        {
            EquipWeapon("Sword");
        }
        
        // 按下 2：切換到弓 (需 LV 20)
        if (Input.GetKeyDown(KeyCode.Alpha2) && _levelSystem.currentLevel >= 20)
        {
            EquipWeapon("Bow");
        }
    }

    void EquipWeapon(string weaponName)
    {
        // 1. 安全檢查：確保兩個模型都存在
        if (swordModel == null || bowModel == null)
        {
            Debug.LogError("錯誤：劍或弓的模型欄位是空的，請確認 Hierarchy 物件已拖入！");
            return;
        }

        // 2. 先把所有武器都關掉 (清空狀態)
        swordModel.SetActive(false);
        bowModel.SetActive(false);

        // 3. 根據選擇開啟對應武器
        if (weaponName == "Sword")
        {
            currentWeapon = "Sword";
            swordModel.SetActive(true); // 只開劍
            _animator.SetInteger("WeaponType", 1);
            playerDamage = 50f;
            playerAttackRange = swordRange;
        }
        else if (weaponName == "Bow")
        {
            currentWeapon = "Bow";
            bowModel.SetActive(true); // 只開弓
            _animator.SetInteger("WeaponType", 2);
            playerDamage = 40f;
            playerAttackRange = bowRange;
        }

        Debug.Log($"<color=green>武器切換成功：{weaponName}</color>");
    }
    void DoAttack()
    {   
        _animator.SetTrigger("Attack");

        // 1. 根據目前武器決定數值
        float range;
        float cooldown;
        
        if (currentWeapon == "Bow") {
            range = bowRange;
            cooldown = bowCooldown;
        } else if (currentWeapon == "Sword") {
            range = swordRange;
            cooldown = swordCooldown;
        } else {
            // 拳頭模式 (None)
            range = playerAttackRange; // 使用基礎範圍
            cooldown = 0.5f; 
        }
        
        nextAttackTime = Time.time + cooldown;

        float multiplier = (_levelSystem != null) ? _levelSystem.attackMultiplier : 1.0f;
        float finalDamage = playerDamage * multiplier;

        // 2. 搜尋範圍內所有 Collider
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, range);
        
        foreach (Collider enemy in hitEnemies)
        {
            // 檢查標籤
            if (enemy.CompareTag("Enemy"))
            {
                // 3. 指向性判定優化 (忽略高度差)
                Vector3 dirToEnemy = (enemy.transform.position - transform.position);
                dirToEnemy.y = 0; // 關鍵：強制抹平 Y 軸，只看平面方向
                Vector3 myForward = transform.forward;
                myForward.y = 0;

                float angle = Vector3.Angle(myForward, dirToEnemy.normalized);

                // 判斷角度
                if (angle <= attackAngle / 2f)
                {
                    // 4. 執行傷害
                    if (currentWeapon == "Bow")
                    {
                        if (HasLineOfSight(enemy.transform))
                        {
                            ApplyDamage(enemy, finalDamage);
                        }
                    }
                    else
                    {
                        ApplyDamage(enemy, finalDamage);
                    }
                }
                else {
                    // 如果沒打中，Debug 出來看看角度差多少
                    Debug.Log($"角度不對：當前角度 {angle}，允許範圍 {attackAngle / 2f}");
                }
            }
        }
    }

    // 弓箭專用的視線檢查
    bool HasLineOfSight(Transform target)
    {
        RaycastHit hit;
        Vector3 startPos = transform.position + Vector3.up * 1f;
        Vector3 dir = (target.position + Vector3.up * 1f) - startPos;

        // 將 Raycast 改為 SphereCast
        // 參數：起點, 半徑, 方向, 結果, 距離
        if (Physics.SphereCast(startPos, arrowWidth, dir.normalized, out hit, bowRange))
        {
            // 只要掃到的物件是目標或是標籤為 Enemy 就算中
            if (hit.transform == target || hit.transform.CompareTag("Enemy"))
            {
                return true;
            }
        }
        return false;
    }
    // 測試距離與角度是否合理 可以在Scene 視窗就能看到攻擊扇形。
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float range = (currentWeapon == "Bow") ? bowRange : swordRange;
        
        // 畫出攻擊距離的圓圈
        Gizmos.DrawWireSphere(transform.position, range);
        
        // 畫出攻擊角度的兩條邊界線
        Vector3 leftBoundary = Quaternion.Euler(0, -attackAngle / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, attackAngle / 2, 0) * transform.forward;
        
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * range);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * range);
    }
    void ApplyDamage(Collider enemy, float damage)
    {
        Health eHealth = enemy.GetComponent<Health>();
        if (eHealth != null)
        {
            eHealth.TakeDamage(damage);
            // 在畫面上畫一條綠線，代表打中了
            Debug.DrawLine(transform.position + Vector3.up, enemy.transform.position + Vector3.up, Color.green, 1.0f);
            Debug.Log($"<color=cyan>成功擊中！</color> {enemy.name} 受到 {damage} 傷害");
        }
    }

    // 配合 LevelSystem 的等級來解鎖武器
    public void CheckWeaponUnlock(int currentLevel)
    {
        if (currentLevel >= 10 && currentWeapon == "None")
        {
            UnlockWeapon("Sword");
        }
        if (currentLevel >= 20 && currentWeapon != "Bow")
        {
            UnlockWeapon("Bow");
        }
    }

    void UnlockWeapon(string type)
    {
        currentWeapon = type;
        if (type == "Sword")
        {
            if (swordModel != null) swordModel.SetActive(true);
            if (bowModel != null) bowModel.SetActive(false);
            playerDamage = 50f;
            Debug.Log("解鎖劍：基礎傷害提升至 50");
        }
        else if (type == "Bow")
        {
            if (swordModel != null) swordModel.SetActive(false);
            if (bowModel != null) bowModel.SetActive(true);
            playerAttackRange = 15f;
            playerDamage = 40f;
            Debug.Log("解鎖弓：攻擊距離大幅提升");
        }
    }
    //復活邏輯
    void Respawn()
    {
        Health health = GetComponent<Health>();
        if (health != null) health.ResetHealth();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (health.deathPanel != null) health.deathPanel.SetActive(false);

        _controller.enabled = false;
        transform.position = new Vector3(0, 5, 0); 
        _controller.enabled = true;

        _animator.Rebind();
        _animator.Update(0f);
    }
    // 旋風斬技能 Q
    IEnumerator WhirlwindRoutine()
    {   
        if(  currentWeapon == "Sword")
        {
        isWhirlwinding = true;
        
        // 1. 播放動畫 (請確認你的 Animator 中有 "Whirlwind" 這個 Trigger)
        _animator.SetTrigger("Whirlwind");
        _animator.SetBool("isWhirlwinding", true);
        float elapsed = 0f;
        // 計算單次轉圈的傷害：攻擊力 * 間隔 (例如 50 * 0.5 = 25)
        // 總共會執行 3 次 (1.5 / 0.5 = 3)，總傷害為 75
        float damagePerRotation = playerDamage * whirlwindInterval;

        Debug.Log($"啟動旋風斬！預計執行 {whirlwindDuration} 秒，每次傷害：{damagePerRotation}");
        
        while (elapsed < whirlwindInterval )
        {
            // 2. 執行範圍傷害計算
            ApplyCircleDamage(damagePerRotation);

            yield return new WaitForSeconds(whirlwindInterval);
            elapsed += whirlwindInterval;

            // 3. 等待 0.5 秒（配合你說的動畫一圈時間）
            yield return new WaitForSeconds(whirlwindInterval);
            elapsed += whirlwindInterval;
        }
        _animator.SetBool("isWhirlwinding", false);
        isWhirlwinding = false;
        Debug.Log("旋風斬結束");
        }
    }
    // 旋風斬傷害
    void ApplyCircleDamage(float damagePerRotation)
    {
        // 物理圓形偵測：找出半徑 1.5m 內的所有碰撞體
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, whirlwindRadius);
        
        foreach (var hitCollider in hitColliders)
        {
            // 檢查是否為怪物 (假設你的怪物有 EnemyHealth 腳本或 Tag 為 Enemy)
            if (hitCollider.CompareTag("Enemy"))
            {
                // 取得怪物身上的 Health 腳本並扣血
                // 這裡假設你的怪物腳本叫 EnemyStatus，並且有個 TakeDamage 方法
                var enemy = hitCollider.GetComponent<Health>(); 
                if (enemy != null)
                {
                    enemy.TakeDamage(damagePerRotation);
                    Debug.Log($"[技能] 擊中 {hitCollider.name}，造成 {damagePerRotation} 傷害");
                }
            }
        }
    }
}