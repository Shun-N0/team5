using System.Collections;
using UnityEngine;

/// <summary>
/// ステージ3専用ボス「サンダーロード」。
/// 既存のボス（Boss / MechaJellyfishBoss）と差別化するため、以下の特徴を持たせている。
///   ・左右に動き回りながら攻撃する（既存ボスは静止したまま撃つ）
///   ・螺旋（スパイラル）弾幕、ホーミング魚雷、自機狙いショットガンの3種を使い分ける
///   ・HPが半分を切ると「発狂モード」に突入し、攻撃間隔が縮まり全方位リング弾も追加される
/// </summary>
public class Boss3 : MonoBehaviour
{
    [Header("基本設定")]
    public int maxHp = 90;                 // 最大HP（ステージ3のボリュームに合わせて多め）
    public int scoreValue = 1500;          // 撃破時の獲得スコア
    public float stopY = 3.2f;             // 登場後に停止するY座標
    public float entrySpeed = 1.6f;        // 登場時に下降してくる速さ
    public float attackInterval = 1.8f;    // 攻撃と攻撃の間隔（秒）

    [Header("左右移動（動き回る挙動）")]
    public float swaySpeed = 2.2f;         // 左右に動く速さ
    public float swayRangeX = 2.4f;        // 左右に動ける範囲（中心からの距離）

    [Header("弾のプレハブ")]
    public GameObject normalBullet;        // 通常弾（EnemyBullet）を割り当てる
    public GameObject homingBullet;        // ホーミング弾（HomingBullet）を割り当てる

    [Header("1. 螺旋弾幕（Spiral）")]
    public int spiralBulletCount = 24;     // 1回の螺旋で撃つ弾数
    public float spiralAngleStep = 24f;    // 1発ごとに回転させる角度
    public float spiralFireDelay = 0.05f;  // 1発ごとの発射間隔
    public float spiralBulletSpeed = 3.0f; // 螺旋弾の速さ
    public float spiralBulletScale = 0.35f;// 螺旋弾の大きさ

    [Header("2. ホーミング魚雷（Homing）")]
    public int homingCount = 3;            // 一度に放つホーミング弾の数
    public float homingSpread = 1.2f;      // 放つ位置の左右の広がり
    public float homingBulletSpeed = 3.5f; // ホーミング弾の速さ
    public float homingBulletScale = 0.6f; // ホーミング弾の大きさ

    [Header("3. 自機狙いショットガン（Shotgun）")]
    public int shotgunPellets = 7;         // 1回に撃つ散弾の数
    public float shotgunSpreadAngle = 60f; // 散弾全体の広がり角度
    public float shotgunBulletSpeed = 5.5f;// 散弾の速さ
    public float shotgunBulletScale = 0.5f;// 散弾の大きさ

    [Header("発狂モード（HP半分で突入）")]
    [Range(0f, 1f)] public float enrageHpRatio = 0.5f; // 発狂に入るHP割合
    public float enrageAttackInterval = 1.0f;          // 発狂後の攻撃間隔
    public Color enrageColor = new Color(1f, 0.45f, 0.2f, 1f); // 発狂中の体色
    public int enrageRingBulletCount = 30;             // 発狂中に追加される全方位リングの弾数
    public float enrageRingBulletSpeed = 2.6f;         // リング弾の速さ
    public float enrageRingBulletScale = 0.3f;         // リング弾の大きさ

    [Header("被弾エフェクト")]
    public Color hitColor = Color.white;   // 被弾時に一瞬光る色
    public float flashDuration = 0.05f;    // 光っている時間

    private int currentHp;
    private bool isReady = false;          // 登場が完了して攻撃を開始できる状態か
    private bool isEnraged = false;        // 発狂モードに入っているか
    private SpriteRenderer spriteRenderer;
    private Color originalColor;           // 元の体色（被弾フラッシュから戻す用）
    private float swayDirection = 1f;      // 現在の左右移動の向き（1:右 / -1:左）
    private float centerX;                 // 左右移動の中心となるX座標

    void Start()
    {
        currentHp = maxHp;
        centerX = transform.position.x;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    void Update()
    {
        if (!isReady)
        {
            // --- 登場演出：指定のY座標までゆっくり下降する ---
            transform.Translate(Vector3.down * entrySpeed * Time.deltaTime);
            if (transform.position.y <= stopY)
            {
                isReady = true;
                StartCoroutine(AttackRoutine());
            }
            return;
        }

        // --- 攻撃開始後は左右に動き回る（既存ボスとの差別化ポイント）---
        transform.Translate(Vector3.right * swayDirection * swaySpeed * Time.deltaTime);
        if (transform.position.x > centerX + swayRangeX)
        {
            transform.position = new Vector3(centerX + swayRangeX, transform.position.y, 0);
            swayDirection = -1f;
        }
        else if (transform.position.x < centerX - swayRangeX)
        {
            transform.position = new Vector3(centerX - swayRangeX, transform.position.y, 0);
            swayDirection = 1f;
        }
    }

    /// <summary>攻撃をランダムに繰り返すメインループ。</summary>
    IEnumerator AttackRoutine()
    {
        while (currentHp > 0)
        {
            // 発狂中かどうかで攻撃間隔を切り替える
            yield return new WaitForSeconds(isEnraged ? enrageAttackInterval : attackInterval);

            int attackType = Random.Range(0, 3);
            if (attackType == 0) yield return StartCoroutine(SpiralShotRoutine());
            else if (attackType == 1) HomingTorpedoes();
            else AimedShotgun();

            // 発狂中は追い打ちとして全方位リング弾も撒く
            if (isEnraged) RingShot();
        }
    }

    /// <summary>1. 螺旋弾幕：角度を少しずつ回転させながら連射する。</summary>
    IEnumerator SpiralShotRoutine()
    {
        if (normalBullet == null) yield break;

        float angle = 0f;
        for (int i = 0; i < spiralBulletCount; i++)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            GameObject b = Instantiate(normalBullet, transform.position, rotation);
            SetupBullet(b, spiralBulletSpeed, spiralBulletScale);
            angle += spiralAngleStep;
            yield return new WaitForSeconds(spiralFireDelay);
        }
    }

