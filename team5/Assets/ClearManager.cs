using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// クリア画面（Clear Game シーン）の管理スクリプト
public class ClearManager : MonoBehaviour
{
    [Header("スコア表示")]
    [SerializeField] private TextMeshProUGUI scoreText;    // 今回のスコア

    [Header("ランキング表示")]
    [SerializeField] private TextMeshProUGUI rankingText;  // 上位スコア一覧

    void Start()
    {
        // 今回のスコアを表示
        int score = PlayerPrefs.GetInt("ClearScore", 0);
        if (scoreText != null)
            scoreText.text = "Score: " + score;

        // ランキングを表示
        DisplayRanking();
    }

    // PlayerPrefs に保存されたランキングを読み込んで表示する
    // 今回のスコアと一致する最初のエントリを金色でハイライトする
    private void DisplayRanking()
    {
        if (rankingText == null) return;

        int currentScore = PlayerPrefs.GetInt("ClearScore", 0);
        int count = PlayerPrefs.GetInt("RankingCount", 0);
        bool highlighted = false; // 今回スコアのハイライトは最初の1件のみ

        string text = "<b>== RANKING ==</b>\n";
        for (int i = 0; i < count; i++)
        {
            int s = PlayerPrefs.GetInt("Ranking_" + i, 0);
            string line = (i + 1) + ".  " + s;

            // 今回のスコアと一致する最初のエントリを金色でハイライト
            if (!highlighted && s == currentScore)
            {
                text += "<color=#FFD700><b>" + line + " << YOU</b></color>\n";
                highlighted = true;
            }
            else
            {
                text += line + "\n";
            }
        }

        rankingText.text = text;
    }

    public void RetryGame()
    {
        RetrySceneStorage.LoadRetryScene();
    }

    // タイトルボタン用：タイトル画面（Title Scene）へ
    public void GoToTitle()
    {
        SceneManager.LoadScene("Title Scene");
    }
}
