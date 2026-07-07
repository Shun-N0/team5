using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public GameObject shieldItemPrefab;
    
    [Header("出現のタイミング設定")]
    public float spawnCooldown = 20.0f; 
    
    // ★追加：アイテムの落ちる速さをここで設定できるようにする
    [Header("アイテムの移動設定")]
    public float itemFallSpeed = 2.0f; 

    private float timer;
    private bool canSpawn = true;

    void Update()
    {
        if (!canSpawn)
        {
            timer += Time.deltaTime;
            if (timer >= spawnCooldown)
            {
                canSpawn = true;
                timer = 0;
            }
        }
        else
        {
            if (Random.value < 0.01f)
            {
                SpawnShieldItem();
            }
        }
    }

    void SpawnShieldItem()
    {
        canSpawn = false;
        float randomX = Random.Range(-2.1f, 2.1f);
        
        // 1. まずアイテムを生成して、変数「newItem」に入れる
        GameObject newItem = Instantiate(shieldItemPrefab, new Vector3(randomX, 6f, 0), Quaternion.identity);
        
        // 2. ★生成したアイテムについている「Item」スクリプトを探して、速さを設定する
        EvolveItem itemScript = newItem.GetComponent<EvolveItem>();
        if (itemScript != null)
        {
            itemScript.fallSpeed = itemFallSpeed;
        }
    }
}