using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("切り替えるUI")]
    public GameObject titleLayout;      // 今のStartボタンが入っているグループ
    public GameObject stageSelectPanel; // さっき作ったステージ選択パネル

    // 1. Startボタンが押されたとき
    public void ShowStageSelect()
    {
        titleLayout.SetActive(false);      // タイトルとStartボタンを隠す
        stageSelectPanel.SetActive(true); // ステージ選択を出す
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
}