using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void LoadMainScene()
    {
        // Sahne adının Unity Build Settings'de yazdığı gibi olduğundan emin ol!
        SceneManager.LoadScene("MainScene");
    }

    public void LoadMapScene()
    {
        // Sahne adının Unity Build Settings'de yazdığı gibi olduğundan emin ol!
        SceneManager.LoadScene("MapScene");
    }
}