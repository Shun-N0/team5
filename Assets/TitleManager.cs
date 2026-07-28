using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // ボタン制御に必要

public class TitleManager : MonoBehaviour
{
    [Header("切り替えるUIパネル")]
    public GameObject titleLayout;      // タイトルロゴやStartボタンのグループ
    public GameObject stageSelectPanel; // ステージ選択ボタンのグループ
    public GameObject howToPlayPanel;   // 遊び方説明パネル

    [Header("ステージボタン設定")]
    public Button[] stageButtons;       // Stage1, 2, 3の順で入れる
    public GameObject endlessButton;    // エンドレスモード用のボタン

    [Header("開発者用：全ステージ開放")]
    public bool debugUnlockAll = false; 

    void Start()
    {
        // 起動時はタイトルだけ表示し、他は隠す
        BackToTitle();
    }

    // --- パネル切り替え機能 ---

    // Startボタンが押されたとき
    public void ShowStageSelect()
    {
        if (titleLayout != null) titleLayout.SetActive(false);
        if (stageSelectPanel != null) stageSelectPanel.SetActive(true);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);

        // ボタンのロック状態を更新
        UpdateStageButtons();
    }

    // 遊び方ボタンが押されたとき
    public void ShowHowToPlay()
    {
        if (titleLayout != null) titleLayout.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
        if (stageSelectPanel != null) stageSelectPanel.SetActive(false);
    }

    // 戻るボタンが押されたとき
    public void BackToTitle()
    {
        if (titleLayout != null) titleLayout.SetActive(true);
        if (stageSelectPanel != null) stageSelectPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
    }

    // --- ステージアンロック管理 ---

    public void UpdateStageButtons()
    {
        // 保存された進捗を読み込む (0:未クリア, 1:ステ1クリア, 2:ステ2クリア...)
        int progress = PlayerPrefs.GetInt("StageProgress", 0);

        for (int i = 0; i < stageButtons.Length; i++)
        {
            if (stageButtons[i] == null) continue;

            // ステージ1(i=0)は常に表示。それ以外は、前のステージをクリアしていれば表示。
            bool isUnlocked = (i == 0) || (progress >= i);
            
            // デバッグモードがONなら強制的に表示
            if (debugUnlockAll) isUnlocked = true;

            // ボタンを表示・非表示にする
            stageButtons[i].gameObject.SetActive(isUnlocked);
            stageButtons[i].interactable = true;
        }

        // 全通常ステージクリア（ボタンの数だけクリア）でエンドレスボタンを表示
        if (endlessButton != null)
        {
            bool endlessUnlocked = (progress >= stageButtons.Length) || debugUnlockAll;
            endlessButton.SetActive(endlessUnlocked);
        }
    }

    // --- シーン読み込み機能 ---

    // 通常ステージ用 (引数に 1, 2, 3 を入れる)
    public void LoadStage(int num)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene0" + num);
    }

    // エンドレスモード用
    public void LoadEndless()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("EndlessScene");
    }

    // --- 特殊機能 ---

    // 隠しリセットボタン用
    public void ResetSaveData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("すべてのセーブデータを削除しました");
        // 画面を更新するためにタイトルシーンを読み直す
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}