using UnityEngine;

public class MagneticObject : MonoBehaviour
{
    public MagneticPolarity polarity;

    [HideInInspector]
    public Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
}
