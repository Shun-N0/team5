using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EndlessResultManager : MonoBehaviour
{
    [Header("表示用テキスト")]
    [SerializeField] private TextMeshProUGUI scoreText;     // 今回のスコア用
    [SerializeField] private TextMeshProUGUI highScoreText; // ハイスコア用

    void Start()
    {
        // 1. 時間を動かす（クリア時に止めた場合のため）
        Time.timeScale = 1f;

        // 2. 保存されているスコアを読み出す
        int currentScore = PlayerPrefs.GetInt("ClearScore", 0);
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        // 3. 画面のテキストに反映させる（\n で改行を入れる）
        if (scoreText != null) 
            scoreText.text = "SCORE\n" + currentScore.ToString("N0"); // ★\nを追加

        if (highScoreText != null) 
            highScoreText.text = "BEST\n" + highScore.ToString("N0");  // ★\nを追加
    }

    // リトライボタン用
    public void OnClickRetry()
    {
        string lastScene = PlayerPrefs.GetString("SavedScene", "SampleScene01");
        SceneManager.LoadScene(lastScene);
    }

    // タイトルボタン用
    public void OnClickTitle()
    {
        SceneManager.LoadScene("Title Scene");
    }
}