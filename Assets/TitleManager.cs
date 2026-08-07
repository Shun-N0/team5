using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [Header("切り替えるUIパネル")]
    public GameObject titleLayout;      
    public GameObject stageSelectPanel; 
    public GameObject howToPlayPanel;   

    [Header("ステージボタン設定")]
    public Button[] stageButtons;       // Stage1, 2, 3のボタンを順番に入れる
    public GameObject stage0Button;     // ★追加：全クリア後に出現するステージ0ボタン
    public GameObject endlessButton;    // エンドレスモード用のボタン

    [Header("開発者用：全ステージ開放")]
    public bool debugUnlockAll = false; 

    void Start()
    {
        // 起動時はタイトルだけ表示し、他は隠す
        BackToTitle();
    }

    // --- パネル切り替え機能 ---

    public void ShowStageSelect()
    {
        if (titleLayout != null) titleLayout.SetActive(false);
        if (stageSelectPanel != null) stageSelectPanel.SetActive(true);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);

        UpdateStageButtons();
    }

    public void ShowHowToPlay()
    {
        if (titleLayout != null) titleLayout.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
        if (stageSelectPanel != null) stageSelectPanel.SetActive(false);
    }

    public void BackToTitle()
    {
        if (titleLayout != null) titleLayout.SetActive(true);
        if (stageSelectPanel != null) stageSelectPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
    }

    // --- ステージアンロック管理 ---

    public void UpdateStageButtons()
    {
        // 保存された進捗を読み込む (0:未クリア, 1:ステ1クリア, 2:ステ2クリア, 3:ステ3クリア)
        int progress = PlayerPrefs.GetInt("StageProgress", 0);

        for (int i = 0; i < stageButtons.Length; i++)
        {
            if (stageButtons[i] == null) continue;

            // 通常のアンロック判定（ステージ1は常に表示。それ以外は前ステージクリアが条件）
            bool isUnlocked = (i == 0) || (progress >= i);
            if (debugUnlockAll) isUnlocked = true;

            stageButtons[i].gameObject.SetActive(isUnlocked);
            stageButtons[i].interactable = true;
        }

        // ★全ステージ（リストに入れた分すべて）をクリアしたかの判定
        bool allCleared = (progress >= stageButtons.Length) || debugUnlockAll;

        // ★ステージ0ボタンをエンドレスと同じタイミングで出す
        if (stage0Button != null)
        {
            stage0Button.SetActive(allCleared);
        }

        // エンドレスボタンを出す
        if (endlessButton != null)
        {
            endlessButton.SetActive(allCleared);
        }
    }

    // --- シーン読み込み機能 ---

    public void LoadStage(int num)
    {
        Time.timeScale = 1f;
        // 0 を渡せば SampleScene00、1 なら SampleScene01 を読み込む
        SceneManager.LoadScene("SampleScene0" + num);
    }

    public void LoadEndless()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("EndlessScene");
    }

    public void ResetSaveData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("すべてのセーブデータを削除しました");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}