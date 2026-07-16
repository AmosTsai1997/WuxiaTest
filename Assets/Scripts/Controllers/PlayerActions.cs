using System.Collections;
using UnityEditor.Timeline;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerActions : MonoBehaviour
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
    public AudioClip swordSound;
    [Header("武器系統掛載")]
    public Transform weaponSocket;           // 右手骨架下的空物件，Inspector 拖入
    private GameObject _weaponModelInstance; // 目前手上生成的武器模型

    [Header("防具系統掛載")]
    public Transform HelmetSocket;           //  帽子
    private GameObject _HelmetModelInstance;

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
    public float whirlwindDuration = 2.0f; // 總執行秒數：1.5s
    public float whirlwindInterval = 0.5f; // 動態攻擊間隔：0.5s
    public float whirlwindRadius = 2.0f;   // 稍微加大範圍確保打擊感
    public float whirlwindCooldown = 5.0f; // <--- 新增：技能 CD 時間
    private float nextWhirlwindTime = 0f;  // <--- 新增：紀錄下一次可以放技能的時間點
    private bool isWhirlwinding = false;

    // --- 攻速 Buff 狀態（攻速藥水用）---
    private float _attackSpeedMultiplier = 1f;   // 1 = 無加成；1.3 = 攻速 +30%
    private Coroutine _attackSpeedRoutine;       // 記住進行中的 Buff，重複喝藥時要重算時間
    // --- 裝備系統接入 ---
    private int _weaponAttackBonus = 0;              // 目前佩帶武器的攻擊加成
    private RuntimeAnimatorController _baseAnimator; // 原始控制器（脫裝時還原用）
    private float _weaponCooldown = 1f;   // 目前佩帶武器的攻擊間隔

    void Start()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
        _gravityScript = GetComponent<SimpleGravity>();
        _levelSystem = GetComponent<LevelSystem>(); // 初始化引用
                                                    // 記住原本的動畫控制器，之後換武器動作要以它為底
        _baseAnimator = _animator.runtimeAnimatorController;

        // 訂閱裝備廣播：穿脫任何裝備都會通知我們
        if (EquipmentSystem.Instance != null)
        {
            EquipmentSystem.Instance.OnEquipmentChanged += RefreshWeapon;
            RefreshWeapon(); // 開場先同步一次（處理換場景後的狀態）
        }
    }
    void OnDestroy()
    {
        // 退訂，成對出現的鐵律
        if (EquipmentSystem.Instance != null)
            EquipmentSystem.Instance.OnEquipmentChanged -= RefreshWeapon;
    }
    void Update()
    {   
        Health health = GetComponent<Health>();
        if (health != null && health.isDead)
        {   
            if (isWhirlwinding)
            {
                isWhirlwinding = false;
                _animator.SetBool("isWhirlwinding", false);
                StopAllCoroutines(); 
            }
            return;
        }
      
        //  處理跳躍
        if (Input.GetButtonDown("Jump") && _controller.isGrounded)
        {
            if (_gravityScript != null)
            {
                _gravityScript.SetJumpVelocity(jumpHeight); 
                _animator.SetTrigger("Jump");
            }
        }
        //  處理移動
        HandleMovement();

        //  攻擊邏輯
        if (Input.GetMouseButtonDown(0))
        {
            // 檢查現在時間是否已經超過了「下次可攻擊時間」
            if (Time.time >= nextAttackTime)
            {
                DoAttack();
                AudioManager.Instance.PlaySE(swordSound);
            }
            else
            {
                Debug.Log("攻擊冷卻中...");
            }
        }

        // 6. 監測 Q 鍵啟動技能(旋風斬)
        if (Input.GetKeyDown(KeyCode.Q) && !isWhirlwinding)
        {
            if (currentWeapon == "Sword")
            {
                // 檢查是否冷卻完畢
                if (Time.time >= nextWhirlwindTime)
                {
                    StartCoroutine(WhirlwindRoutine());
                }
                else
                {
                    // 在 Console 顯示還剩幾秒，方便除錯
                    float cdLeft = nextWhirlwindTime - Time.time;
                    Debug.Log($"<color=orange>旋風斬冷卻中... 剩餘 {cdLeft:F1} 秒</color>");
                }
            }
            else
            {
                Debug.Log("必須裝備【劍】才能使用旋風斬！");
            }
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
            // --- 修正：旋風斬期間不更新 Speed 參數 ---
            if (!isWhirlwinding)
            {
                _animator.SetFloat("Speed", speed);
            }
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
    public void RefreshWeapon()
    {
        EquipmentData weapon = EquipmentSystem.Instance.GetEquipped(EquipSlotType.Weapon);

        // 換裝第一步：不管新武器是什麼，先把手上舊的模型拆掉
        if (_weaponModelInstance != null) Destroy(_weaponModelInstance);

        if (weapon == null)
        {
            // === 空手狀態 ===
            _weaponAttackBonus = 0;
            currentWeapon = "None";
            _animator.runtimeAnimatorController = _baseAnimator; // 還原預設動作
            _animator.SetInteger("WeaponType", 0);               // 揮拳
            if (swordModel != null) swordModel.SetActive(false);
            return;
        }

        // === 有佩帶武器 ===
        _weaponAttackBonus = weapon.attackBonus;
        _weaponCooldown = weapon.attackCooldown; // 讀取這把武器自己的攻速
        currentWeapon = "Sword"; // 沿用既有近戰流程（旋風斬的檢查也會通過）

        if (weapon.attackAnimation != null)
        {
            // 點唱機換歌：複製一份原始控制器，只把近戰攻擊那格換成武器自己的動作。
            // 中括號裡的字串 = 原本狀態機裡那段動畫「片段」的名字，一字不差
            AnimatorOverrideController aoc = new AnimatorOverrideController(_baseAnimator);
            aoc["MeeleeAttack_TwoHanded"] = weapon.attackAnimation;
            _animator.runtimeAnimatorController = aoc;
        }
        else
        {
            _animator.runtimeAnimatorController = _baseAnimator; // 沒填動作就用預設
        }

        // 注意：換控制器會把所有參數重置，所以 WeaponType 一定要在「換完之後」才設
        _animator.SetInteger("WeaponType", 1);
        // 生成武器模型，掛在手掌骨架的插槽底下（跟著手一起動）
        if (weapon.weaponPrefab != null && weaponSocket != null)
        {
            _weaponModelInstance = Instantiate(weapon.weaponPrefab, weaponSocket);
            _weaponModelInstance.transform.localPosition = weapon.gripPosition;    // 原本是 Vector3.zero
            _weaponModelInstance.transform.localEulerAngles = weapon.gripRotation; // 原本是 identity
        }
    }
    void RefreshArmors()
    {
        EquipmentData Helmet = EquipmentSystem.Instance.GetEquipped(EquipSlotType.Helmet);
        EquipmentData Body = EquipmentSystem.Instance.GetEquipped(EquipSlotType.Body);
        EquipmentData Feet = EquipmentSystem.Instance.GetEquipped(EquipSlotType.Feet);
        EquipmentData Hands = EquipmentSystem.Instance.GetEquipped(EquipSlotType.Hands);


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
            cooldown = _weaponCooldown; // 攻速由佩帶的武器決定，不再是寫死的常數
        } else {
            // 拳頭模式 (None)
            range = playerAttackRange; // 使用基礎範圍
            cooldown = 0.5f;
        }

        // 攻速 Buff：倍率 1.3 = 冷卻除以 1.3 = 出手快 30%
        cooldown /= _attackSpeedMultiplier;

        nextAttackTime = Time.time + cooldown;

        float multiplier = (_levelSystem != null) ? _levelSystem.attackMultiplier : 1.0f;
        // 基礎傷害 + 武器加成，再乘等級倍率
        float finalDamage = (playerDamage + _weaponAttackBonus) * multiplier;

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
    //  復活邏輯
    void Respawn()
    {
        Health health = GetComponent<Health>();
        if (health != null) 
        {
            health.ResetHealth(); // 重要：此方法內必須包含 isDead = false;
        }
        
        if (health.deathPanel != null) health.deathPanel.SetActive(false);

        // 瞬間傳送需先禁用 Controller
        _controller.enabled = false;
        transform.position = new Vector3(440, 5, 400); 
        _controller.enabled = true;

        // 重置動畫狀態機
        _animator.Rebind();
        _animator.Update(0f);
        
        Debug.Log("大俠已在座標 (0, 5, 0) 重生");
    }
    // 旋風斬技能 Q
    IEnumerator WhirlwindRoutine()
    {   
        if(  currentWeapon == "Sword")
        {
            isWhirlwinding = true;

            // 進入 CD：設定下一次可以施放的時間 (目前時間 + CD 時間)
            nextWhirlwindTime = Time.time + whirlwindCooldown;

            // 【防卡死機制】：強制清除殘留的 Trigger，避免 Animator 錯亂
            _animator.ResetTrigger("Whirlwind");

            _animator.SetTrigger("Whirlwind");
            _animator.SetBool("isWhirlwinding", true);

            float elapsed = 0f;
            // 每圈傷害 = 基礎傷害 * 間隔 (例如 50 * 0.5 = 25)
            float damagePerRotation = (playerDamage + _weaponAttackBonus) * whirlwindInterval;

            Debug.Log($"啟動旋風斬！造成 {damagePerRotation} 傷害/次。");
            
            // 修正：條件改為總持續時間
            while (elapsed < whirlwindDuration)
            {
                ApplyCircleDamage(damagePerRotation);
                // 修正：只保留一個等待，確保頻率正確
                yield return new WaitForSeconds(whirlwindInterval);
                elapsed += whirlwindInterval;
            }

            _animator.SetBool("isWhirlwinding", false);
            isWhirlwinding = false;
            Debug.Log("旋風斬結束，進入冷卻狀態。");
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
                var enemy = hitCollider.GetComponent<Health>(); 
                if (enemy != null)
                {
                    enemy.TakeDamage(damagePerRotation);
                }
            }
        }
    }

    // --- 攻速藥水入口：bonus 0.3 = 攻速 +30%，duration 為持續秒數 ---
    public void ApplyAttackSpeedBuff(float bonus, float duration)
    {
        // 重複喝藥：先停掉舊的倒數，改用新的（效果不疊加、時間重算）
        if (_attackSpeedRoutine != null) StopCoroutine(_attackSpeedRoutine);
        _attackSpeedRoutine = StartCoroutine(AttackSpeedRoutine(bonus, duration));
    }

    IEnumerator AttackSpeedRoutine(float bonus, float duration)
    {
        _attackSpeedMultiplier = 1f + bonus;
        Debug.Log($"<color=orange>攻速提升 {bonus:P0}！持續 {duration} 秒</color>");

        yield return new WaitForSeconds(duration);

        _attackSpeedMultiplier = 1f;
        _attackSpeedRoutine = null;
        Debug.Log("攻速加成結束，恢復正常。");
    }
}