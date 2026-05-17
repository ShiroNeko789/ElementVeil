using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(TransitionOut(sceneName));
    }

    IEnumerator TransitionOut(string sceneName)
    {
        // If SceneWipe exists in current scene, wipe to black first
        if (SceneWipe.Instance != null)
            yield return StartCoroutine(SceneWipe.Instance.WipeIn());
        else
            yield return new WaitForSeconds(0.2f);

        // Then go to loading screen
        LoadingScreen.TargetScene = sceneName;
        SceneManager.LoadScene("LoadingScreen");
    }
}