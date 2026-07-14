using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Boss2 : MonoBehaviour
{
    [Header("基本設定")]
    public int hp = 50;
    public float stopY = 3.0f;       
    public float moveSpeed = 1.5f;   
    public float attackInterval = 2.0f; 

    [Header("弾のプレハブ")]
    public GameObject normalBullet;
    public GameObject stunBullet;

    [Header("1. 全方位弾幕 (Ring)")]
    public float ringBulletSpeed = 2.5f;
    public float ringBulletScale = 0.3f;
    public int ringBulletCount = 36;     

    [Header("2. 3方向スタン攻撃 (Stun)")]
    public float stunBulletSpeed = 4.0f;
    public float stunBulletScale = 0.8f;

    [Header("3. 自機狙い連射 (Burst)")]
    public float burstBulletSpeed = 6.0f;
    public float burstBulletScale = 0.6f;
    public int burstCount = 5;           

    [Header("4. 横一列・壁攻撃 (Line)")]
    public float lineBulletSpeed = 3.0f;
    public float lineBulletScale = 0.7f;
    public int lineBulletCount = 10;     
    public float lineRangeX = 2.2f;      

    // ★追加：ヒット演出用の設定
    [Header("被弾エフェクト")]
    public Color hitColor = Color.white; // 当たった時の色
    public float flashDuration = 0.05f;  // 光る時間（一瞬にするのがコツ）

    private bool isReady = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor; // 元の色を保存する用

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // 最初のエネミーの色を覚えておく
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    void Update()
    {
        if (!isReady)
        {
            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
            if (transform.position.y <= stopY)
            {
                isReady = true;
                StartCoroutine(AttackRoutine());
            }
        }
    }

    IEnumerator AttackRoutine()
    {
        while (hp > 0)
        {
            yield return new WaitForSeconds(attackInterval);
            int attackType = Random.Range(0, 4);
            if (attackType == 0) RingShot();
            else if (attackType == 1) StunTripleShot();
            else if (attackType == 2) StartCoroutine(TargetBurstRoutine());
            else LineShot();
        }
    }

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

    IEnumerator TargetBurstRoutine()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) yield break;
        for (int i = 0; i < burstCount; i++)
        {
            Vector3 dir = (player.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
            GameObject b = Instantiate(normalBullet, transform.position, Quaternion.Euler(0, 0, angle));
            SetupBullet(b, burstBulletSpeed, burstBulletScale);
            yield return new WaitForSeconds(0.2f);
        }
    }

    void LineShot()
    {
        for (int i = 0; i < lineBulletCount; i++)
        {
            float t = (float)i / (lineBulletCount - 1);
            float posX = Mathf.Lerp(-lineRangeX, lineRangeX, t);
            Vector3 spawnPos = new Vector3(posX, transform.position.y - 0.5f, 0);
            GameObject b = Instantiate(normalBullet, spawnPos, Quaternion.identity);
            SetupBullet(b, lineBulletSpeed, lineBulletScale);
        }
    }

    void SetupBullet(GameObject bulletObj, float speed, float scale)
    {
        bulletObj.transform.localScale = Vector3.one * scale;
        bulletObj.SendMessage("SetSpeed", speed, SendMessageOptions.DontRequireReceiver);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            Destroy(collision.gameObject);
            hp--;

            // ★追加：当たった瞬間に光らせる
            StartCoroutine(FlashRoutine());

            if (hp <= 0) Defeat();
        }
    }

    // ★追加：一瞬だけ白く光らせるコルーチン
    IEnumerator FlashRoutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitColor;      // 白くする
            yield return new WaitForSeconds(flashDuration); // 少し待つ
            spriteRenderer.color = originalColor; // 元の色に戻す
        }
    }

    void Defeat()
    {
        Debug.Log("ボスを倒した！");
        SceneManager.LoadScene("Clear Game");
        Destroy(gameObject);
    }
}