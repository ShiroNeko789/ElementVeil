using UnityEngine;
using System.Collections;

public class ExitWall : MonoBehaviour
{
    [Header("Boss")]
    public GameObject boss;

    [Header("Positions")]
    public Transform closedPosition;
    public Transform openPosition;

    [Header("Slide Settings")]
    public float slideDuration = 2f;

    private IBoss trackedBoss;
    private bool isOpen = false;

    void Start()
    {
        if (closedPosition != null)
            transform.position = closedPosition.position;

        if (boss != null)
        {
            trackedBoss = boss.GetComponent<IBoss>();
            if (trackedBoss == null)
                Debug.LogWarning("[ExitWall] Assigned boss has no IBoss component.");
        }
        else
        {
            Debug.LogWarning("[ExitWall] No boss assigned in Inspector.");
        }
    }

    void Update()
    {
        if (isOpen || trackedBoss == null) return;
        if (trackedBoss.IsDead()) OpenWall();
    }

    void OpenWall()
    {
        if (isOpen) return;
        isOpen = true;
        StartCoroutine(SlideToOpen());
    }

    IEnumerator SlideToOpen()
    {
        if (openPosition == null) yield break;

        Vector3 startPos = transform.position;
        Vector3 endPos = openPosition.position;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        transform.position = endPos;
    }
}