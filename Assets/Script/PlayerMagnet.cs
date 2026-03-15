using UnityEngine;

public class PlayerMagnet : MonoBehaviour
{
    public MagneticPolarity playerPolarity = MagneticPolarity.North;

    public float magneticForce = 10f;
    public float magneticRadius = 5f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwitchPolarity();
        }

        ApplyMagnetForce();
    }

    void SwitchPolarity()
    {
        if (playerPolarity == MagneticPolarity.North)
            playerPolarity = MagneticPolarity.South;
        else
            playerPolarity = MagneticPolarity.North;

        Debug.Log("Player polarity: " + playerPolarity);
    }

    void ApplyMagnetForce()
    {
        Collider2D[] objects = Physics2D.OverlapCircleAll(transform.position, magneticRadius);

        foreach (Collider2D obj in objects)
        {
            MagneticObject magnet = obj.GetComponent<MagneticObject>();

            if (magnet != null)
            {
                Vector2 direction = magnet.transform.position - transform.position;

                if (magnet.polarity == playerPolarity)
                {
                    magnet.rb.AddForce(direction.normalized * magneticForce);
                }
                else
                {
                    magnet.rb.AddForce(-direction.normalized * magneticForce);
                }
            }
        }
    }
}
