using System.Collections;
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class PlayerActions : MonoBehaviour
{
    private Animator _animator;
    private CharacterController _controller;
    private SimpleGravity _gravityScript;
    private LevelSystem _levelSystem; // 引用新的等級系統
    private Health _health; // 快取起來，不要每幀 GetComponent

    [Header("空手基礎值（有佩帶武器時全部改用武器資料）")]
    public float playerDamage = 30f;      // 基礎傷害（武器加成疊在這上面）
    public float playerAttackRange = 2f;  // 空手距離
    public float fistCooldown = 0.5f;     // 空手攻擊間隔

    [Header("移動與跳躍")]
    public float speed = 5f;            
    public float jumpHeight = 2.0f; // 修正：0.1f 太低了，改回 2.0f 較合適     
    public float turnSpeed = 720f;      

    [Header("武器系統掛載")]
    public Transform righthandSocket;           // 右手骨架下的空物件，Inspector 拖入
    public Transform lefthandSocket; // 左手骨架下的空物件：弓專用（拉弓動畫是左手持弓、右手拉弦）
    private GameObject _weaponModelInstance; // 目前手上生成的武器模型
    public AudioClip swordSound;
    private EquipmentData _weapon;           // 目前佩帶的武器資料卡（null = 空手

    [Header("防具系統掛載")]
    public Transform HelmetSocket;           //  帽子
    private GameObject _HelmetModelInstance;

    [Header("攻擊判定")]
    public float arrowWidth = 0.5f;          // 弓箭判定半徑（粗水管）
    [Range(0, 180)]
    public float attackAngle = 60f;          // 正面攻擊角度

   [Header("旋風斬設定")]
    public float whirlwindDuration = 2.0f; // 總執行秒數：1.5s
    public float whirlwindInterval = 0.5f; // 動態攻擊間隔：0.5s
    public float whirlwindRadius = 2.0f;   // 稍微加大範圍確保打擊感
    public float whirlwindCooldown = 5.0f; // <--- 新增：技能 CD 時間
    private float nextWhirlwindTime = 0f;  // <--- 新增：紀錄下一次可以放技能的時間點
    private bool isWhirlwinding = false;

    // 對外提供「目前手上武器模型」的位置，校準工具直接問這裡，不用自己猜在哪隻手
    public Transform CurrentWeaponInstance =>
        (_weaponModelInstance != null) ? _weaponModelInstance.transform : null;
    private float nextAttackTime = 0f; // 下次可攻擊的時間點
    // --- 攻速 Buff 狀態（攻速藥水用）---
    private float _attackSpeedMultiplier = 1f;   // 1 = 無加成；1.3 = 攻速 +30%
    private Coroutine _attackSpeedRoutine;       // 記住進行中的 Buff，重複喝藥時要重算時間
    // --- 裝備系統接入 ---
    private RuntimeAnimatorController _baseAnimator; // 原始控制器（脫裝時還原用）


    void Start()
    {
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
        _gravityScript = GetComponent<SimpleGravity>();
        _levelSystem = GetComponent<LevelSystem>(); 
        _health = GetComponent<Health>();                          
        _baseAnimator = _animator.runtimeAnimatorController; // 記住原本的動畫控制器，之後換武器動作要以它為底

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

        if (_health != null && _health.isDead) return; // 死人不能操作
      
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
            if (Time.time >= nextAttackTime) DoAttack();
            else Debug.Log("攻擊冷卻中...");
        }

        // 6. 監測 Q 鍵啟動技能(旋風斬)
        if (Input.GetKeyDown(KeyCode.Q) && !isWhirlwinding)
        {
            bool hasMelee = (_weapon != null && _weapon.weaponKind == WeaponKind.Melee);
            if (!hasMelee) { Debug.Log("必須佩帶【近戰武器】才能使用旋風斬！"); return; }
            if (Time.time >= nextWhirlwindTime) StartCoroutine(WhirlwindRoutine());
            else Debug.Log($"<color=orange>旋風斬冷卻中... 剩餘 {nextWhirlwindTime - Time.time:F1} 秒</color>");
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
            if (!isWhirlwinding)  _animator.SetFloat("Speed", speed);
        }
        else _animator.SetFloat("Speed", 0f);
        if (_gravityScript != null) finalVelocity.y = _gravityScript.verticalVelocity;
        _controller.Move(finalVelocity * Time.deltaTime);
    }
    public void RefreshWeapon()
    {
        _weapon = EquipmentSystem.Instance.GetEquipped(EquipSlotType.Weapon);

        // 換裝第一步：不管新武器是什麼，先把手上舊的模型拆掉
        if (_weaponModelInstance != null) Destroy(_weaponModelInstance);

        if (_weapon == null)
        {
            // 空手
            _animator.runtimeAnimatorController = _baseAnimator;
            _animator.SetInteger("WeaponType", 0);
            return;
        }

        // 換攻擊動作（目前只換近戰那格；弓用內建 BowShot 動作）
        if (_weapon.attackAnimation != null && _weapon.weaponKind == WeaponKind.Melee)
        {
            AnimatorOverrideController aoc = new AnimatorOverrideController(_baseAnimator);
            aoc["MeeleeAttack_TwoHanded"] = _weapon.attackAnimation;
            _animator.runtimeAnimatorController = aoc;
        }
        else
        {
            _animator.runtimeAnimatorController = _baseAnimator;
        }

        // 依武器種類自動適配動畫層：1=近戰、2=弓（換控制器後才能設參數）
        _animator.SetInteger("WeaponType", _weapon.weaponKind == WeaponKind.Bow ? 2 : 1);

        // 依武器種類選掛載的手：弓掛左手，其他掛右手
        Transform socket = (_weapon.weaponKind == WeaponKind.Bow && lefthandSocket != null)
            ? lefthandSocket
            : righthandSocket;
        // 生成武器模型
        if (_weapon.weaponPrefab != null && socket != null)
        {
            _weaponModelInstance = Instantiate(_weapon.weaponPrefab, socket);
            _weaponModelInstance.transform.localPosition = _weapon.gripPosition;
            _weaponModelInstance.transform.localEulerAngles = _weapon.gripRotation;
        }
    }
    void RefreshArmors()
    {
        // TODO：防具外觀掛載（頭盔等），之後做
        EquipmentData Helmet = EquipmentSystem.Instance.GetEquipped(EquipSlotType.Helmet);
        EquipmentData Body = EquipmentSystem.Instance.GetEquipped(EquipSlotType.Body);
        EquipmentData Feet = EquipmentSystem.Instance.GetEquipped(EquipSlotType.Feet);
        EquipmentData Hands = EquipmentSystem.Instance.GetEquipped(EquipSlotType.Hands);
    }

    void DoAttack()
    {

        // 數值一律問武器資料卡；空手用基礎值
        float range = (_weapon != null) ? _weapon.attackRange : playerAttackRange;
        float cooldown = (_weapon != null) ? _weapon.attackCooldown : fistCooldown;
        cooldown /= _attackSpeedMultiplier; // 攻速 Buff

        float multiplier = (_levelSystem != null) ? _levelSystem.attackMultiplier : 1.0f;
        float bonus = (_weapon != null) ? _weapon.attackBonus : 0f;
        float finalDamage = (playerDamage + bonus) * multiplier;
        nextAttackTime = Time.time + cooldown;
        if (_weapon != null && _weapon.weaponKind == WeaponKind.Bow)
        {
            // 弓：現在開始拉弓，冷卻結束的瞬間才放箭結算
            StartCoroutine(BowShotRoutine(cooldown, finalDamage, range));
        }
        else
        {
            // 近戰／空手：點擊瞬間結算
            _animator.SetTrigger("Attack");
            AudioManager.Instance.PlaySE(swordSound);
            MeleeHit(finalDamage, range);
        }
    }

    // 近戰命中判定：扇形範圍內全部結算
    void MeleeHit(float damage, float range)
    {
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, range);
        foreach (Collider enemy in hitEnemies)
        {
            if (!enemy.CompareTag("Enemy")) continue;
            if (!InAttackAngle(enemy.transform)) continue;
            ApplyDamage(enemy, damage);
        }
    }
    // 弓：拉弓蓄力 → 時間到才放箭（傷害延遲結算，打斷條件：死亡）
    IEnumerator BowShotRoutine(float drawTime, float damage, float range)
    {
        _animator.SetTrigger("Attack"); // WeaponType=2 時會走 BowShot 狀態
        Debug.Log($"拉弓蓄力中...{drawTime:F1} 秒");

        yield return new WaitForSeconds(drawTime);

        // 蓄力期間死掉就不放箭（腳本被關閉「不會」自動停協程，要自己檢查）
        if (_health != null && _health.isDead) yield break;

        AudioManager.Instance.PlaySE(swordSound); // 放箭音效

        if (_weapon != null && _weapon.projectilePrefab != null)
        {
            // 生成點貼近胸口。太前面的話，貼臉的怪會站在「箭的出生點後面」，
            // 而射線從碰撞體內部出發不會命中它 —— 永遠射不到近身怪
            Vector3 spawnPos = transform.position + Vector3.up * 1.0f + transform.forward * 0.1f;

            // 軟自動瞄準：朝正面扇形內最近敵人的「軀幹中心」射，
            // 順便解決矮怪問題（水平直射會從哥布林頭上飛過去）
            Vector3 dir = transform.forward;
            Transform targetEnemy = FindNearestTarget(range);
            if (targetEnemy != null)
            {
                Collider tc = targetEnemy.GetComponent<Collider>();
                // bounds.center = 碰撞體的幾何中心（世界座標）——
                // 比 transform.position 可靠，因為那通常是「腳底」
                Vector3 aimPoint = (tc != null) ? tc.bounds.center : targetEnemy.position;
                dir = (aimPoint - spawnPos).normalized;
            }

            GameObject arrowObj = Instantiate(_weapon.projectilePrefab,
                                              spawnPos, Quaternion.LookRotation(dir));

            Arrow arrow = arrowObj.GetComponent<Arrow>();
            if (arrow != null) arrow.Init(damage, range);
        }
    }

    // 找正面扇形內「最近」的敵人（放箭自動瞄準用）
    Transform FindNearestTarget(float range)
    {
        Transform nearest = null;
        float nearestDist = float.MaxValue; // 先設成無限大，見一個比一個

        Collider[] hits = Physics.OverlapSphere(transform.position, range);
        foreach (Collider c in hits)
        {
            if (!c.CompareTag("Enemy")) continue;
            if (!InAttackAngle(c.transform)) continue;

            float d = Vector3.Distance(transform.position, c.transform.position);
            if (d < nearestDist)
            {
                nearestDist = d;
                nearest = c.transform;
            }
        }
        return nearest;
    }

    // 共用的正面角度判定（忽略高度差）
    bool InAttackAngle(Transform target)
    {
        Vector3 dirToEnemy = target.position - transform.position;
        dirToEnemy.y = 0;
        Vector3 myForward = transform.forward;
        myForward.y = 0;
        return Vector3.Angle(myForward, dirToEnemy.normalized) <= attackAngle / 2f;
    }

    void ApplyDamage(Collider enemy, float damage)
    {
        Health eHealth = enemy.GetComponent<Health>();
        if (eHealth != null)
        {
            eHealth.TakeDamage(damage);
            // 在畫面上畫一條綠線，代表打中了
            Debug.DrawLine(transform.position + Vector3.up, enemy.transform.position + Vector3.up, Color.green, 1.0f);
        }
    }

    // 旋風斬技能 Q
    IEnumerator WhirlwindRoutine()
    {   
            isWhirlwinding = true;

            // 進入 CD：設定下一次可以施放的時間 (目前時間 + CD 時間)
            nextWhirlwindTime = Time.time + whirlwindCooldown;

            // 【防卡死機制】：強制清除殘留的 Trigger，避免 Animator 錯亂
            _animator.ResetTrigger("Whirlwind");
            _animator.SetTrigger("Whirlwind");
            _animator.SetBool("isWhirlwinding", true);

            float bonus = (_weapon != null) ? _weapon.attackBonus : 0f;
            float elapsed = 0f;
            // 每圈傷害 = 基礎傷害 * 間隔 (例如 50 * 0.5 = 25)
            float damagePerRotation = (playerDamage + bonus) * whirlwindInterval;

            
            // 修正：條件改為總持續時間
            while (elapsed < whirlwindDuration)
            {
                // 轉到一半死掉要立刻停：協程不會因腳本被關而停止，必須自己檢查
                if (_health != null && _health.isDead) break;
                ApplyCircleDamage(damagePerRotation);
                // 修正：只保留一個等待，確保頻率正確
                yield return new WaitForSeconds(whirlwindInterval);
                elapsed += whirlwindInterval;
            }

            _animator.SetBool("isWhirlwinding", false);
            isWhirlwinding = false;        
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
                if (enemy != null)  enemy.TakeDamage(damagePerRotation);
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