using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StageManager : MonoBehaviour
{
    private const string HighScoreKey = "HighScore";
    private const string RankingCountKey = "RankingCount";
    private const string RankingScoreKey = "Ranking_";
    private const int MaxRankingCount = 5; 

    public static StageManager Instance { get; private set; }

    [Header("スコア表示")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    private int score = 0;
    private int highScore = 0;

    [Header("HP表示（赤丸アイコン）")]
    [SerializeField] private GameObject[] hpCircles; 

    // --- ★ここがステージごとの切り替え設定 ---
    [Header("ステージクリア条件")]
    public GameObject goalLine;      // ステージ1用：GoalLineオブジェクトを入れる
    public GameObject bossPrefab;    // ステージ2用：ボスのプレハブを入れる
    public int clearThreshold = 2000; // クリア（ボス出現）に必要なスコア
    private bool isConditionMet = false; 

    private bool cleared;
    private bool gameOver;
    private AudioSource seAudioSource;

    void Awake()
    {
        Instance = this;
        seAudioSource = gameObject.AddComponent<AudioSource>();
        
        // ゴールラインが設定されている場合は、最初は隠しておく
        if (goalLine != null) goalLine.SetActive(false);
    }

    void Start()
    {
        PlayerPrefs.SetString("SavedScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();

        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        UpdateScoreUI();
        UpdateHighScoreUI();
        UpdateHPUI(hpCircles != null ? hpCircles.Length : 3);
    }

    public void AddScore(int addedScore)
    {
        score += addedScore;
        UpdateHighScore();
        UpdateScoreUI();
        UpdateHighScoreUI();

        // 目標スコアに達した時の判定
        if (!isConditionMet && score >= clearThreshold)
        {
            isConditionMet = true;
            HandleStageGoal();
        }
    }

    // スコア達成時の挙動
    void HandleStageGoal()
    {
        // ステージ1：ゴールラインを出現させる
        if (goalLine != null)
        {
            goalLine.SetActive(true);
            Debug.Log("ゴールライン出現！");
        }

        // ステージ2：ボスを出現させる
        if (bossPrefab != null)
        {
            if (EnemySpawner.Instance != null) EnemySpawner.Instance.StopSpawning();
            Instantiate(bossPrefab, new Vector3(0, 6, 0), Quaternion.identity);
            Debug.Log("ボス出現！");
        }
    }

    // --- 以下、ランキングやHP更新の既存コードはそのまま ---
    public void PlaySE(AudioClip clip, float volume) { if (clip != null) seAudioSource.PlayOneShot(clip, volume); }
    public void AddKill() { AddScore(100); }
    public void UpdateHP(int currentLives) { UpdateHPUI(currentLives); }
    
    public void TriggerClear()
    {
        if (cleared) return;
        cleared = true;
        PlayerPrefs.SetInt("ClearScore", score);
        SaveScoreToRanking(score);
        SceneManager.LoadScene("Clear Game");
    }

    public void TriggerGameOver()
    {
        if (gameOver) return;
        gameOver = true;
        SceneManager.LoadScene("GameOverScene");
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

    private void UpdateScoreUI() { if (scoreText != null) scoreText.text = "Score: " + score; }
    private void UpdateHighScore() { if (score > highScore) { highScore = score; PlayerPrefs.SetInt(HighScoreKey, highScore); PlayerPrefs.Save(); } }
    private void UpdateHighScoreUI() { if (highScoreText != null) highScoreText.text = "High Score: " + highScore; }
    private void UpdateHPUI(int currentLives) {
        if (hpCircles == null) return;
        for (int i = 0; i < hpCircles.Length; i++) if (hpCircles[i] != null) hpCircles[i].SetActive(i < currentLives);
    }
}