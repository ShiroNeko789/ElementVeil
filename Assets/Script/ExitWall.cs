using UnityEngine;
using System.Collections;

public class ExitWall : MonoBehaviour
{
    [Header("Settings")]
    public float slideDistance = 5f;    // how far up it moves
    public float slideDuration = 2f;    // how many seconds it takes
    public string nextScene = "";       // optional: scene to load after

    private bool isOpen = false;

    // Called by MushroomBoss on death
    public void OpenWall()
    {
        if (!isOpen)
        {
            isOpen = true;
            StartCoroutine(SlideUp());
        }
    }

    IEnumerator SlideUp()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position + new Vector3(0, slideDistance, 0);

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration); // smooth ease in/out
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        transform.position = endPos;

        // Optional: load next scene after wall fully opens
        if (!string.IsNullOrEmpty(nextScene))
        {
            yield return new WaitForSeconds(1f);
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }
    }
}