using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StageManager : MonoBehaviour
{
    private const string HighScoreKey = "HighScore";
    private const string RankingCountKey = "RankingCount";
    private const string RankingScoreKey = "Ranking_";
    private const int MaxRankingCount = 5; // ランキングの最大保存件数

    public static StageManager Instance { get; private set; }

    [Header("ステージクリア")]
    [SerializeField] private GameObject clearPanel;
    [SerializeField] private TextMeshProUGUI clearText;

    [Header("スコア表示")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    private int score = 0;
    private int highScore = 0;

    [Header("HP表示（赤丸アイコン）")]
    [SerializeField] private GameObject[] hpCircles; 

    private bool cleared;
    private bool gameOver;

    // ★追加：SE再生用のコンポーネント
    private AudioSource seAudioSource;

    void Awake()
    {
        Instance = this;
        RetrySceneStorage.SaveCurrentScene();
        // ★追加：自分自身にSE専用のAudioSourceを付ける
        seAudioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        UpdateScoreUI();
        UpdateHighScoreUI();
        UpdateHPUI(hpCircles != null ? hpCircles.Length : 3);
    }

    // 外から音を鳴らすための命令
    // これを使うと、複数の音が重なっても打ち消し合わずに全て鳴ります
    public void PlaySE(AudioClip clip, float volume)
    {
        if (clip != null && seAudioSource != null)
        {
            seAudioSource.PlayOneShot(clip, volume);
        }
    }

    public void AddKill()
    {
        AddScore(100);
    }

    public void AddScore(int addedScore)
    {
        score += addedScore;
        UpdateHighScore();
        UpdateScoreUI();
        UpdateHighScoreUI();
    }

    public void UpdateHP(int currentLives)
    {
        UpdateHPUI(currentLives);
    }

    public void TriggerClear()
    {
        if (cleared) return;
        cleared = true;

        // スコアをクリア画面に引き渡すためPlayerPrefsに保存
        PlayerPrefs.SetInt("ClearScore", score);

        // ランキングにスコアを追加して保存
        SaveScoreToRanking(score);

        // クリア専用シーンへ遷移
        SceneManager.LoadScene("Clear Game");
    }

    public void TriggerGameOver()
    {
        if (gameOver) return;
        gameOver = true;
        SceneManager.LoadScene("GameOverScene");
    }

    // スコアを既存ランキングに追加し、上位 MaxRankingCount 件のみ保存する
    private void SaveScoreToRanking(int newScore)
    {
        int count = PlayerPrefs.GetInt(RankingCountKey, 0);

        // 既存スコアを読み込む
        List<int> scores = new List<int>();
        for (int i = 0; i < count; i++)
            scores.Add(PlayerPrefs.GetInt(RankingScoreKey + i, 0));

        // 今回のスコアを追加して降順ソート
        scores.Add(newScore);
        scores.Sort((a, b) => b.CompareTo(a));

        // 上位 MaxRankingCount 件に絞る
        if (scores.Count > MaxRankingCount)
            scores.RemoveRange(MaxRankingCount, scores.Count - MaxRankingCount);

        // PlayerPrefs に保存
        PlayerPrefs.SetInt(RankingCountKey, scores.Count);
        for (int i = 0; i < scores.Count; i++)
            PlayerPrefs.SetInt(RankingScoreKey + i, scores[i]);
        PlayerPrefs.Save();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    private void UpdateHighScore()
    {
        if (score <= highScore) return;

        highScore = score;
        PlayerPrefs.SetInt(HighScoreKey, highScore);
        PlayerPrefs.Save();
    }

    private void UpdateHighScoreUI()
    {
        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore;
    }

    private void UpdateHPUI(int currentLives)
    {
        if (hpCircles == null) return;
        for (int i = 0; i < hpCircles.Length; i++)
        {
            if (hpCircles[i] != null)
                hpCircles[i].SetActive(i < currentLives);
        }
    }
}