    /// <summary>2. ホーミング魚雷：自機を追尾する弾を左右に散らして放つ。</summary>
    void HomingTorpedoes()
    {
        if (homingBullet == null) return;

        for (int i = 0; i < homingCount; i++)
        {
            // 左右に均等に広げた発射位置を計算する
            float t = homingCount > 1 ? (float)i / (homingCount - 1) : 0.5f;
            float offsetX = Mathf.Lerp(-homingSpread, homingSpread, t);
            Vector3 spawnPos = transform.position + new Vector3(offsetX, -0.4f, 0);

            GameObject b = Instantiate(homingBullet, spawnPos, Quaternion.identity);
            SetupBullet(b, homingBulletSpeed, homingBulletScale);
        }
    }

    /// <summary>3. 自機狙いショットガン：プレイヤー方向へ扇状に散弾を撃つ。</summary>
    void AimedShotgun()
    {
        if (normalBullet == null) return;

        GameObject player = GameObject.FindWithTag("Player");
        // プレイヤーが見つからない場合は真下を基準にする
        float baseAngle = 180f;
        if (player != null)
        {
            Vector3 dir = (player.transform.position - transform.position).normalized;
            baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
        }

        for (int i = 0; i < shotgunPellets; i++)
        {
            // 扇状に均等な角度で散らす
            float t = shotgunPellets > 1 ? (float)i / (shotgunPellets - 1) : 0.5f;
            float angle = baseAngle + Mathf.Lerp(-shotgunSpreadAngle / 2f, shotgunSpreadAngle / 2f, t);
            GameObject b = Instantiate(normalBullet, transform.position, Quaternion.Euler(0, 0, angle));
            SetupBullet(b, shotgunBulletSpeed, shotgunBulletScale);
        }
    }

    /// <summary>発狂中の追い打ち：全方位に均等なリング弾を撒く。</summary>
    void RingShot()
    {
        if (normalBullet == null || enrageRingBulletCount <= 0) return;

        float angleStep = 360f / enrageRingBulletCount;
        float angle = 0f;
        for (int i = 0; i < enrageRingBulletCount; i++)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            GameObject b = Instantiate(normalBullet, transform.position, rotation);
            SetupBullet(b, enrageRingBulletSpeed, enrageRingBulletScale);
            angle += angleStep;
        }
    }

    /// <summary>弾の大きさと速さをまとめて設定するヘルパー。</summary>
    void SetupBullet(GameObject bulletObj, float speed, float scale)
    {
        bulletObj.transform.localScale = Vector3.one * scale;
        bulletObj.SendMessage("SetSpeed", speed, SendMessageOptions.DontRequireReceiver);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet")) return;

        Destroy(collision.gameObject);
        TakeDamage(1);
    }

    void TakeDamage(int damage)
    {
        currentHp -= damage;

        // 被弾時に一瞬光らせる
        StartCoroutine(FlashRoutine());

        // HPが半分を切ったら一度だけ発狂モードに突入する
        if (!isEnraged && currentHp <= maxHp * enrageHpRatio && currentHp > 0)
        {
            EnterEnrage();
        }

        if (currentHp <= 0) Defeat();
    }

    /// <summary>発狂モードへの突入処理。攻撃が激化し、体色も変わる。</summary>
    void EnterEnrage()
    {
        isEnraged = true;
        originalColor = enrageColor;                 // 以降フラッシュから戻す色も発狂色にする
        if (spriteRenderer != null) spriteRenderer.color = enrageColor;
        Debug.Log("サンダーロードが発狂した！");
    }

    /// <summary>被弾時に一瞬だけ光らせるコルーチン。</summary>
    IEnumerator FlashRoutine()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    void Defeat()
    {
        Debug.Log("サンダーロードを撃破！ステージ3クリア！");
        // スコア加算とクリア処理は StageManager に委ねる（ランキング保存まで一括で行われる）
        StageManager.Instance?.AddScore(scoreValue);
        StageManager.Instance?.TriggerClear();
        Destroy(gameObject);
    }
}
