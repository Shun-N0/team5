using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    // ゲームオーバー画面が表示されてから、ボタン入力を受け付けるまでの最低待ち時間（秒）
    [Header("入力ガード")]
    [SerializeField] private float inputGuardSeconds = 0.5f;

    // この時刻になるまでボタン入力を無視する
    private float allowInputAfterTime;
    // シーン開始後に一度マウスを離したか
    private bool mouseReleasedSinceStart;
    // シーン開始後に「離してから新たに押した」かどうか。これが true になるまでボタンクリックを通さない。
    private bool freshMousePressDetected;

    void Start()
    {
        // 死亡時に射撃ボタン（左クリック）を押しっぱなしのままシーン遷移すると
        // リトライボタンが即押し扱いになってしまうのを防ぐ
        allowInputAfterTime = Time.unscaledTime + inputGuardSeconds;
        mouseReleasedSinceStart = false;
        freshMousePressDetected = false;
    }

    void Update()
    {
        // マウス左ボタン（射撃キーと同じ）が一度離されたことを記録する
        if (!Input.GetMouseButton(0))
        {
            mouseReleasedSinceStart = true;
        }
        // 「一度離してから新たに押した」場合のみ、本当のクリック意思とみなす
        // これにより、射撃で押しっぱなしのままシーン遷移しても、離した瞬間にボタンが発火しない
        else if (mouseReleasedSinceStart)
        {
            freshMousePressDetected = true;
        }
    }

    // 入力ガード中かどうか
    private bool IsInputBlocked()
    {
        return !freshMousePressDetected || Time.unscaledTime < allowInputAfterTime;
    }

    public void RetryGame()
    {
        // 射撃ボタン長押しからのシーン遷移直後の誤クリックを防ぐ
        if (IsInputBlocked()) return;

        // ★修正：時間を動かし、メモしておいたシーンを読み込む
        Time.timeScale = 1f;
        string sceneToLoad = PlayerPrefs.GetString("SavedScene", "SampleScene01");
        SceneManager.LoadScene(sceneToLoad);
    }

    public void GoToTitle()
    {
        // 射撃ボタン長押しからのシーン遷移直後の誤クリックを防ぐ
        if (IsInputBlocked()) return;

        Time.timeScale = 1f;
        SceneManager.LoadScene("Title Scene");
    }
}
