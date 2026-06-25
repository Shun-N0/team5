using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Boss : MonoBehaviour
{
    [Header("基本設定")]
    public int hp = 50;
    public float stopY = 3.0f;       // ボスが止まる高さ
    public float moveSpeed = 1.5f;   // 登場時の移動速度
    public float attackInterval = 2.0f; // 攻撃と攻撃の間隔

    [Header("弾のプレハブ")]
    public GameObject normalBullet;
    public GameObject stunBullet;

    [Header("1. 全方位弾幕 (Ring)")]
    public float ringBulletSpeed = 2.5f;
    public float ringBulletScale = 0.3f; // 弾のサイズ（小さめ推奨）
    public int ringBulletCount = 36;     // ★弾の数（多いほど密集します）

    [Header("2. 3方向スタン攻撃 (Stun)")]
    public float stunBulletSpeed = 4.0f;
    public float stunBulletScale = 0.8f;

    [Header("3. 自機狙い連射 (Burst)")]
    public float burstBulletSpeed = 6.0f;
    public float burstBulletScale = 0.6f;
    public int burstCount = 5;           // 何連射するか

    [Header("4. 横一列・壁攻撃 (Line)")]
    public float lineBulletSpeed = 3.0f;
    public float lineBulletScale = 0.7f;
    public int lineBulletCount = 10;     // 横に並べる数（多いと避けられません）
    public float lineRangeX = 2.2f;      // 横に広がる幅

    private bool isReady = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 1. 指定の位置まで降りてくる処理
        if (!isReady)
        {
            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
            if (transform.position.y <= stopY)
            {
                isReady = true;
                StartCoroutine(AttackRoutine()); // 定位置に着いたら攻撃開始
            }
        }
    }

    // 攻撃をループさせるコルーチン
    IEnumerator AttackRoutine()
    {
        while (hp > 0)
        {
            yield return new WaitForSeconds(attackInterval);
            
            // 4種類の攻撃からランダムに選ぶ
            int attackType = Random.Range(0, 4);
            
            if (attackType == 0) RingShot();
            else if (attackType == 1) StunTripleShot();
            else if (attackType == 2) StartCoroutine(TargetBurstRoutine());
            else LineShot();
        }
    }

    // --- 攻撃パターン1：全方位弾幕 ---
    void RingShot()
    {
        float angleStep = 360f / ringBulletCount;
        float angle = 0f;

        for (int i = 0; i < ringBulletCount; i++)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            GameObject b = Instantiate(normalBullet, transform.position, rotation);
            SetupBullet(b, ringBulletSpeed, ringBulletScale);
            angle += angleStep;
        }
    }

    // --- 攻撃パターン2：3方向スタン攻撃 ---
    void StunTripleShot()
    {
        float[] angles = { -25f, 0f, 25f };
        foreach (float angle in angles)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            GameObject b = Instantiate(stunBullet, transform.position, rotation);
            SetupBullet(b, stunBulletSpeed, stunBulletScale);
        }
    }

    // --- 攻撃パターン3：自機狙い連射 ---
    IEnumerator TargetBurstRoutine()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) yield break;

        for (int i = 0; i < burstCount; i++)
        {
            // 自機の方角を計算
            Vector3 dir = (player.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
            
            GameObject b = Instantiate(normalBullet, transform.position, Quaternion.Euler(0, 0, angle));
            SetupBullet(b, burstBulletSpeed, burstBulletScale);
            
            yield return new WaitForSeconds(0.2f); // 連射の間隔
        }
    }

    // --- 攻撃パターン4：横一列・壁攻撃 ---
    void LineShot()
    {
        for (int i = 0; i < lineBulletCount; i++)
        {
            // 横に等間隔で並べる
            float t = (float)i / (lineBulletCount - 1);
            float posX = Mathf.Lerp(-lineRangeX, lineRangeX, t);
            
            Vector3 spawnPos = new Vector3(posX, transform.position.y - 0.5f, 0);
            GameObject b = Instantiate(normalBullet, spawnPos, Quaternion.identity);
            SetupBullet(b, lineBulletSpeed, lineBulletScale);
        }
    }

    // 弾のスピードとサイズを一括設定するヘルパー
    void SetupBullet(GameObject bulletObj, float speed, float scale)
    {
        bulletObj.transform.localScale = Vector3.one * scale;
        // BulletスクリプトやHomingBulletスクリプトのSetSpeedを呼び出す
        bulletObj.SendMessage("SetSpeed", speed, SendMessageOptions.DontRequireReceiver);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            Destroy(collision.gameObject); // プレイヤーの弾を消す
            hp--;
            // ダメージを受けた時に一瞬赤くするなどの演出を入れるならここ
            if (hp <= 0) Defeat();
        }
    }

    void Defeat()
    {
        Debug.Log("ボスを倒した！");
        // 勝利！クリアシーンへ移動
        SceneManager.LoadScene("Clear Game");
        Destroy(gameObject);
    }
}