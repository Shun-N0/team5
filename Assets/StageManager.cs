using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StageManager : MonoBehaviour
{
    private const string HighScoreKey = "HighScore";
    private const string SavedSceneKey = "SavedScene";
    private const string RankingCountKey = "RankingCount";
    private const string RankingScoreKey = "Ranking_";
    private const int MaxRankingCount = 5; 

    public static StageManager Instance { get; private set; }

    [Header("モード設定")]
    public bool isEndless = false;   

    [Header("UI表示")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private GameObject[] hpCircles; 

    [Header("通常ステージ用 設定")]
    public GameObject bossPrefab;    
    public int clearThreshold = 2000; 

    [Header("エンドレスモード用 設定")]
    public List<GameObject> bossPrefabs; 
    public int bossIntervalScore = 5000; 
    
    private int score = 0;
    private int highScore = 0;
    private int nextBossScore;           
    private int currentBossIndex = 0;    
    private bool isBossActive = false;   
    private bool isConditionMet = false; 
    private bool cleared;
    private bool gameOver;
    private AudioSource seAudioSource;

    void Awake()
    {
        // ★重複防止処理：新しいシーンのManagerを常に優先する
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        // SE再生用のAudioSourceを準備
        seAudioSource = GetComponent<AudioSource>();
        if (seAudioSource == null) seAudioSource = gameObject.AddComponent<AudioSource>();
        
        nextBossScore = bossIntervalScore; 
    }

    void Start()
    {
        // ★重要：シーンが始まったら必ず時間を動かす
        Time.timeScale = 1f;

        // リトライ用に今のシーン名を保存
        PlayerPrefs.SetString(SavedSceneKey, SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();

        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        UpdateScoreUI();
        UpdateHighScoreUI();
        UpdateHPUI(hpCircles != null ? hpCircles.Length : 3);
    }

    public void PlaySE(AudioClip clip, float volume) { if (clip != null) seAudioSource.PlayOneShot(clip, volume); }
    public void AddKill() { AddScore(100); }

    public void AddScore(int addedScore)
    {
        score += addedScore;
        UpdateScoreUI();
        UpdateHighScore();

        if (isEndless)
        {
            if (!isBossActive && score >= nextBossScore)
            {
                isBossActive = true;
                SpawnEndlessBoss();
            }
        }
        else
        {
            if (!isConditionMet && score >= clearThreshold)
            {
                isConditionMet = true;
                SpawnNormalBoss();
            }
        }
    }

    void SpawnNormalBoss()
    {
        if (bossPrefab != null)
        {
            if (EnemySpawner.Instance != null) EnemySpawner.Instance.StopSpawning();
            Instantiate(bossPrefab, new Vector3(0, 6, 0), Quaternion.identity);
        }
    }

    void SpawnEndlessBoss()
    {
        if (bossPrefabs == null || bossPrefabs.Count == 0) return;
        if (EnemySpawner.Instance != null) EnemySpawner.Instance.StopSpawning();
        GameObject bossToSpawn = bossPrefabs[currentBossIndex];
        Instantiate(bossToSpawn, new Vector3(0, 6, 0), Quaternion.identity);
        currentBossIndex = (currentBossIndex + 1) % bossPrefabs.Count;
    }

    public void OnBossDefeated()
    {
        if (isEndless)
        {
            isBossActive = false;
            nextBossScore = score + bossIntervalScore;
            if (EnemySpawner.Instance != null) EnemySpawner.Instance.ResumeSpawning();
        }
        else
        {
            TriggerClear();
        }
    }

    public void UpdateHP(int currentLives) { UpdateHPUI(currentLives); }

    public void TriggerClear()
    {
        if (cleared) return;
        cleared = true;

        // ステージ進捗の保存
        int currentStageNum = 1; 
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.Contains("01")) currentStageNum = 1;
        else if (sceneName.Contains("02")) currentStageNum = 2;
        else if (sceneName.Contains("03")) currentStageNum = 3;

        int savedProgress = PlayerPrefs.GetInt("StageProgress", 0);
        if (currentStageNum > savedProgress)
        {
            PlayerPrefs.SetInt("StageProgress", currentStageNum);
            PlayerPrefs.Save();
        }

        PlayerPrefs.SetInt("ClearScore", score);
        SaveScoreToRanking(score);
        SceneManager.LoadScene("Clear Game");
    }

    public void TriggerGameOver()
    {
        if (gameOver) return;
        gameOver = true;
        PlayerPrefs.SetInt("ClearScore", score);
        PlayerPrefs.Save();

        if (isEndless) SceneManager.LoadScene("EndlessResultScene");
        else SceneManager.LoadScene("GameOverScene");
    }

    private void SaveScoreToRanking(int newScore)
    {
        int count = PlayerPrefs.GetInt(RankingCountKey, 0);
        List<int> scores = new List<int>();
        for (int i = 0; i < count; i++) scores.Add(PlayerPrefs.GetInt(RankingScoreKey + i, 0));
        scores.Add(newScore);
        scores.Sort((a, b) => b.CompareTo(a));
        if (scores.Count > MaxRankingCount) scores.RemoveRange(MaxRankingCount, scores.Count - MaxRankingCount);
        PlayerPrefs.SetInt(RankingCountKey, scores.Count);
        for (int i = 0; i < scores.Count; i++) PlayerPrefs.SetInt(RankingScoreKey + i, scores[i]);
        PlayerPrefs.Save();
    }

    private void UpdateScoreUI() { if (scoreText != null) scoreText.text = "Score: " + score.ToString("N0"); }
    private void UpdateHighScore() { if (score > highScore) { highScore = score; PlayerPrefs.SetInt(HighScoreKey, highScore); UpdateHighScoreUI(); } }
    private void UpdateHighScoreUI() { if (highScoreText != null) highScoreText.text = "High Score: " + highScore.ToString("N0"); }
    private void UpdateHPUI(int currentLives) { if (hpCircles == null) return; for (int i = 0; i < hpCircles.Length; i++) if (hpCircles[i] != null) hpCircles[i].SetActive(i < currentLives); }
}