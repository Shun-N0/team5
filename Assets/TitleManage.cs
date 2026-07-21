using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("切り替えるUI")]
    public GameObject titleLayout;      // 今のStartボタンが入っているグループ
    public GameObject stageSelectPanel; // さっき作ったステージ選択パネル
    public GameObject howToPlayPanel;   // 遊び方を表示するパネル

    // 1. Startボタンが押されたとき
    public void ShowStageSelect()
    {
        titleLayout.SetActive(false);      // タイトルとStartボタンを隠す
        stageSelectPanel.SetActive(true); // ステージ選択を出す
    }

    // 遊び方ボタンが押されたとき：遊び方パネルを表示してタイトルを隠す
    public void ShowHowToPlay()
    {
        if (titleLayout != null) titleLayout.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
    }

    // 遊び方パネルの「戻る」ボタンが押されたとき：タイトルに戻る
    public void BackToTitle()
    {
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (stageSelectPanel != null) stageSelectPanel.SetActive(false);
        if (titleLayout != null) titleLayout.SetActive(true);
    }

    // 2. Stage 1が押されたとき
    public void LoadStage1()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene01");
    }

    // 3. Stage 2が押されたとき
    public void LoadStage2()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene02");
    }

    // 4. Stage 3が押されたとき（電撃嵐ステージ）
    public void LoadStage3()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene03");
    }
}