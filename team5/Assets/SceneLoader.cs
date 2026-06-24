using UnityEngine;
using UnityEngine.SceneManagement; // これが重要

public class SceneLoader : MonoBehaviour
{
    public void LoadGameScene()
    {
        SceneManager.LoadScene("SampleScene01");
    }
}
