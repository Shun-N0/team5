using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public void RetryGame()
    {
        RetrySceneStorage.LoadRetryScene();
    }

    public void GoToTitle()
    {
        SceneManager.LoadScene("Title Scene");
    }
}
