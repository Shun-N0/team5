using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ClearManager : MonoBehaviour
{
    // ★ここを[SerializeField]のまま残しておけば、Unity上でエラーは出ません
    [Header("スコア表示")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("ランキング表示")]
    [SerializeField] private TextMeshProUGUI rankingText;

    void Start()
    {
        // ★修正：スコアを表示する処理をコメントアウト（無効化）します
        /* 
        int score = PlayerPrefs.GetInt("ClearScore", 0);
        if (scoreText != null) scoreText.text = "Score: " + score;
        */

        DisplayRanking();
    }

    private void DisplayRanking()
    {
        // （ここは今のままでOKです。ランキングには自分のスコアが表示されます）
        if (rankingText == null) return;

        int currentScore = PlayerPrefs.GetInt("ClearScore", 0);
        int count = PlayerPrefs.GetInt("RankingCount", 0);
        bool highlighted = false; 

        string text = "<b>== RANKING ==</b>\n";
        for (int i = 0; i < count; i++)
        {
            int s = PlayerPrefs.GetInt("Ranking_" + i, 0);
            string line = (i + 1) + ".  " + s;

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
        Time.timeScale = 1f;
        string sceneToLoad = PlayerPrefs.GetString("SavedScene", "SampleScene01");
        SceneManager.LoadScene(sceneToLoad);
    }

    public void GoToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Title Scene");
    }
}