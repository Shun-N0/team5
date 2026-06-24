using UnityEngine;
using UnityEngine.SceneManagement;

public static class RetrySceneStorage
{
    private const string RetrySceneKey = "RetryScene";
    private const string DefaultStageScene = "SampleScene01";

    public static void SaveCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString(RetrySceneKey, currentSceneName);
        PlayerPrefs.Save();
    }

    public static void LoadRetryScene()
    {
        string retrySceneName = PlayerPrefs.GetString(RetrySceneKey, DefaultStageScene);
        Time.timeScale = 1f;
        SceneManager.LoadScene(retrySceneName);
    }
}
