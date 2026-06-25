using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public void RetryGame()
    {
        // ★修正：時間を動かし、メモしておいたシーンを読み込む
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