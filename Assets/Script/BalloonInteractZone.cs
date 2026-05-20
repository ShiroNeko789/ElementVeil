using UnityEngine;


public class BalloonInteractZone : MonoBehaviour
{
    public HotAirBalloon balloon;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) balloon?.OnPlayerEnter();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) balloon?.OnPlayerExit();
    }
}